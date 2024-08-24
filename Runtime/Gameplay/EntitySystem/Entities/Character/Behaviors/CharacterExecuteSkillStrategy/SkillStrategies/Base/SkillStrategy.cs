using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class SkillStrategy<T> : ISkillStrategy where T : SkillModel
    {
        #region Members

        protected SkillActionPhase currentSkillActionPhase;
        protected ISkillAction currentSkillAction;
        protected List<ISkillAction> skillActions;
        protected CharacterModel creatorModel;
        protected Transform creatorTransform;

        #endregion Members

        #region Class Methods

        public void Init(SkillModel skillModel, CharacterModel creatorModel, Transform creatorTransform)
        {
            this.creatorModel = creatorModel;
            this.creatorTransform = creatorTransform;
            skillActions = new List<ISkillAction>();
            InitActions(skillModel as T);
        }

        protected abstract void InitActions(T skillModel);

        public async UniTask<SkillTriggerResult> CheckCanCast(CancellationToken cancellationToken)
        {
            SkillActionData skillActionData = new SkillActionData();
            foreach (var skillAction in skillActions)
            {
                if (skillAction.SkillActionPhase != SkillActionPhase.Precheck)
                    return new SkillTriggerResult(true, skillActionData);

                currentSkillActionPhase = skillAction.SkillActionPhase;
                currentSkillAction = skillAction;
                skillActionData = await skillAction.RunOperateAsync(cancellationToken, skillActionData);
                if (!skillActionData.result)
                    return new SkillTriggerResult(false);
            }
            return new SkillTriggerResult(true, skillActionData);
        }

        public async UniTask Execute(SkillActionData skillActionData, CancellationToken cancellationToken)
        {
            foreach (var skillAction in skillActions)
            {
                if (skillAction.SkillActionPhase != SkillActionPhase.Precheck)
                {
                    currentSkillActionPhase = skillAction.SkillActionPhase;
                    currentSkillAction = skillAction;
                    skillActionData = await skillAction.RunOperateAsync(cancellationToken, skillActionData);
                    if (!skillActionData.result)
                        return;
                }
            }
        }

        public SkillActionPhase Cancel()
        {
            currentSkillAction?.Cancel();
            return currentSkillActionPhase;
        }

        public void Dispose()
        {
            foreach (var skillAction in skillActions)
                skillAction.Cancel();
        }

        #endregion Class Methods
    }
}