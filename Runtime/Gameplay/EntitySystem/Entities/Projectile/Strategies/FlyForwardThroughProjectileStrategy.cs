using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyForwardThroughProjectileStrategyData : FlyProjectileStrategyData
    {
        #region Class Methods

        public FlyForwardThroughProjectileStrategyData(DamageSource damageSource, float lifeRange, float speed, float damageBonus = 0,
                                                       DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, ProjectileStrategyType.ForwardThrough, lifeRange, speed, damageBonus, damageFactors, damageModifierModels) { }

        #endregion Class Methods
    }

    public class FlyForwardThroughProjectileStrategy : FlyForwardProjectileStrategy<FlyForwardThroughProjectileStrategyData>
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