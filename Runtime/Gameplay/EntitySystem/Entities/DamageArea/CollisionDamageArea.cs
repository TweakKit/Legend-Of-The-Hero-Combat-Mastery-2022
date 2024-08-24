using Cysharp.Threading.Tasks;
using Runtime.Gameplay.CollisionDetection;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionDamageArea : DamageArea, ICollisionBody
    {
        #region Members

        private ICollisionShape _collisionShape;
        private Collider2D _collider;
        private CollisionSearchTargetType[] _collisionSearchTargetTypes;

        #endregion Members

        #region Properties

        public int RefId { get; set; }

        public ICollisionShape CollisionShape => _collisionShape;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => _collisionSearchTargetTypes;

        public CollisionBodyType CollisionBodyType => CollisionBodyType.DamageArea;

        public Collider2D Collider => _collider;

        public Vector2 CollisionSystemPosition => _collider.bounds.center;

        #endregion Properties

        #region Class Methods

        public async override UniTask BuildAsync(CharacterModel creatorModel, Vector3 position, DamageAreaData data)
        {
            _collider = gameObject.GetComponent<Collider2D>();
            _collider.enabled = false;
            await base.BuildAsync(creatorModel, position, data);
            RefId = -1;
            _collisionSearchTargetTypes = creatorModel.EntityType.GetCollisionBodySearchTypes(true);
            _collisionShape = this.CreateCollisionShape(_collider);
            CollisionSystem.Instance.AddBody(this);
        }

        public void OnCollision(CollisionResult result, ICollisionBody other)
        {
            if(result.collisionType == CollisionType.Enter)
            {
                var collider = other.Collider;
                if(collider != null)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>(true);
                        if (interactable != null && !interactable.Model.IsDead)
                        {
                            if (interactable.Model.EntityType != data.creatorModel.EntityType && !damagedTargets.Contains(interactable))
                                damagedTargets.Add(interactable);
                        }
                    }
                }
            }
            else if (result.collisionType == CollisionType.Exit)
            {
                var collider = other.Collider;
                if (collider != null)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>(true);
                        if (interactable != null)
                        {
                            if (damagedTargets.Contains(interactable))
                                damagedTargets.Remove(interactable);
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            if (!HasDisposed)
            {
                HasDisposed = true;
                CollisionSystem.Instance.RemoveBody(this);
            }
        }

        #endregion Class Methods
    }
}