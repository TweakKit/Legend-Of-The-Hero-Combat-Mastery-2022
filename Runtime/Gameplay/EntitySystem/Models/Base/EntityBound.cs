using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public enum BoundType
    {
        Rectangle,
        Circle,
        Capsule,
        None,
    }

    public abstract class Bound
    {
        #region Members

        public Transform owner;

        #endregion Members

        #region Properties

        public abstract BoundType BoundType { get; }
        public abstract float Height { get; }

        #endregion Properties

        #region Class Methods

        public Bound(Transform owner) => this.owner = owner;

        public abstract Vector3 GetEdgePoint(Vector3 centerToEdgeDirection);

        #endregion Class Methods
    }

    public class RectangleBound : Bound
    {
        #region Members

        public float width;
        public float height;

        #endregion Members

        #region Properties

        public override BoundType BoundType => BoundType.Rectangle;
        public override float Height => height;

        #endregion Properties

        #region Class Methods

        public RectangleBound(Transform ownerTransform, float width, float height) : base(ownerTransform)
        {
            this.width = width;
            this.height = height;
        }

        public override Vector3 GetEdgePoint(Vector3 centerToEdgeDirection)
            => owner.position + centerToEdgeDirection * width;

        #endregion Class Methods
    }

    public class CircleBound : Bound
    {
        #region Members

        public float radius;

        #endregion Members

        #region Properties

        public override BoundType BoundType => BoundType.Circle;
        public override float Height => radius * 2;

        #endregion Properties

        #region Class Methods

        public CircleBound(Transform ownerTransform, float radius) : base(ownerTransform) => this.radius = radius;

        public override Vector3 GetEdgePoint(Vector3 centerToEdgeDirection)
            => owner.position + centerToEdgeDirection * radius;

        #endregion Class Methods
    }

    public class CapsuleBound : Bound
    {
        #region Members

        public float radius;
        public float height;

        #endregion Members

        #region Properties

        public override BoundType BoundType => BoundType.Capsule;
        public override float Height => height;

        #endregion Properties

        #region Class Methods

        public CapsuleBound(Transform ownerTransform, float radius, float height) : base(ownerTransform)
        {
            this.radius = radius;
            this.height = height;
        }

        public override Vector3 GetEdgePoint(Vector3 centerToEdgeDirection)
            => owner.position + centerToEdgeDirection * radius;

        #endregion Class Methods
    }

    public class NoBound : Bound
    {
        #region Properties

        public override BoundType BoundType => BoundType.None;
        public override float Height => 0.0f;

        #endregion Properties

        #region Class Methods

        public NoBound(Transform ownerTransform) : base(ownerTransform) { }

        public override Vector3 GetEdgePoint(Vector3 centerToEdgeDirection) => owner.position;

        #endregion Class Methods
    }
}