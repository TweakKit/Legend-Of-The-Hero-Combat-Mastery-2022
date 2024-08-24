using System;
using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class BossTransformDeathStrategy : IDeathStrategy
    {
        #region Class Methods

        public async UniTask Execute(EntityModel entityModel, DeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            var dataConfig = deathDataConfig as BossTransformDeathDataConfigItem;
            await SpawnTransformationExplosionEffect(entityModel, dataConfig, cancellationToken);
            await UniTask.Delay(TimeSpan.FromSeconds(dataConfig.proceedTransformationDelay));
            await ProceedTransformation(entityModel, dataConfig, cancellationToken);
        }

        public async UniTask SpawnTransformationExplosionEffect(EntityModel entityModel, BossTransformDeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            var preTransformExplosionGameObject = await PoolManager.Instance.Get(deathDataConfig.preTransformExplosionPrefab, cancellationToken);
            preTransformExplosionGameObject.transform.position = entityModel.Position;
        }

        public async UniTask ProceedTransformation(EntityModel entityModel, BossTransformDeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            var transformedBossModel = await EntitiesManager.Instance.CreateTransformedBossAsync(entityModel.SpawnedWaveIndex, entityModel.EntityId, deathDataConfig.transformedBossId, entityModel.Level,
                                                                                                 entityModel.Position, deathDataConfig.bossTransformationDataConfig, cancellationToken);
            var numberOfParasites = deathDataConfig.spawnParasitesInfo.entityNumber;
            var angle = Constants.CIRCLE_DEGREES / deathDataConfig.spawnParasitesInfo.entityNumber;
            Vector3 targetDirection = Vector3.up;
            for (int i = 0; i < numberOfParasites; i++)
            {
                var parasiteDirection = Quaternion.Euler(0, 0, angle * i) * targetDirection;
                var parasitePosition = (Vector3)entityModel.Position + parasiteDirection * deathDataConfig.spawnParasitesCenterOffsetDistance;
                await EntitiesManager.Instance.CreateParasiteAsync(entityModel.SpawnedWaveIndex, deathDataConfig.spawnParasitesInfo.entityId, parasitePosition,
                                                                   transformedBossModel as CharacterModel, cancellationToken);
            }
        }

        #endregion Class Methods
    }
}