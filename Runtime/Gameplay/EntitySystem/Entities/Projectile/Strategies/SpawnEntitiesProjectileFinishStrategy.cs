using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnEntitiesProjectileFinishStrategyData : ProjectileStrategyData
    {
        #region Members

        public readonly CancellationToken CancellationToken;
        public readonly SpawnedEntityInfo[] SpawnedEntitiesInfo;
        public readonly Action SpawnedEntitiesAction;

        #endregion Members

        #region Class Methods

        public SpawnEntitiesProjectileFinishStrategyData(DamageSource damageSource,
            SpawnedEntityInfo[] spawnedEntitiesInfo, Action spawnedEntitiesAction, CancellationToken cancellationToken) : base(damageSource, ProjectileStrategyType.SpawnEntities)
        {
            SpawnedEntitiesInfo = spawnedEntitiesInfo;
            CancellationToken = cancellationToken;
            SpawnedEntitiesAction = spawnedEntitiesAction;
        }

        #endregion Class Methods
    }

    public class SpawnEntitiesProjectileFinishStrategy : ProjectileStrategy<SpawnEntitiesProjectileFinishStrategyData>
    {
        private static readonly float s_entitySpawnFromCharacterOffset = 0.1f;

        public override void Start()
        {
            Complete(false, true);
        }

        public override void Complete(bool forceComplete, bool displayImpact)
        {
            base.Complete(forceComplete, displayImpact);
            SpawnEntitiesAsync().Forget();
            strategyData.SpawnedEntitiesAction?.Invoke();
        }

        private async UniTaskVoid SpawnEntitiesAsync()
        {
            await EntitiesManager.Instance.CreateEntitiesAsync(controllerProjectile.SpawnedWaveIndex, controllerProjectile.CenterPosition, s_entitySpawnFromCharacterOffset,
                                                               false, strategyData.CancellationToken, strategyData.SpawnedEntitiesInfo);
        }
    }
}