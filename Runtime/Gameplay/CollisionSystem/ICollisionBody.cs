using System;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.CollisionDetection
{
    public enum CollisionSearchTargetType : int
    {
        None = -1,
        Hero = 0,
        Zombie = 1,
        Object = 2,
        Projectile = 3,
    }

    public enum CollisionBodyType
    {
        Default,
        TargetDetect,
        Hero,
        Zombie,
        Projectile,
        Object,
        Trap,
        DamageArea
    }

    public static class CollisionBodyExtensions
    {
        #region Class Methods

        public static CollisionSearchTargetType[] GetCollisionBodySearchTypes(this EntityType entityType, bool isProjectile = false)
        {
            if (entityType == EntityType.Boss || entityType == EntityType.Zombie)
            {
                if (isProjectile)
                    return new[] { CollisionSearchTargetType.Hero, CollisionSearchTargetType.Object };
                else
                    return new[] { CollisionSearchTargetType.Hero };
            }
            else if (entityType == EntityType.Hero)
                return new[] { CollisionSearchTargetType.Zombie, CollisionSearchTargetType.Object };
            return new CollisionSearchTargetType[0];
        }

        public static CollisionBodyType GetCollisionBodyType(this EntityType entityType)
        {
            if (entityType == EntityType.Boss || entityType == EntityType.Zombie)
                return CollisionBodyType.Zombie;
            else if (entityType == EntityType.Hero)
                return CollisionBodyType.Hero;
            else if (entityType == EntityType.Object)
                return CollisionBodyType.Object;
            else if (entityType == EntityType.Trap)
                return CollisionBodyType.Trap;
            else if (entityType == EntityType.DamageArea)
                return CollisionBodyType.DamageArea;
            return CollisionBodyType.Default;
        }

        public static ICollisionShape CreateCollisionShape(this ICollisionBody collisionBody, Collider2D collider)
        {
            if (collisionBody.Collider != null)
            {
                if (collider is BoxCollider2D)
                    return new RectangleCollisionShape(collisionBody, collider.bounds.extents.x * 2, collider.bounds.extents.y * 2);
                else if (collider is CircleCollider2D)
                    return new CircleCollisionShape(collisionBody, ((CircleCollider2D)collider).radius);
                else
                    return new ColliderCollisionShape(collisionBody);
            }
            return null;
        }

        public static ICollisionShape CreateCollisionShape(this ICollisionBody collisionBody, float radius)
            => new CircleCollisionShape(collisionBody, radius);

        #endregion Class Methods
    }

    public interface ICollisionBody
    {
        #region Properties

        int RefId { get; set; }
        ICollisionShape CollisionShape { get; }
        CollisionSearchTargetType[] CollisionSearchTargetTypes { get; }
        CollisionBodyType CollisionBodyType { get; }
        Collider2D Collider { get; }
        Vector2 CollisionSystemPosition { get; }

        #endregion Properties

        #region Interface Methods

        void OnCollision(CollisionResult result, ICollisionBody other);

        #endregion Interface Methods
    }
}