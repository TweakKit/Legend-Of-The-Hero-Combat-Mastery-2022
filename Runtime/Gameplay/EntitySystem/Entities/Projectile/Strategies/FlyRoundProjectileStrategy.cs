using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Utilities;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlyRoundProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public float flyDuration;
        public float flyHeight;
        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] modifierModels;
        public string warningPrefabName;
        public float warningHeight;
        public float warningWidth;
        public CancellationToken cancellationToken;

        #endregion Members

        #region Class Methods

        public FlyRoundProjectileStrategyData(DamageSource damageSource, float flyDuration, float flyHeight, string warningPrefabName,
                                        float warningHeight, float warningWidth, CancellationToken cancellationToken,
                                        float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null) 
            : base(damageSource, ProjectileStrategyType.FlyRound)
        {
            this.warningPrefabName = warningPrefabName;
            this.flyHeight = flyHeight;
            this.flyDuration = flyDuration;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.modifierModels = modifierModels;
            this.warningHeight = warningHeight;
            this.warningWidth = warningWidth;
            this.cancellationToken = cancellationToken;
        }

        #endregion Class Methods
    }

    public class FlyRoundProjectileStrategy : ProjectileStrategy<FlyRoundProjectileStrategyData>
    {
        #region Members

        protected Vector2 targetPosition;
        protected Vector2 originalPosition;
        protected Vector2 middlePosition;
        protected float currentFlyTime;
        protected GameObject warningGameObject;

        #endregion Members

        public override void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null)
        {
            base.Init(projectileStrategyData, controllerProjectile, direction, originalPosition, targetModel);

            targetPosition = targetModel.Position;
            this.originalPosition = originalPosition;
            middlePosition = new Vector2((targetPosition.x + originalPosition.x) / 2, (targetPosition.y + originalPosition.y) / 2 + strategyData.flyHeight);
            currentFlyTime = 0;
            if (!string.IsNullOrEmpty(strategyData.warningPrefabName))
            {
                SpawnWarningVfx(targetPosition).Forget();
            }
        }

        private async UniTaskVoid SpawnWarningVfx(Vector2 spawnPosition)
        {
            warningGameObject = await PoolManager.Instance.Get(strategyData.warningPrefabName);
            var warningDamageArea = warningGameObject.GetComponent<WarningDamageArea>();
            warningDamageArea.Init(spawnPosition, new Vector2(strategyData.warningWidth / 2, strategyData.warningHeight / 2));
        }

        public override void Update()
        {
            base.Update();
            if(currentFlyTime >= strategyData.flyDuration)
            {
                if(warningGameObject)
                {
                    PoolManager.Instance.Remove(warningGameObject);
                }

                Complete(false, true);
            }
            else
            {
                currentFlyTime += Time.deltaTime;
                var moveToPosition = MathUtility.Bezier(originalPosition, middlePosition, targetPosition, Mathf.Clamp01(currentFlyTime / strategyData.flyDuration));
                controllerProjectile.UpdatePosition(moveToPosition);
            }
        }
    }
}
