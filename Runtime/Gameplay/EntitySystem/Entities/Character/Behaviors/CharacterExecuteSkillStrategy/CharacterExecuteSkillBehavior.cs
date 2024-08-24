using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior executes character's skills.
    /// Each skill is executed separately by their corresponding skill input type.
    /// </summary>
    public sealed class CharacterExecuteSkillBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private static string s_vfxHolderName = "vfx_holder";
        private static string s_warningSkillVFX = "warning_execute_skill_vfx";
        private static string s_vfxTopPositionName = "top_position";
        private Transform _topPosition;
        private int _currentlyUsedSkillIndex;
        private List<SkillModel> _skillModels;
        private List<ISkillStrategy> _skillStrategies;
        private CancellationTokenSource _executeSkillCancellationTokenSource;
        private CancellationTokenSource[] _skillCooldownCancellationTokenSources;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);

            var vfxHolder = transform.FindChildTransform(s_vfxHolderName);
            _topPosition = vfxHolder.Find(s_vfxTopPositionName);

            _skillModels = ownerModel.SkillModels;
            if (_skillModels != null && _skillModels.Count > 0)
            {
                var cooldownReduction = ownerModel.GetTotalStatValue(StatType.CooldownReduction);
                _currentlyUsedSkillIndex = 0;
                _skillCooldownCancellationTokenSources = new CancellationTokenSource[_skillModels.Count];
                _skillStrategies = new List<ISkillStrategy>();

                for (int i = 0; i < _skillModels.Count; i++)
                {
                    var skillModel = _skillModels[i];
                    skillModel.Cooldown = skillModel.Cooldown * (1 - cooldownReduction);
                    var skillStrategy = SkillStrategyFactory.GetSkillStrategy(skillModel.SkillType);
                    skillStrategy.Init(skillModel, ownerModel, ownerTransform);
                    _skillStrategies.Add(skillStrategy);
                }

                ownerModel.ActionTriggeredEvent += OnActionTriggered;
                ownerModel.HardCCImpactedEvent += OnHardCCImpacted;
                return true;
            }
            else return false;
        }

        public void Disable()
        {
            _executeSkillCancellationTokenSource?.Cancel();
            _executeSkillCancellationTokenSource = null;

            foreach (var skillStrategy in _skillStrategies)
                skillStrategy.Dispose();

            foreach (var skillCooldownCancellationTokenSource in _skillCooldownCancellationTokenSources)
                skillCooldownCancellationTokenSource?.Cancel();
        }

        private void OnActionTriggered(ActionInputType actionInputType)
        {
            if (actionInputType == ActionInputType.PrimarySkill ||
                actionInputType == ActionInputType.SecondarySkill ||
                actionInputType == ActionInputType.TertiarySkill)
            {
                var skillIndex = actionInputType.GetSkillIndex();
                if (_skillModels[skillIndex] != null && _skillModels[skillIndex].IsReady)
                {
                    if (ownerModel.CheckCanUseSkill && !_skillModels[skillIndex].IsUsing)
                    {
                        _currentlyUsedSkillIndex = skillIndex;
                        TriggerSkillExecution();
                    }
                }
            }
        }

        private void TriggerSkillExecution()
        {
            _executeSkillCancellationTokenSource = new CancellationTokenSource();
            StartExecutingSkillAsync(_executeSkillCancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid StartExecutingSkillAsync(CancellationToken cancellationToken)
        {
            _skillModels[_currentlyUsedSkillIndex].IsUsing = true;
            var canCastResult = await _skillStrategies[_currentlyUsedSkillIndex].CheckCanCast(cancellationToken);
            if (canCastResult.Result)
            {
                ownerModel.isPlayingSkill = true;
                ownerModel.SetMoveDirection(Vector2.zero);
                await SpawnWarningVFXAsync(cancellationToken);
                await _skillStrategies[_currentlyUsedSkillIndex].Execute(canCastResult.Data, cancellationToken);
                FinishSkill();
            }
            else
            {
                _skillModels[_currentlyUsedSkillIndex].IsUsing = false;
                _currentlyUsedSkillIndex = -1;
            }
        }

        private async UniTask SpawnWarningVFXAsync(CancellationToken cancellationToken)
        {
            var warningVFX = await PoolManager.Instance.Get(s_warningSkillVFX, cancellationToken);
            warningVFX.transform.SetParent(_topPosition);
            warningVFX.transform.localPosition = new Vector2(0, 0.3f);
        }

        private void OnHardCCImpacted(StatusEffectType statusEffectType)
        {
            if (_currentlyUsedSkillIndex != -1 && _skillModels[_currentlyUsedSkillIndex].IsUsing)
            {
                if (_skillModels[_currentlyUsedSkillIndex].CanBeCanceled)
                {
                    _executeSkillCancellationTokenSource.Cancel();
                    var executedSkillActionPhase = _skillStrategies[_currentlyUsedSkillIndex].Cancel();
                    if (executedSkillActionPhase == SkillActionPhase.Precheck)
                        FinishSkillAtPrecheck();
                    else
                        FinishSkill();
                }
            }
        }

        private void FinishSkillAtPrecheck()
        {
            _skillModels[_currentlyUsedSkillIndex].IsUsing = false;
            _currentlyUsedSkillIndex = -1;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
        }

        private void FinishSkill()
        {
            _skillModels[_currentlyUsedSkillIndex].CurrentCooldown = _skillModels[_currentlyUsedSkillIndex].Cooldown;
            var cancellationTokenSource = new CancellationTokenSource();
            RunCountdownSkillAsync(_skillModels[_currentlyUsedSkillIndex], cancellationTokenSource.Token).Forget();
            _skillCooldownCancellationTokenSources[_currentlyUsedSkillIndex] = cancellationTokenSource;
            _skillModels[_currentlyUsedSkillIndex].IsUsing = false;
            _currentlyUsedSkillIndex = -1;
            ownerModel.isPlayingSkill = false;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
        }

        private async UniTaskVoid RunCountdownSkillAsync(SkillModel skillModel, CancellationToken cancellationToken)
        {
            while (skillModel.CurrentCooldown > 0)
            {
                skillModel.CurrentCooldown -= Time.deltaTime;
                await UniTask.Yield(cancellationToken);
            }
        }

        #endregion Class Methods
    }
}