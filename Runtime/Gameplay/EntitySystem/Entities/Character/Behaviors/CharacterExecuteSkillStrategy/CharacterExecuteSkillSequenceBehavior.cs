using System;
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
    /// This behavior executes a sequence of character's skills.
    /// Each skill is executed after the previous skill by a delay.<br/>
    /// There is no skill cooldown mechanic in this behavior.
    /// </summary>
    public class CharacterExecuteSkillSequenceBehavior : CharacterBehavior, IDisable
    {
        #region Members

        protected static string VfxHolderName = "vfx_holder";
        protected static string WarningSkillVFX = "warning_execute_skill_vfx";
        protected static string VfxTopPositionName = "top_position";
        protected Transform topPosition;
        protected int currentlyUsedSkillIndex;
        protected bool isNextSkillReady;
        protected List<float> nextSkillDelays;
        protected List<SkillModel> skillModels;
        protected List<ISkillStrategy> skillStrategies;
        protected CancellationTokenSource executeSkillCancellationTokenSource;
        protected CancellationTokenSource delayNextSkillCancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);

            var vfxHolder = transform.FindChildTransform(VfxHolderName);
            topPosition = vfxHolder.Find(VfxTopPositionName);

            var entitySkillSequenceData = model.GetEntityDistinctData<IEntitySkillSequenceData>();
            if (entitySkillSequenceData != null)
            {
                skillModels = entitySkillSequenceData.SkillModels;
                nextSkillDelays = entitySkillSequenceData.NextSkillDelays;
                if (skillModels != null && skillModels.Count > 0)
                {
                    skillStrategies = new List<ISkillStrategy>();
                    for (int i = 0; i < skillModels.Count; i++)
                    {
                        var skillModel = skillModels[i];
                        var skillStrategy = SkillStrategyFactory.GetSkillStrategy(skillModel.SkillType);
                        skillStrategy.Init(skillModel, ownerModel, ownerTransform);
                        skillStrategies.Add(skillStrategy);
                    }
                    isNextSkillReady = true;
                    currentlyUsedSkillIndex = 0;
                    ownerModel.ActionTriggeredEvent += OnActionTriggered;
                    ownerModel.HardCCImpactedEvent += OnHardCCImpacted;
                    return true;
                }
                else return false;
            }
            else return false;
        }

        public virtual void Disable()
        {
            executeSkillCancellationTokenSource?.Cancel();
            executeSkillCancellationTokenSource = null;
            delayNextSkillCancellationTokenSource?.Cancel();
            delayNextSkillCancellationTokenSource = null;

            foreach (var skillStrategy in skillStrategies)
                skillStrategy.Dispose();
        }

        protected virtual void OnActionTriggered(ActionInputType actionInputType)
        {
            if (actionInputType == ActionInputType.PrimarySkill ||
                actionInputType == ActionInputType.SecondarySkill ||
                actionInputType == ActionInputType.TertiarySkill)
            {
                if (isNextSkillReady)
                {
                    if (ownerModel.CheckCanUseSkill && !skillModels[currentlyUsedSkillIndex].IsUsing)
                        TriggerSkillExecution();
                }
            }
        }

        protected virtual void TriggerSkillExecution()
        {
            executeSkillCancellationTokenSource = new CancellationTokenSource();
            StartExecutingSkillAsync(executeSkillCancellationTokenSource.Token).Forget();
        }

        protected virtual async UniTaskVoid StartExecutingSkillAsync(CancellationToken cancellationToken)
        {
            skillModels[currentlyUsedSkillIndex].IsUsing = true;
            var canCastResult = await skillStrategies[currentlyUsedSkillIndex].CheckCanCast(cancellationToken);
            if (canCastResult.Result)
            {
                ownerModel.isPlayingSkill = true;
                ownerModel.SetMoveDirection(Vector2.zero);
                await SpawnWarningVFXAsync(cancellationToken);
                await skillStrategies[currentlyUsedSkillIndex].Execute(canCastResult.Data, cancellationToken);
                FinishSkill();
            }
            else skillModels[currentlyUsedSkillIndex].IsUsing = false;
        }

        protected virtual async UniTask SpawnWarningVFXAsync(CancellationToken cancellationToken)
        {
            var warningVFX = await PoolManager.Instance.Get(WarningSkillVFX, cancellationToken);
            warningVFX.transform.SetParent(topPosition);
            warningVFX.transform.localPosition = new Vector2(0, 0.3f);
        }

        protected virtual void OnHardCCImpacted(StatusEffectType statusEffectType)
        {
            if (skillModels[currentlyUsedSkillIndex].IsUsing)
            {
                if (skillModels[currentlyUsedSkillIndex].CanBeCanceled)
                {
                    executeSkillCancellationTokenSource.Cancel();
                    var executedSkillActionPhase = skillStrategies[currentlyUsedSkillIndex].Cancel();
                    if (executedSkillActionPhase == SkillActionPhase.Precheck)
                        FinishSkillAtPrecheck();
                    else
                        FinishSkill();
                }
            }
        }

        protected virtual void FinishSkillAtPrecheck()
        {
            skillModels[currentlyUsedSkillIndex].IsUsing = false;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
        }

        protected virtual void FinishSkill()
        {
            skillModels[currentlyUsedSkillIndex].IsUsing = false;
            delayNextSkillCancellationTokenSource = new CancellationTokenSource();
            ownerModel.isPlayingSkill = false;
            ownerModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustFinishedUseSkill);
            RunDelayForNextSkillAsync(nextSkillDelays[currentlyUsedSkillIndex], delayNextSkillCancellationTokenSource.Token).Forget();
        }

        protected virtual async UniTaskVoid RunDelayForNextSkillAsync(float delay, CancellationToken cancellationToken)
        {
            isNextSkillReady = false;
            currentlyUsedSkillIndex++;
            if (currentlyUsedSkillIndex == skillModels.Count)
                currentlyUsedSkillIndex = 0;
            ownerModel.SkillUsageChangedEvent.Invoke(currentlyUsedSkillIndex);
            await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
            isNextSkillReady = true;
        }

        #endregion Class Methods
    }
}