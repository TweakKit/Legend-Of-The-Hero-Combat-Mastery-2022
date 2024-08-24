using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class GoBackThroughProjectileStrategy : GoBackProjectileStrategy
    {
        public override void Collide(Collider2D collider)
        {
            var hitPoint = collider.ClosestPoint(controllerProjectile.CenterPosition);
            var hitDirection = hitPoint - controllerProjectile.CenterPosition;
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                if (interactable != null && !interactable.Model.IsDead)
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
    }
}