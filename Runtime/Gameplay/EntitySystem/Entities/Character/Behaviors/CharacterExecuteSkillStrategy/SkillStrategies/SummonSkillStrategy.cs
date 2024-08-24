using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SummonSkillStrategy : SkillStrategy<SummonSkillModel>
    {
        #region Class Methods

        protected override void InitActions(SummonSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            SummonSkillAction summonSkillAction = new SummonSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(summonSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        Definition.SkillActionPhase.Precheck);

            summonSkillAction.Init(creatorModel,
                                   creatorTransform,
                                   skillModel.SkillType,
                                   skillModel.TargetType,
                                   SkillActionPhase.Cast,
                                   skillModel.SummonedSpawnEntitiesInfo,
                                   skillModel.UseOwnerLevel,
                                   skillModel.SummonedCenterOffsetDistance
                                   );
        }

        #endregion Class Methods
    }
}