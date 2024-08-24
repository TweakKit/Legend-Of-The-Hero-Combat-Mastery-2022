using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class IdleThroughProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] damageModifierModels;
        public Transform parentController;

        #endregion Members

        #region Class Methods

        public IdleThroughProjectileStrategyData(DamageSource damageSource, Transform parentController, float damageBonus = 0,
                                                 DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, ProjectileStrategyType.IdleThrough)
        {
            this.parentController = parentController;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.damageModifierModels = damageModifierModels;
        }

        #endregion Class Methods
    }

    public class IdleThroughProjectileStrategy : ProjectileStrategy<IdleThroughProjectileStrategyData>
    {
        #region Class Methods

        public override void Collide(Collider2D collider)
        {
            var hitPoint = collider.ClosestPoint(controllerProjectile.CenterPosition);
            var hitDirection = strategyData.parentController ? hitPoint - (Vector2)strategyData.parentController.position : hitPoint - controllerProjectile.CenterPosition;
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                if (interactable != null)
                {
                    if (controllerProjectile.CreatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                    {
                        var damageInfo = controllerProjectile.CreatorModel.GetDamageInfo(strategyData.damageSource, strategyData.damageBonus, strategyData.damageFactors, strategyData.damageModifierModels, interactable.Model);
                        interactable.GetHit(damageInfo, new DamageMetaData(hitDirection, controllerProjectile.CenterPosition));
                        CreateImpactEffect(hitPoint);
                    }
                }
            }
        }

        #endregion Class Methods
    }
}