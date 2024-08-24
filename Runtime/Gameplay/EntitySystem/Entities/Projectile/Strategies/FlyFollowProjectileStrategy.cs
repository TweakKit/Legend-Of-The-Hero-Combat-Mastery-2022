using UnityEngine;
using Runtime.Extensions;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyFollowProjectileStrategyData : FollowProjectileStrategyData
    {
        #region Class Methods

        public FlyFollowProjectileStrategyData(DamageSource damageSource, float offsetDegree, float moveDistance, float moveSpeed, float damageBonus = 0,
                                               DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, ProjectileStrategyType.Follow, offsetDegree, moveDistance, moveSpeed, damageBonus, damageFactors, damageModifierModels) { }

        #endregion Class Methods
    }

    public abstract class FollowProjectileStrategyData : FlyProjectileStrategyData
    {
        #region Members

        public float offsetDegree;

        #endregion Members

        #region Class Methods

        public FollowProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType projectileStrategyType, float offsetDegree, float moveDistance, float moveSpeed,
                                            float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] damageModifierModels = null)
            : base(damageSource, projectileStrategyType, moveDistance, moveSpeed, damageBonus, damageFactors, damageModifierModels)
            => this.offsetDegree = offsetDegree;

        #endregion Class Methods
    }

    public class FlyFollowProjectileStrategy : FlyFollowProjectileStrategy<FlyFollowProjectileStrategyData> { }

    public abstract class FlyFollowProjectileStrategy<T> : FlyProjectileStrategy<T> where T : FollowProjectileStrategyData
    {
        #region Members

        protected const float TIME_DELAY = 0.15f;
        protected float currentTime = 0;
        protected EntityModel targetModel;

        #endregion Members

        #region Class Methods

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);
            this.targetModel = targetModel;
        }

        public override void Start()
        {
            currentTime = 0;
            controllerProjectile.UpdateRotation(currentDirection.ToQuaternion());
        }

        public override void Update()
        {
            var moveVector = currentDirection * strategyData.moveSpeed * Time.deltaTime;
            if (Vector2.SqrMagnitude(controllerProjectile.CenterPosition - targetModel.Position) <= moveVector.sqrMagnitude)
            {
                Complete(false, true);
                return;
            }

            controllerProjectile.UpdatePositionBySpeed(strategyData.moveSpeed, currentDirection);

            if (currentTime > TIME_DELAY)
            {
                currentTime = 0;
                if (targetModel != null && !targetModel.IsDead)
                {
                    var currentAngle = CalculateCurrentDegree();
                    if (Mathf.Abs(currentAngle) >= strategyData.offsetDegree)
                    {
                        var offset = currentAngle > 0 ? strategyData.offsetDegree : -strategyData.offsetDegree;
                        currentDirection = Quaternion.AngleAxis(offset, controllerProjectile.Direction) * currentDirection;
                    }
                    else currentDirection = (targetModel.Position - controllerProjectile.CenterPosition).normalized;
                    controllerProjectile.UpdateRotation(currentDirection.ToQuaternion());
                }
            }
            else currentTime += Time.deltaTime;
            base.Update();
        }

        private float CalculateCurrentDegree()
            => Vector2.SignedAngle(currentDirection, targetModel.Position - controllerProjectile.CenterPosition);

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