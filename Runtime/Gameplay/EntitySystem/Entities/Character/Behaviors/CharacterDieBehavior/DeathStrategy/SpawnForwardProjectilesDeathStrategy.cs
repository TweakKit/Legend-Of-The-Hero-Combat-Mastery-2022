using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnForwardProjectilesDeathStrategy : IDeathStrategy
    {
        public UniTask Execute(EntityModel entityModel, DeathDataConfigItem deathDataConfig, CancellationToken cancellationToken)
        {
            if (entityModel.EntityType.IsCharacter())
            {
                var characterModel = entityModel as CharacterModel;
                var dataConfig = deathDataConfig as SpawnForwardProjectilesDeathDataConfigItem;
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
                    SpawnProjectileAsync(characterModel, dataConfig, direction, projectilePosition + (Vector2)direction * offset, cancellationToken).Forget();
                }

            }
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid SpawnProjectileAsync(CharacterModel characterModel, SpawnForwardProjectilesDeathDataConfigItem dataConfig, Vector2 direction, Vector2 projectilePosition, CancellationToken cancellationToken)
        {
            FlyForwardProjectileStrategyData flyForwardProjectileStrategyData = null;
            flyForwardProjectileStrategyData = new FlyForwardProjectileStrategyData(DamageSource.FromOther,
                                                                                    dataConfig.projectileMoveDistance,
                                                                                    dataConfig.projectileMoveSpeed,
                                                                                    dataConfig.projectileDamageBonus,
                                                                                    damageFactors: dataConfig.projectileDamageFactors);

            var projectileGameObject = await EntitiesManager.Instance.CreateProjectileAsync(dataConfig.projectileId, characterModel, projectilePosition, cancellationToken);
            var projectile = projectileGameObject.GetComponent<Projectile>();
            var projectileStrategy = ProjectileStrategyFactory.GetProjectileStrategy(ProjectileStrategyType.Forward);
            projectileStrategy.Init(flyForwardProjectileStrategyData, projectile, direction, projectilePosition);
            projectile.InitStrategies(new[] { projectileStrategy });
        }
    }
}