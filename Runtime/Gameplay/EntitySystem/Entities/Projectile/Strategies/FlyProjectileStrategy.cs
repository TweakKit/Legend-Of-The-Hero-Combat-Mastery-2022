using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public float moveDistance;
        public float moveSpeed;
        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] modifierModels;

        #endregion Members

        #region Class Methods

        public FlyProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType strategyType, float moveDistance, float moveSpeed,
                                        float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null)
            : base(damageSource, strategyType)
        {
            this.moveDistance = moveDistance;
            this.moveSpeed = moveSpeed;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.modifierModels = modifierModels;
        }

        #endregion Class Methods
    }

    public abstract class FlyProjectileStrategy<T> : ProjectileStrategy<T> where T : FlyProjectileStrategyData
    {
        #region Properties

        protected Vector2 currentDirection;
        protected Vector2 originalPosition;

        #endregion Properties

        #region Class Methods

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);
            this.originalPosition = originalPosition;
            this.currentDirection = direction;
        }

        public override void Update()
        {
            if (Vector2.SqrMagnitude(originalPosition - controllerProjectile.CenterPosition) > strategyData.moveDistance * strategyData.moveDistance)
                Complete(false, true);
        }

        public override void Collide(Collider2D collider)
        {
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                if (interactable != null && !interactable.Model.IsDead)
                {
                    if (controllerProjectile.CreatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                    {
                        var hitPoint = collider.ClosestPoint(controllerProjectile.CenterPosition);
                        var hitDirection = controllerProjectile.Direction;
                        HitTarget(interactable, hitPoint, hitDirection);
                    }
                }
                else Complete(false, true);
            }
            else Complete(false, true);
        }

        protected virtual void HitTarget(IInteractable target, Vector2 hitPoint, Vector2 hitDirection) { }

        #endregion Class Methods
    }
}