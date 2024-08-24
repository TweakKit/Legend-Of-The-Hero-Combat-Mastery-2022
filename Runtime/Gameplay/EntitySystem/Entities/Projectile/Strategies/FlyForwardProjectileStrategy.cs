using UnityEngine;
using Runtime.Extensions;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyForwardProjectileStrategyData : FlyProjectileStrategyData
    {
        #region Class Methods

        public FlyForwardProjectileStrategyData(DamageSource damageSource, float moveDistance, float moveSpeed, float damageBonus = 0,
                                                DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, ProjectileStrategyType.Forward, moveDistance, moveSpeed, damageBonus, damageFactors, damageModifierModels) { }

        #endregion Class Methods
    }

    public class FlyForwardProjectileStrategy : FlyForwardProjectileStrategy<FlyForwardProjectileStrategyData> { }

    public abstract class FlyForwardProjectileStrategy<T> : FlyProjectileStrategy<T> where T : FlyProjectileStrategyData
    {
        #region Class Methods

        public override void Start()
        {
            base.Start();
            controllerProjectile.UpdateRotation(currentDirection.ToQuaternion());
        }

        public override void Update()
        {
            controllerProjectile.UpdatePositionBySpeed(strategyData.moveSpeed, currentDirection);
            base.Update();
        }

        protected override void HitTarget(IInteractable target, Vector2 hitPoint, Vector2 hitDirection)
        {
            var damageInfo = controllerProjectile.CreatorModel.GetDamageInfo(strategyData.damageSource, strategyData.damageBonus, strategyData.damageFactors, strategyData.modifierModels, target.Model);
            target.GetHit(damageInfo, new DamageMetaData(hitDirection, controllerProjectile.CenterPosition));
            CreateImpactEffect(hitPoint);
            Complete(false, true);
        }

        #endregion Class Methods
    }
}