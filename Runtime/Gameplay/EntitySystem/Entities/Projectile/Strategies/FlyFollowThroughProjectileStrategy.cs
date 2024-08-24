using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyFollowThroughProjectileStrategyData : FollowProjectileStrategyData
    {
        #region Class Methods

        public FlyFollowThroughProjectileStrategyData(DamageSource damageSource, float offsetDegree, float moveDistance, float moveSpeed, float damageBonus = 0,
                                                      DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, ProjectileStrategyType.FollowThrough, offsetDegree, moveDistance, moveSpeed, damageBonus, damageFactors, damageModifierModels) { }

        #endregion Class Methods
    }

    public class FlyFollowThroughProjectileStrategy : FlyFollowProjectileStrategy<FlyFollowThroughProjectileStrategyData>
    {
        #region Class Methods

        protected override void HitTarget(IInteractable target, Vector2 hitPoint, Vector2 hitDirection)
        {
            var damageInfo = controllerProjectile.CreatorModel.GetDamageInfo(strategyData.damageSource, strategyData.damageBonus, strategyData.damageFactors, strategyData.modifierModels, target.Model);
            target.GetHit(damageInfo, new DamageMetaData(hitDirection, controllerProjectile.CenterPosition));
            CreateImpactEffect(hitPoint);
        }

        #endregion Class Methods
    }
}