using UnityEngine;
using Runtime.Gameplay.CollisionDetection;
using System.Threading;
using System;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior helps check the collision occured between the character and others.<br/>
    /// </summary>
    public class CharacterCollideBehavior : CharacterBehavior, ICollisionBody, IDisable
    {
        #region Members

        protected CancellationTokenSource getHurtCancellationTokenSource;
        protected ICollisionShape collisionShape;
        protected Collider2D collider;
        protected CollisionBodyType collisionBodyType;
        protected CollisionSearchTargetType[] collisionBodySearchTypes;
        protected bool isTurnOffCollide;

        #endregion Members

        #region Properties

        public int RefId { get; set; }
        public ICollisionShape CollisionShape => collisionShape;
        public Vector2 CollisionSystemPosition => collider.bounds.center;
        public Collider2D Collider => collider;
        public CollisionBodyType CollisionBodyType => collisionBodyType;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => collisionBodySearchTypes;

        #endregion Properties

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            collisionBodySearchTypes = ownerModel.EntityType.GetCollisionBodySearchTypes(false);
            collisionBodyType = ownerModel.EntityType.GetCollisionBodyType();
            var collider = transform.GetComponent<Collider2D>();
            if (collider != null)
            {
                ownerModel.CollideToggledEvent += OnCollideToggled;

                isTurnOffCollide = false;
                this.collider = collider;
                collisionShape = this.CreateCollisionShape(collider);
                CollisionSystem.Instance.AddBody(this);
                return true;
            }
            return false;
        }

        private void OnCollideToggled(bool value)
        {
            if (value)
            {
                if (isTurnOffCollide)
                {
                    collider.enabled = true;
                    CollisionSystem.Instance.AddBody(this);
                    var proxyEntity = ownerTransform.GetComponentInChildren<ProxyEntity>(true);
                    if (proxyEntity)
                    {
                        var footCollider = proxyEntity.GetComponent<Collider2D>();
                        if (footCollider)
                        {
                            footCollider.gameObject.SetActive(true);
                        }
                    }
                    isTurnOffCollide = false;
                }
            }
            else
            {
                if (!isTurnOffCollide)
                {
                    collider.enabled = false;
                    CollisionSystem.Instance.RemoveBody(this);
                    var proxyEntity = ownerTransform.GetComponentInChildren<ProxyEntity>(true);
                    if (proxyEntity)
                    {
                        var footCollider = proxyEntity.GetComponent<Collider2D>();
                        if (footCollider)
                        {
                            footCollider.gameObject.SetActive(false);
                        }
                    }
                    isTurnOffCollide = true;
                }
            }
        }

        public virtual void Disable() => CollisionSystem.Instance.RemoveBody(this);
        public virtual void OnCollision(CollisionResult result, ICollisionBody other) { }

        #endregion Class Methods
    }
}