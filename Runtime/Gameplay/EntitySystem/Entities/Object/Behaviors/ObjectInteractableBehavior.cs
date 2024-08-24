using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.CollisionDetection;

namespace Runtime.Gameplay.EntitySystem
{
    public sealed class ObjectInteractableBehavior : ObjectBehavior, IInteractable, ICollisionBody, IDisable
    {
        #region Members

        private SimpleAnimation _animationController;
        private ICollisionShape _collisionShape;
        private Collider2D _collider;
        private CollisionSearchTargetType[] _collisionSearchTargetTypes;

        #endregion Members

        #region Properties

        public EntityModel Model => ownerModel;
        public int RefId { get; set; }
        public ICollisionShape CollisionShape => _collisionShape;
        public CollisionBodyType CollisionBodyType => CollisionBodyType.Object;
        public Collider2D Collider => _collider;
        public Vector2 CollisionSystemPosition => _collider.bounds.center;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => _collisionSearchTargetTypes;

        #endregion Properties

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            _collider = ownerTransform.GetComponent<Collider2D>();
            if (_collider == null)
            {
                Debug.LogError("Have no collider!");
                return;
            }
        }
#endif

        public void GetHit(DamageInfo damageData, DamageMetaData damageMetaData)
        {
            if (!ownerModel.IsDead)
            {
                ownerModel.currentDestroyHits -= 1;
                _animationController?.RunAnim(0.2f);
                if (ownerModel.currentDestroyHits <= 0)
                    ownerModel.DestroyEvent.Invoke();
#if DEBUGGING
                Debug.Log($"get_hit_log || target: {ownerModel.EntityId}/{ownerModel.EntityType} | current_destroy_hits: {ownerModel.currentDestroyHits}");
#endif
            }
        }

        public void GetTrapped(TrapType trapType, DamageInfo damageData, DamageMetaData damageMetaData) { }
        public void StopTrapped(TrapType trapType) { }
        public void GetAffected(AffectedStatusEffectInfo affectedStatusEffectInfo, StatusEffectMetaData statusEffectMetaData) { }
        public void OnCollision(CollisionResult result, ICollisionBody other) { }

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            _collisionSearchTargetTypes = new CollisionSearchTargetType[0];
            _animationController = transform.GetComponentInChildren<SimpleAnimation>();
            _collider = transform.GetComponent<Collider2D>();
            _collisionShape = this.CreateCollisionShape(_collider);
            CollisionSystem.Instance.AddBody(this);
            return true;
        }

        public void Disable() => CollisionSystem.Instance.RemoveBody(this);

        #endregion Class Methods
    }
}