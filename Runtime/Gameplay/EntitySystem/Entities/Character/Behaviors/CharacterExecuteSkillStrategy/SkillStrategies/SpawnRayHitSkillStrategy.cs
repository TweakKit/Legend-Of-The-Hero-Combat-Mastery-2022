using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnRayHitSkillStrategy : SkillStrategy<SpawnRayHitSkillModel>
    {
        #region Class Methods

        protected override void InitActions(SpawnRayHitSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            SpawnRayHitSkillAction spawnRayHitSkillAction = new SpawnRayHitSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(spawnRayHitSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            spawnRayHitSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Cast,
                                        skillModel.RayRange,
                                        skillModel.NumberOfRays,
                                        skillModel.CanRotateRays,
                                        skillModel.DelayBeforeRotateRays,
                                        skillModel.RotateRaysSpeed,
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