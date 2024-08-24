using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnDamageAreaProjectileFinishStrategyData : ProjectileStrategyData
    {
        #region Members

        public readonly string DamageAreaId;
        public readonly float Lifetime;
        public readonly float Interval;
        public readonly float Width;
        public readonly float Height;
        public readonly  DamageFactor[] DamageAreaDamageFactors;
        public readonly StatusEffectModel[] DamageAreaDamageModifierModels;
        public readonly DamageFactor[] FirstInitDamageFactors;
        public readonly StatusEffectModel[] FirstInitDamageModifierModels;

        #endregion Members

        #region Class Methods

        public SpawnDamageAreaProjectileFinishStrategyData(DamageSource damageSource, string damageAreaId, float lifeTime, DamageFactor[] damageAreaDamageFactors, float damageWidth, float damageHeight,
                                                           float interval, StatusEffectModel[] damageAreaDamageModifierModels, DamageFactor[] firstInitDamageFactors, StatusEffectModel[] firstInitDamageModifierModels)
            : base(damageSource, ProjectileStrategyType.SpawnDamageArea)
        {
            DamageAreaId = damageAreaId;
            Lifetime = lifeTime;
            DamageAreaDamageModifierModels = damageAreaDamageModifierModels;
            DamageAreaDamageFactors = damageAreaDamageFactors;
            Interval = interval;
            Width = damageWidth;
            Height = damageHeight;
            FirstInitDamageFactors = firstInitDamageFactors;
            FirstInitDamageModifierModels = firstInitDamageModifierModels;
        }

        #endregion Class Methods
    }

    public class SpawnDamageAreaProjectileFinishStrategy : ProjectileStrategy<SpawnDamageAreaProjectileFinishStrategyData>
    {
        #region Class Methods

        public override void Start()
        {
            Complete(false, true);
        }

        public override void Complete(bool forceComplete, bool displayImpact)
        {
            SpawnDamageAreaAsync().Forget();
            base.Complete(forceComplete, displayImpact);
        }

        private async UniTaskVoid SpawnDamageAreaAsync()
        {
            var damageAreaData = new DamageAreaData(controllerProjectile.CreatorModel,
                                                    strategyData.Lifetime,
                                                    strategyData.Interval,
                                                    strategyData.damageSource,
                                                    strategyData.Width,
                                                    strategyData.Height,
                                                    strategyData.DamageAreaDamageFactors,
                                                    strategyData.DamageAreaDamageModifierModels,
                                                    strategyData.FirstInitDamageFactors,
                                                    strategyData.FirstInitDamageModifierModels
            );

            await EntitiesManager.Instance.CreateDamageAreaAsync(strategyData.DamageAreaId, controllerProjectile.CreatorModel, controllerProjectile.CenterPosition, damageAreaData);
        }

        #endregion Class Methods
    }
}