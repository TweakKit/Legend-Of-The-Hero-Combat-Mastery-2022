using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class Entity : Disposable, IEntity
    {
        #region Properties

        public uint EntityUId { get; private set; }

        #endregion Properties

        #region Class Methods

        public virtual void Build(EntityModel model, Vector3 position)
        {
            EntityUId = model.EntityUId;
            HasDisposed = false;
            SetUpPosition(model, position);
            SetUpBound(model);
        }

        public override void Dispose() { }
        public abstract T GetBehavior<T>(bool includeProxy = false) where T : class;

        protected virtual void SetUpPosition(EntityModel model, Vector3 position)
        {
            transform.position = position;
            model.Position = transform.position;
        }

        protected virtual void SetUpBound(EntityModel model)
        {
            var collider = gameObject.GetComponent<Collider2D>();
            if (collider != null)
            {
                if (collider.GetType() == typeof(BoxCollider2D))
                {
                    BoxCollider2D boxCollider = collider as BoxCollider2D;
                    model.Bound = new RectangleBound(transform, boxCollider.size.x, boxCollider.size.y);
                }
                else if (collider.GetType() == typeof(CircleCollider2D))
                {
                    CircleCollider2D circleCollider = collider as CircleCollider2D;
                    model.Bound = new CircleBound(transform, circleCollider.radius);
                }
                else if (collider.GetType() == typeof(CapsuleCollider2D))
                {
                    CapsuleCollider2D capsuleCollider = collider as CapsuleCollider2D;
                    model.Bound = new CapsuleBound(transform, capsuleCollider.size.x, capsuleCollider.size.y);
                }
                else model.Bound = new NoBound(transform);
            }
            else model.Bound = new NoBound(transform);
        }

        #endregion Class Methods
    }
}