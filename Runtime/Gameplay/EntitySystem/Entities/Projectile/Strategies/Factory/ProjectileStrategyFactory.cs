namespace Runtime.Gameplay.EntitySystem
{
    public static class ProjectileStrategyFactory
    {
        #region Class Methods

        public static IProjectileStrategy GetProjectileStrategy(ProjectileStrategyType projectileStrategyType)
        {
            switch (projectileStrategyType)
            {
                case ProjectileStrategyType.Forward:
                    return new FlyForwardProjectileStrategy();

                case ProjectileStrategyType.ForwardThrough:
                    return new FlyForwardThroughProjectileStrategy();

                case ProjectileStrategyType.Follow:
                    return new FlyFollowProjectileStrategy();

                case ProjectileStrategyType.FollowThrough:
                    return new FlyFollowThroughProjectileStrategy();

                case ProjectileStrategyType.SpawnDamageArea:
                    return new SpawnDamageAreaProjectileFinishStrategy();

                case ProjectileStrategyType.Shattered:
                    return new ShatteredProjectileStrategy();

                case ProjectileStrategyType.GoBack:
                    return new GoBackProjectileStrategy();

                case ProjectileStrategyType.IdleThrough:
                    return new IdleThroughProjectileStrategy();

                case ProjectileStrategyType.ForwardRotateIncrease:
                    return new FlyForwardRotateIncreaseProjectileStrategy();

                case ProjectileStrategyType.SpawnEntities:
                    return new SpawnEntitiesProjectileFinishStrategy();

                case ProjectileStrategyType.FlyRound:
                    return new FlyRoundProjectileStrategy();

                case ProjectileStrategyType.FlyFollowLimitDuration:
                    return new FlyFollowLimitDurationProjectileStrategy();

                case ProjectileStrategyType.FlyFollowThroughLimitDuration:
                    return new FlyFollowThroughLimitDurationProjectileStrategy();

                case ProjectileStrategyType.FlyZigzagLimitDuration:
                    return new FlyZigzagLimitDurationProjectileStrategy();

                case ProjectileStrategyType.GoBackThrough:
                    return new GoBackThroughProjectileStrategy();

                default:
                    return null;
            }
        }

        #endregion Class Methods
    }
}