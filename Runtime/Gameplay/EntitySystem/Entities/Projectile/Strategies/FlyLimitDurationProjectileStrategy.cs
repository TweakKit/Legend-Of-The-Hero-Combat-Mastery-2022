using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyLimitDurationProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public float moveSpeed;
        public float moveDuration;
        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] modifierModels;
        public bool rotateByDirection;

        #endregion Members

        #region Class Methods

        public FlyLimitDurationProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType strategyType, float moveDuration, float moveSpeed, bool rotateByDirection,
                                        float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null)
            : base(damageSource, strategyType)
        {
            this.moveDuration = moveDuration;
            this.moveSpeed = moveSpeed;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.modifierModels = modifierModels;
            this.rotateByDirection = rotateByDirection;
        }

        #endregion Class Methods
    }

    public class FlyLimitDurationProjectileStrategy<T> : ProjectileStrategy<T> where T : FlyLimitDurationProjectileStrategyData
    {
        #region Members

        protected Vector2 currentDirection;
        protected Vector2 originalPosition;
        protected float currentFlyTime;

        #endregion Members

        #region Class Methods

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);
            this.originalPosition = originalPosition;
            this.currentDirection = direction;
            currentFlyTime = 0;
        }

        public override void Update()
        {
            currentFlyTime += Time.deltaTime;
            if(currentFlyTime >= strategyData.moveDuration)
            {
                Complete(false, true);
            }
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