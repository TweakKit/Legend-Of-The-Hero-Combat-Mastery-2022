using UnityEngine;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class ShatteredProjectileStrategyData : ProjectileStrategyData
    {
        #region Members

        public string projectileId;
        public uint projectileNumber;
        public float projectileLifeRange;
        public float speed;
        public float damageBonus;
        public DamageFactor[] damageFactors;
        public StatusEffectModel[] modifierModels;

        #endregion Members

        #region Class Methods

        public ShatteredProjectileStrategyData(DamageSource damageSource, string projectileId, uint projectileNumber, float projectileLifeRange, float speed,
                                               float damageBonus = 0, DamageFactor[] damageFactors = null, StatusEffectModel[] modifierModels = null)
            : base(damageSource, ProjectileStrategyType.Shattered)
        {
            this.projectileId = projectileId;
            this.projectileNumber = projectileNumber;
            this.projectileLifeRange = projectileLifeRange;
            this.damageBonus = damageBonus;
            this.damageFactors = damageFactors;
            this.modifierModels = modifierModels;
            this.speed = speed;
        }

        #endregion Class Methods
    }

    public class ShatteredProjectileStrategy : ProjectileStrategy<ShatteredProjectileStrategyData>
    {
        #region Class Methods

        public override void Start()
        {
            Complete(false, true);
        }

        public override void Complete(bool forceComplete, bool displayImpact)
        {
            StartShatteringAsync().Forget();
            base.Complete(forceComplete, displayImpact);
        }

        private async UniTaskVoid StartShatteringAsync()
        {
            var offsetDegrees = 360/strategyData.projectileNumber;
            for (int i = 0; i < strategyData.projectileNumber; i++)
            {
                var direction = (Quaternion.AngleAxis(offsetDegrees * i, Vector3.forward) * Vector2.up).normalized;
                var flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(strategyData.damageSource, strategyData.projectileLifeRange, strategyData.speed, strategyData.damageBonus, strategyData.damageFactors, strategyData.modifierModels);
                var smallProjectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(strategyData.projectileId, controllerProjectile.CreatorModel, controllerProjectile.CenterPosition);
                var smallProjectile = smallProjectileGameObject.GetComponent<Projectile>();
                var smallProjectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.Forward);
                smallProjectileStrategy.Init(flyForwardProjectileStrategyData, controllerProjectile, direction, controllerProjectile.CenterPosition);
                smallProjectile.InitStrategies(new[] { smallProjectileStrategy });
            }
        }

        #endregion Class Methods
    }
}