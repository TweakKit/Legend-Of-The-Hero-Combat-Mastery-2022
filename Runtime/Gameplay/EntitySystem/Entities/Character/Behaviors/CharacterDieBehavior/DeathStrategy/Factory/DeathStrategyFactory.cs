using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public static class DeathStrategyFactory
    {
        public static IDeathStrategy GetDeathStrategy(DeathDataType deathConfigType)
        {
            switch (deathConfigType)
            {
                case DeathDataType.SpawnEntities:
                    return new SpawnEntitiesDeathStrategy();

                case DeathDataType.SpawnForwardProjectiles:
                    return new SpawnForwardProjectilesDeathStrategy();

                case DeathDataType.SpawnContainEntitiesProjectiles:
                    return new SpawnContainEntitiesProjectilesDeathStrategy();

                case DeathDataType.BossTransform:
                    return new BossTransformDeathStrategy();

                default:
                    return null;
            }
        }
    }
}