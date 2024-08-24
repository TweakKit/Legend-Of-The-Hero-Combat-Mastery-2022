using System.Linq;
using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior executes a sequence of character's skills along with an additinal skill.
    /// The additional skill can be played with other skills simultaneously or just played as a skill in the sequence.<br/>
    /// There is no skill cooldown mechanic in this behavior.
    /// Note: For now, just only one additional skill is used with the sequence.
    /// </summary>
    public sealed class CharacterExecuteSkillSequenceWithAdditionalSkillBehavior : CharacterExecuteSkillSequenceBehavior
    {
        #region Members

        private SkillModel _additionalSkillModel;
        private float _additionalNextSkillDelay;
        private ISkillStrategy _additionalSkillStrategy;
        private bool _useSimultaneouslyWithOthers;
        private int[] _simultaneousSkillIndexes;
        private bool _hasRunAdditionalSkill;
        private bool _hasSimultaneousSkillFinished;
        private bool _hasAdditionalSkillFinished;
        private CancellationTokenSource _executeAdditionalSkillCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            if (base.InitModel(model, transform))
            {
                var entityAdditionalSkillSequenceData = model.GetEntityDistinctData<IEntityAdditionalSkillSequenceData>();
                if (entityAdditionalSkillSequenceData != null)
                {
                    _additionalSkillModel = entityAdditionalSkillSequenceData.AdditionalSkillModel;
                    _additionalNextSkillDelay = entityAdditionalSkillSequenceData.AdditionalNextSkillDelay;
                    _additionalSkillStrategy = SkillStrategyFactory.GetSkillStrategy(_additionalSkillModel.SkillType);
                    _additionalSkillStrategy.Init(_additionalSkillModel, ownerModel, ownerTransform);
                    _useSimultaneouslyWithOthers = entityAdditionalSkillSequenceData.UseSimultaneouslyWithOthers;
                    if (!_useSimultaneouslyWithOthers)
                    {
                        _simultaneousSkillIndexes = null;
                        skillStrategies.Add(_additionalSkillStrategy);
                        skillModels.Add(_additionalSkillModel);
                        nextSkillDelays.Add(_additionalNextSkillDelay);
                    }
                    else _simultaneousSkillIndexes = entityAdditionalSkillSequenceData.SimultaneousSkillIndexes;
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public override void Disable()
        {
            _additionalSkillStrategy.Dispose();
            _executeAdditionalSkillCancellationTokenSource?.Cancel();
            _executeAdditionalSkillCancellationTokenSource = null;
            base.Disable();
        }

        protected override void TriggerSkillExecution()
        {
            base.TriggerSkillExecution();
            _hasRunAdditionalSkill = false;
            if (_useSimultaneouslyWithOthers)
            {
                if (_simultaneousSkillIndexes.Any(x => x == currentlyUsedSkillIndex))
                {
                    _hasRunAdditionalSkill = true;
                    _hasSimultaneousSkillFinished = false;
                    _hasAdditionalSkillFinished = false;
                    _executeAdditionalSkillCancellationTokenSource = new CancellationTokenSource();
                    StartExecutingAdditionalSkillAsync(_executeAdditionalSkillCancellationTokenSource.Token).Forget();
                }
                else _hasRunAdditionalSkill = false;
            }
        }

        protected override void OnHardCCImpacted(StatusEffectType statusEffectType)
        {
            if (_useSimultaneouslyWithOthers)
            {
                base.OnHardCCImpacted(statusEffectType);
                if (_hasRunAdditionalSkill && _additionalSkillModel.IsUsing)
                {
                    if (_additionalSkillModel.CanBeCanceled)
                    {
                        _executeAdditionalSkillCancellationTokenSource.Cancel();
                        var executedSkillActionPhase = _additionalSkillStrategy.Cancel();
                        if (executedSkillActionPhase == SkillActionPhase.Precheck)
                            FinishAdditionalSkillAtPrecheck();
                        else
                            FinishAdditionalSkill();
                    }
                }
            }
            else base.OnHardCCImpacted(statusEffectType);
        }

        protected override void FinishSkill()
        {
            if (_useSimultaneouslyWithOthers && _hasRunAdditionalSkill)
            {
                _hasSimultaneousSkillFinished = true;
                skillModels[currentlyUsedSkillIndex].IsUsing = false;
                ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
                if (_hasAdditionalSkillFinished)
                {
                    delayNextSkillCancellationTokenSource = new CancellationTokenSource();
                    ownerModel.isPlayingSkill = false;
                    RunDelayForNextSkillAsync(nextSkillDelays[currentlyUsedSkillIndex], delayNextSkillCancellationTokenSource.Token).Forget();
                }
            }
            else base.FinishSkill();
        }

        private async UniTaskVoid StartExecutingAdditionalSkillAsync(CancellationToken cancellationToken)
        {
            _additionalSkillModel.IsUsing = true;
            var canCastResult = await _additionalSkillStrategy.CheckCanCast(cancellationToken);
            if (canCastResult.Result)
            {
                ownerModel.isPlayingSkill = true;
                ownerModel.SetMoveDirection(Vector2.zero);
                await SpawnWarningVFXAsync(cancellationToken);
                await _additionalSkillStrategy.Execute(canCastResult.Data, cancellationToken);
                FinishAdditionalSkill();
            }
            else _additionalSkillModel.IsUsing = false;
        }

        private void FinishAdditionalSkillAtPrecheck()
        {
            _additionalSkillModel.IsUsing = false;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
        }

        private void FinishAdditionalSkill()
        {
            _hasAdditionalSkillFinished = true;
            _additionalSkillModel.IsUsing = false;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
            if (_hasSimultaneousSkillFinished)
            {
                delayNextSkillCancellationTokenSource = new CancellationTokenSource();
                ownerModel.isPlayingSkill = false;
                RunDelayForNextSkillAsync(_additionalNextSkillDelay, delayNextSkillCancellationTokenSource.Token).Forget();
            }
        }

        #endregion Class Methods
    }
}