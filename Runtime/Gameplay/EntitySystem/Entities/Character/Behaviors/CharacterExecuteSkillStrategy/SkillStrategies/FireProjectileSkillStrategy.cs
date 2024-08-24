using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class FireProjectileSkillStrategy : SkillStrategy<FireProjectileSkillModel>
    {
        #region Class Methods

        protected override void InitActions(FireProjectileSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            FireProjectileSkillAction fireProjectileSkillAction = new FireProjectileSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(fireProjectileSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            FlyProjectileStrategyData flyProjectileStrategyData = null;
            switch (skillModel.ProjectileStrategyType)
            {
                case ProjectileStrategyType.Follow:
                    flyProjectileStrategyData = new FlyFollowProjectileStrategyData(DamageSource.FromSkill,
                                                                                    Constants.PROJECTILE_STEERING_ANGLE,
                                                                                    skillModel.ProjectileMoveDistance,
                                                                                    skillModel.ProjectileMoveSpeed,
                                                                                    skillModel.ProjectileDamageBonus,
                                                                                    skillModel.ProjectileDamageFactors,
                                                                                    skillModel.ProjectileDamageModifierModels);
                    break;

                case ProjectileStrategyType.FollowThrough:
                    flyProjectileStrategyData = new FlyFollowThroughProjectileStrategyData(DamageSource.FromSkill,
                                                                                           Constants.PROJECTILE_STEERING_ANGLE,
                                                                                           skillModel.ProjectileMoveDistance,
                                                                                           skillModel.ProjectileMoveSpeed,
                                                                                           skillModel.ProjectileDamageBonus,
                                                                                           skillModel.ProjectileDamageFactors,
                                                                                           skillModel.ProjectileDamageModifierModels);
                    break;

                default:
                    flyProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromSkill,
                                                                                     skillModel.ProjectileMoveDistance,
                                                                                     skillModel.ProjectileMoveSpeed,
                                                                                     skillModel.ProjectileDamageBonus,
                                                                                     skillModel.ProjectileDamageFactors,
                                                                                     skillModel.ProjectileDamageModifierModels);
                    break;
            }

            fireProjectileSkillAction.Init(creatorModel,
                                           creatorTransform,
                                           skillModel.SkillType,
                                           skillModel.TargetType,
                                           SkillActionPhase.Cast,
                                           skillModel.ProjectileId,
                                           new[] { flyProjectileStrategyData },
                                           skillModel.NumberOfProjectiles,
                                           skillModel.TimeDelayBetweenProjectiles,
                                           skillModel.CanFireTowardsTarget,
                                           skillModel.FireDeflectionAngle,
                                           skillModel.NumberOfBulletsForEachProjectile,
                                           skillModel.AngleBetweenBulletForEachProjectile);
        }

        #endregion Class Methods
    }
}