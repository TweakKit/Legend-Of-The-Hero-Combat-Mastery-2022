using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IProjectileStrategy
    {
        #region Interface Methods

        void Init(ProjectileStrategyData projectileStrategyData, Projectile controllerProjectile, Vector2 direction, Vector2 originalPosition, EntityModel targetModel = null);
        void Start();
        void Update();
        void Collide(Collider2D collider);
        void Complete(bool forceComplete, bool displayImpact);

        #endregion Interface Methods
    }

    public abstract class ProjectileStrategyData
    {
        #region Members

        public DamageSource damageSource;
        public ProjectileStrategyType projectileStrategyType;

        #endregion Members

        #region Class Methods

        public ProjectileStrategyData(DamageSource damageSource, ProjectileStrategyType projectileStrategyType)
        {
            this.damageSource = damageSource;
            this.projectileStrategyType = projectileStrategyType;
        }

        #endregion Class Methods
    }

    public enum ProjectileStrategyType
    {
        None,
        Idle,
        IdleThrough,
        Forward,
        ForwardThrough,
        ForwardRotateIncrease,
        Follow,
        FollowThrough,
        SpawnDamageArea,
        Shattered,
        GoBack,
        SpawnEntities,
        FlyRound,
        FlyFollowLimitDuration,
        FlyFollowThroughLimitDuration,
        FlyZigzagLimitDuration,
        GoBackThrough
    }
}