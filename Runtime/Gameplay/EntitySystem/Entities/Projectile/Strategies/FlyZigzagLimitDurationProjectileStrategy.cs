using System;
using Runtime.Definition;
using Runtime.Extensions;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyZigzagLimitDurationProjectileStrategyData : FlyLimitDurationProjectileStrategyData
    {
        #region Members

        public int numberOfHits;

        #endregion Members

        #region Class Methods

        public FlyZigzagLimitDurationProjectileStrategyData(int numberOfHits, DamageSource damageSource, ProjectileStrategyType strategyType, float moveDuration, float moveSpeed, bool rotateByDirection, float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null) : base(damageSource, strategyType, moveDuration, moveSpeed, rotateByDirection, damageBonus, damageFactors, modifierModels)
        {
            this.numberOfHits = numberOfHits;
        }

        #endregion Class Methods
    }

    public class FlyZigzagLimitDurationProjectileStrategy : FlyLimitDurationProjectileStrategy<FlyZigzagLimitDurationProjectileStrategyData>
    {
        #region Members

        protected int currentHit;

        #endregion Members

        #region Class Methods

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);
            currentHit = 0;
        }

        public override void Start()
        {
            base.Start();
            if (strategyData.rotateByDirection)
                controllerProjectile.UpdateRotation(currentDirection.ToQuaternion());
        }

        public override void Update()
        {
            controllerProjectile.UpdatePositionBySpeed(strategyData.moveSpeed, currentDirection);
            base.Update();
#if DEBUGGING
            DebugDrawLine();
#endif
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
                        CheckHit(collider);
                    }
                }
                else
                {
                    CheckHit(collider);
                }
            }
            else
            {
                CheckHit(collider);
            }
        }

        private void CheckHit(Collider2D collider)
        {
            currentHit++;
            if (currentHit >= strategyData.numberOfHits)
            {
                Complete(false, true);
            }
            else
            {
                CalculateNextDirection(currentDirection, collider);
            }
        }

        protected override void HitTarget(IInteractable target, Vector2 hitPoint, Vector2 hitDirection)
        {
            var damageInfo = controllerProjectile.CreatorModel.GetDamageInfo(strategyData.damageSource, strategyData.damageBonus, strategyData.damageFactors, strategyData.modifierModels, target.Model);
            target.GetHit(damageInfo, new DamageMetaData(hitDirection, controllerProjectile.CenterPosition));
            CreateImpactEffect(hitPoint);
        }

        private void CalculateNextDirection(Vector2 direction, Collider2D collider)
        {
            controllerProjectile.DisableCollide();

            var raycastHits = Physics2D.RaycastAll(controllerProjectile.CenterPosition, direction, 100);
            foreach (var raycastHit in raycastHits)
            {
                if (raycastHit.collider.gameObject.layer == Layers.OBSTACLE_LAYER && !raycastHit.collider.CompareTag(TagNames.OBSTACLE_NOT_BLOCK))
                {
                    var inComingVector = direction;
                    var normal = raycastHit.normal.normalized;
                    currentDirection = Vector2.Reflect(inComingVector, normal);
                    _currentDirection = direction;
                    _normalDirection = normal;
                    _reflectDirection = currentDirection;
                    _hitPoint = raycastHit.point;

                    break;
                }
                else
                {
                    var entity = raycastHit.collider.GetComponent<IEntity>();
                    if(entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && controllerProjectile.CreatorModel.EntityType.CanCauseDamage(interactable.Model.EntityType))
                        {
                            var inComingVector = direction;
                            var normal = raycastHit.normal.normalized;
                            var updatedDirection = Vector2.Reflect(inComingVector, normal);
                            currentDirection = updatedDirection;
                            _currentDirection = direction;
                            _normalDirection = normal;
                            _reflectDirection = currentDirection;
                            _hitPoint = raycastHit.point;

                            break;
                        }
                    }
                }
            }
            if (strategyData.rotateByDirection)
                controllerProjectile.UpdateRotation(direction.ToQuaternion());

            controllerProjectile.EnableCollide();
        }

        private Vector2 _currentDirection;
        private Vector2 _normalDirection;
        private Vector2 _reflectDirection;
        private Vector2 _hitPoint;

        private void DebugDrawLine()
        {
            Debug.DrawLine(_hitPoint - _currentDirection * 100, _hitPoint, Color.red);
            Debug.DrawLine(_hitPoint, _hitPoint + _normalDirection * 100, Color.blue);
            Debug.DrawLine(_hitPoint, _hitPoint + _reflectDirection * 100, Color.green);
        }

        #endregion Class Methods
    }
}