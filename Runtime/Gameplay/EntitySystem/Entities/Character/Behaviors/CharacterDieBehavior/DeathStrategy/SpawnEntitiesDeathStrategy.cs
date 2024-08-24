using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnEntitiesDeathStrategy : IDeathStrategy
    {
        private static readonly float s_entitySpawnFromCharacterOffset = 1.5f;

        public async UniTask Execute(EntityModel entityModel, DeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            var dataConfig = deathDataConfig as SpawnEntitiesDeathDataConfigItem;

            await EntitiesManager.Instance.CreateEntitiesAsync(entityModel.SpawnedWaveIndex, entityModel.Position, s_entitySpawnFromCharacterOffset, 
                                                               false, cancellationToken, dataConfig.spawnEntityInfo);
        }
    }
}
