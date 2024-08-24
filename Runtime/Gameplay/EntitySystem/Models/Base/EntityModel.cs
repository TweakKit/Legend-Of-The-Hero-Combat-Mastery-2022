using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class EntityModel
    {
        #region Members

        protected uint entityUId;
        protected string entityId;
        protected int detectedPriority;
        protected bool isBeingTargeted;
        protected Vector2 position;
        protected uint spawnedWaveIndex;
        protected Bound bound;

        #endregion Members

        #region Properties

        public virtual bool IsDead => false;
        public virtual uint SpawnedWaveIndex => spawnedWaveIndex;

        public virtual bool IsBeingTargeted
        {
            get => isBeingTargeted;
            set => isBeingTargeted = value;
        }

        public Bound Bound
        {
            get => bound;
            set => bound = value;
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                if (position != value)
                    position = value;
            }
        }

        public virtual uint Level { get { return 1; } }
        public float Height { get { return bound.Height; } }
        public int DetectedPriority { get { return detectedPriority; } }
        public uint EntityUId { get { return entityUId; } }
        public string EntityId { get { return entityId; } }
        public Vector2 CenterPosition { get { return Position + Vector2.up * Height; } }
        public abstract EntityType EntityType { get; }

        #endregion Properties

        #region Class Methods

        public EntityModel(uint spawnedWaveIndex, uint entityUId, string entityId, int detectedPriority)
        {
            this.entityUId = entityUId;
            this.entityId = entityId;
            this.detectedPriority = detectedPriority;
            this.spawnedWaveIndex = spawnedWaveIndex;
        }

        public T GetEntityDistinctData<T>() where T : class
            => this as T;

        public Vector2 GetEdgePoint(Vector2 centerToEdgeDirection)
            => Bound.GetEdgePoint(centerToEdgeDirection);

        public static implicit operator bool(EntityModel entityModel)
            => entityModel != null;

        #endregion Class Methods
    }

    public static class EntityModelExtensions
    {
        #region Class Methods

        public static bool IsHero(this EntityType entityType) => entityType == EntityType.Hero;
        public static bool IsZombie(this EntityType entityType) => entityType == EntityType.Zombie;
        public static bool IsBoss(this EntityType entityType) => entityType == EntityType.Boss;
        public static bool IsEnemy(this EntityType entityType) => entityType == EntityType.Zombie || entityType == EntityType.Boss;
        public static bool IsCharacter(this EntityType entityType) => entityType == EntityType.Hero || entityType == EntityType.Zombie || entityType == EntityType.Boss;
        public static bool IsObject(this EntityType entityType) => entityType == EntityType.Object;
        public static bool IsTrap(this EntityType entityType) => entityType == EntityType.Trap;
        public static bool IsDamageArea(this EntityType entityType) => entityType == EntityType.DamageArea;
        public static bool IsTrapInterval(this uint id) => id != Constants.TRAP_POISON_ID;

        public static float GetLayOnGroundTime(this EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Boss:
                    return Constants.BOSS_LAY_ON_GROUND_TIME;
                default:
                    return Constants.CHARACTER_LAY_ON_GROUND_TIME;
            }
        }

        public static bool CanCauseDamage(this EntityType entityType, EntityType targetEntityType)
        {
            if (entityType == targetEntityType)
                return false;
            else if (entityType.IsEnemy() && targetEntityType.IsEnemy())
                return false;
            else if (targetEntityType == EntityType.Object)
                return true;
            return true;
        }

        public static bool CanDisableCollideWhenGetHurt(this EntityType entityType)
        {
            return entityType == EntityType.Hero;
        }

        public static int ShowHitEffectColorTimes(this EntityType entityType)
        {
            if (entityType == EntityType.Hero)
                return 4;
            return 2;
        }

        #endregion Class Methods
    }
}