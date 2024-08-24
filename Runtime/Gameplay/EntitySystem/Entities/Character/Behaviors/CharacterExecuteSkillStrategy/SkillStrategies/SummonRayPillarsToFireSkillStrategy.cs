using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SummonRayPillarsToFireSkillStrategy : SkillStrategy<SummonRayPillarsToFireSkillModel>
    {
        #region Class Methods

        protected override void InitActions(SummonRayPillarsToFireSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            SummonRayPillarsToFireSkillAction summonRayPillarsToFireSkillAction = new SummonRayPillarsToFireSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(summonRayPillarsToFireSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            summonRayPillarsToFireSkillAction.Init(creatorModel,
                                                   creatorTransform,
                                                   skillModel.SkillType,
                                                   skillModel.TargetType,
                                                   SkillActionPhase.Cast,
                                                   skillModel.RayRange,
                                                   skillModel.NumberOfRaysPerPillar,
                                                   skillModel.DisplayWarningIndicatorDuration,
                                                   skillModel.RayFireHitDuration,
                                                   skillModel.RayPrefabName,
                                                   skillModel.TargetGetDamagedDelay,
                                                   skillModel.RayDamageBonus,
                                                   skillModel.RayDamageFactors,
                                                   skillModel.RayDamageModifierModels);
        }

        #endregion Class Methods
    }
}