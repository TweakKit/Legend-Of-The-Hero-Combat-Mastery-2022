using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class FireRotatedScaledProjectileSkillStrategy : SkillStrategy<FireRotatedScaledProjectileSkillModel>
    {
        #region Class Methods

        protected override void InitActions(FireRotatedScaledProjectileSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            FireProjectileSkillAction fireProjectileTowardsTargetSkillAction = new FireProjectileSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(fireProjectileTowardsTargetSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            ProjectileStrategyData strategyData = new FlyForwardRotateIncreaseProjectileStrategyData(DamageSource.FromSkill,
                                                                                                     skillModel.ProjectileMoveDistance,
                                                                                                     skillModel.ProjectileMoveSpeed,
                                                                                                     skillModel.ProjectileRotateSpeed,
                                                                                                     skillModel.ProjectileRotateDegree,
                                                                                                     skillModel.ProjectileScaleSpeed,
                                                                                                     skillModel.NumberOfAttachedChildProjectiles,
                                                                                                     skillModel.AttachedChildProjectilePrefabName,
                                                                                                     skillModel.AttachedChildProjectileCenterOffsetDistance,
                                                                                                     skillModel.AttachedChildProjectileDamageFactors,
                                                                                                     skillModel.AttachedChildProjectileDamageBonus);
            fireProjectileTowardsTargetSkillAction.Init(creatorModel,
                                                        creatorTransform,
                                                        skillModel.SkillType,
                                                        skillModel.TargetType,
                                                        SkillActionPhase.Cast,
                                                        skillModel.ProjectilePrefabName,
                                                        new[] { strategyData });
        }

        #endregion Class Methods
    }
}