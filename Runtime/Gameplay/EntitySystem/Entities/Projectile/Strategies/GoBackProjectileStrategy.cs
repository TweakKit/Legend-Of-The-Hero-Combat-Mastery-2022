using UnityEngine;
using Runtime.Extensions;
using System;

namespace Runtime.Gameplay.EntitySystem
{
    public class GoBackProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public Transform goBackTransform;
        public float speed;
        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] damageModifierModels;
        public Action onWentBackAction;
        public Func<bool> canComplete;
        public bool rotateByDirection;

        #endregion Members

        #region Class Methods

        public GoBackProjectileStrategyData(Transform goBackTransform, DamageSource damageSource, float speed, bool rotateByDirection, float damageBonus,
                                            DamageFactor[] damageFactors, StatusEffectModel[] damageModifierModels, Action onWentBackAction, Func<bool> canComplete)
            : base(damageSource, ProjectileStrategyType.GoBack)
        {
            this.goBackTransform = goBackTransform;
            this.speed = speed;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.damageModifierModels = damageModifierModels;
            this.onWentBackAction = onWentBackAction;
            this.rotateByDirection = rotateByDirection;
            this.canComplete = canComplete;
        }

        #endregion Class Methods
    }

    public class GoBackProjectileStrategy : ProjectileStrategy<GoBackProjectileStrategyData>
    {
        #region Members

        private const float TIME_COUNT = 0.06f;
        private const float DISTANCE_THRESHOLD = 0.2f;
        private float _currentTime = 0;
        private Vector2 _direction;

        #endregion Members

        #region Class Methods

        public override void Update()
        {
            if (controllerProjectile.CreatorModel == null || controllerProjectile.CreatorModel.IsDead)
            {
                Complete(false, true);
                return;
            }

            if (Vector2.SqrMagnitude((Vector2)strategyData.goBackTransform.position - controllerProjectile.CenterPosition) > (strategyData.speed * _direction.normalized * Time.deltaTime).sqrMagnitude + DISTANCE_THRESHOLD)
            {
                controllerProjectile.UpdatePositionBySpeed(strategyData.speed, _direction);
                if (_currentTime <= 0)
                {
                    _currentTime = TIME_COUNT;
                    _direction = ((Vector2)strategyData.goBackTransform.position - controllerProjectile.CenterPosition).normalized;

                    if(strategyData.rotateByDirection)
                        controllerProjectile.UpdateRotation(_direction.ToQuaternion());
                }
                else _currentTime -= Time.deltaTime;
            }
            else
            {
                strategyData.onWentBackAction?.Invoke();
                Complete(false, false);

                if (Vector2.SqrMagnitude((Vector2)strategyData.goBackTransform.position - controllerProjectile.CenterPosition) > Mathf.Epsilon)
                {
                    var moveToPosition = Vector2.MoveTowards(controllerProjectile.CenterPosition, strategyData.goBackTransform.position, Time.deltaTime * strategyData.speed);
                    controllerProjectile.UpdatePosition(moveToPosition);
                }
            }
        }

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
                        Complete(false, true);
                        CreateImpactEffect(hitPoint);
                    }
                }
                else Complete(false, true);
            }
            else Complete(false, true);
        }

        public override void Complete(bool forceComplete, bool displayImpact)
        {
            if(strategyData.canComplete == null || strategyData.canComplete.Invoke())
                base.Complete(forceComplete, displayImpact);
        }

        #endregion Class Methods
    }
}