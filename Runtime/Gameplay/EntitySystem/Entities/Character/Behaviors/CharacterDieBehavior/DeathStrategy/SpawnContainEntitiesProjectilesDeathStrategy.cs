using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using UnityEngine;
using System.Linq;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnContainEntitiesProjectilesDeathStrategy : IDeathStrategy
    {
        public UniTask Execute(EntityModel entityModel, DeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            if (entityModel.EntityType.IsCharacter())
            {
                var characterModel = entityModel as CharacterModel;
                var dataConfig = deathDataConfig as SpawnContainEntitiesProjectilesDeathDataConfigItem;
                var numberOfProjectiles = dataConfig.projectileNumber;

                var bigAngle = 360;
                var projectileCenterAngleOffset = (float)bigAngle / numberOfProjectiles;
                var firstDegree = 0;
                var firstDirection = Vector2.up;

                Vector2 projectilePosition = entityModel.Position;
                float offset = 0.3f; // avoid collide obstacle immediately when spawn
                for (int i = 0; i < numberOfProjectiles; i++)
                {
                    var direction = (Quaternion.AngleAxis(firstDegree +  projectileCenterAngleOffset * i, Vector3.forward) * firstDirection).normalized;
                    EntitiesManager.Instance.AddActionContainEnemySpawnedWaveIndex(characterModel.SpawnedWaveIndex);
                    SpawnProjectileAsync(characterModel, dataConfig, direction, projectilePosition + (Vector2)(offset * direction), cancellationToken).Forget();
                }

            }
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid SpawnProjectileAsync(CharacterModel characterModel, SpawnContainEntitiesProjectilesDeathDataConfigItem dataConfig, Vector2 direction, Vector2 projectilePosition, CancellationToken cancellationToken)
        {
            FlyForwardProjectileStrategyData flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromOther,
                                                                                        dataConfig.projectileMoveDistance,
                                                                                        dataConfig.projectileMoveSpeed,
                                                                                        dataConfig.projectileDamageBonus,
                                                                                        damageFactors: dataConfig.projectileDamageFactors);

            var spawnEntities = dataConfig.spawnEntities;
            if (dataConfig.useOwnerLevel)
            {
                spawnEntities = spawnEntities.Select(x => new SpawnedEntityInfo(x.entityId, x.entityType, x.entityNumber, characterModel.Level)).ToArray();
            }
            SpawnEntitiesProjectileFinishStrategyData spawnEntitiesProjectileFinishStrategyData = new SpawnEntitiesProjectileFinishStrategyData
                                                                                                        (DamageSource.FromOther,
                                                                                                         spawnEntities,
                                                                                                         () =>
                                                                                                         {
                                                                                                             var spawnedWaveIndex = characterModel.SpawnedWaveIndex;
                                                                                                             EntitiesManager.Instance.RemoveActionContainEnemySpawnedWaveIndex(spawnedWaveIndex);
                                                                                                         },
                                                                                                         cancellationToken);


            var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(dataConfig.projectileId, characterModel, projectilePosition, cancellationToken);
            var projectile = projectileGameObject.GetComponent<Projectile>();

            var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.Forward);
            var finishProjectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.SpawnEntities);

            projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, direction, projectilePosition);
            finishProjectileStrategy.Init(spawnEntitiesProjectileFinishStrategyData, projectile, direction, projectilePosition);

            projectile.InitStrategies(new[] { projectileStrategy, finishProjectileStrategy });
        }
    }
}
