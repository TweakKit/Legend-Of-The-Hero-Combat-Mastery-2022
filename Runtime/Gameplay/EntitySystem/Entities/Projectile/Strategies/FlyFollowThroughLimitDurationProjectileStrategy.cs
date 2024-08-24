using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyFollowThroughLimitDurationProjectileStrategyData : FollowLimitDurationProjectileStrategyData
    {
        #region Class Methods

        public FlyFollowThroughLimitDurationProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType strategyType, float offsetDegree,  bool forceFollow, float moveDuration, float moveSpeed, bool rotateByDirection, float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null) 
            : base(damageSource, strategyType, offsetDegree, forceFollow, moveDuration, moveSpeed, rotateByDirection, damageBonus, damageFactors, modifierModels)
        {
        }

        #endregion Class Methods
    }

    public class FlyFollowThroughLimitDurationProjectileStrategy : FlyFollowLimitDurationProjectileStrategy<FlyFollowThroughLimitDurationProjectileStrategyData>
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
