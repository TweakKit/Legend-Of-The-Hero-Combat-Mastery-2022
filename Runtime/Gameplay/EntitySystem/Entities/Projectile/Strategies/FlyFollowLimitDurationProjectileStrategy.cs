using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyFollowLimitDurationProjectileStrategyData : FollowLimitDurationProjectileStrategyData
    {
        #region Class Methods

        public FlyFollowLimitDurationProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType strategyType, float offsetDegree, bool forceFollow, float moveDuration, float moveSpeed, bool rotateByDirection, float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null) : base(damageSource, strategyType, offsetDegree, forceFollow, moveDuration, moveSpeed, rotateByDirection, damageBonus, damageFactors, modifierModels)
        {
        }

        #endregion Class Methods
    }

    public abstract class FollowLimitDurationProjectileStrategyData : FlyLimitDurationProjectileStrategyData
    {
        #region Members

        public float offsetDegree;
        public bool forceFollow;

        #endregion Members

        #region Class Methods

        protected FollowLimitDurationProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType strategyType, float offsetDegree, bool forceFollow,
            float moveDuration, float moveSpeed, bool rotateByDirection, float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null) : base(damageSource, strategyType, moveDuration, moveSpeed, rotateByDirection, damageBonus, damageFactors, modifierModels)
        {
            this.offsetDegree = offsetDegree;
            this.forceFollow = forceFollow;
        }

        #endregion Class Methods
    }

    public class FlyFollowLimitDurationProjectileStrategy : FlyFollowLimitDurationProjectileStrategy<FlyFollowLimitDurationProjectileStrategyData> { }

    public class FlyFollowLimitDurationProjectileStrategy<T> : FlyLimitDurationProjectileStrategy<T> where T : FollowLimitDurationProjectileStrategyData
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
            if (strategyData.rotateByDirection)
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
                    if (strategyData.forceFollow || Mathf.Abs(currentAngle) >= strategyData.offsetDegree)
                    {
                        var offset = currentAngle > 0 ? strategyData.offsetDegree : -strategyData.offsetDegree;
                        currentDirection = Quaternion.AngleAxis(offset, Vector3.forward) * currentDirection;
                    }
                    else currentDirection = (targetModel.Position - controllerProjectile.CenterPosition).normalized;
                    if (strategyData.rotateByDirection)
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