using UnityEngine;
using Runtime.Gameplay.CollisionDetection;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(Collider2D))]
    public class CollisionProjectile : Projectile, ICollisionBody
    {
        #region Members

        private ICollisionShape _collisionShape;
        private Collider2D _collider;
        private CollisionSearchTargetType[] _collisionSearchTargetTypes;
        private bool _isDisablingCollide;
        private List<Collider2D> _aboveObstacleColliders;

        #endregion Members

        #region Properties

        public int RefId { get; set; }
        public Vector2 CollisionSystemPosition => _collider.bounds.center;
        public ICollisionShape CollisionShape => _collisionShape;
        public Collider2D Collider => _collider;
        public CollisionBodyType CollisionBodyType => CollisionBodyType.Projectile;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => _collisionSearchTargetTypes;

        #endregion Properties

        #region API Methods

        private void OnDisable()
            => Dispose();

        #endregion API Methods

        #region Class Methods

        public override async UniTask BuildAsync(CharacterModel creatorModel, Vector3 position)
        {
            _collider = gameObject.GetComponent<Collider2D>();
            _collider.enabled = false;
            await base.BuildAsync(creatorModel, position);
            RefId = -1;
            _collisionSearchTargetTypes = creatorModel.EntityType.GetCollisionBodySearchTypes(true);
            _collisionShape = this.CreateCollisionShape(_collider);
            CollisionSystem.Instance.AddBody(this);
        }

        public override void InitStrategies(IProjectileStrategy[] projectileStrategies)
        {
            base.InitStrategies(projectileStrategies);
            _isDisablingCollide = false;
            _collider.enabled = true;

            var contactFilter = new ContactFilter2D();
            contactFilter.useTriggers = false;
            contactFilter.SetLayerMask(LayerMask.GetMask(LayerNames.OBSTACLE_NAME));
            var colliders = new List<Collider2D>();
            var numberColliders = _collider.OverlapCollider(contactFilter, colliders);
            if (numberColliders > 0)
            {
                _aboveObstacleColliders = colliders;
            }
        }

        public void OnCollision(CollisionResult result, ICollisionBody other)
        {
            if(other.Collider != null && _aboveObstacleColliders != null && _aboveObstacleColliders.Count > 0 && _aboveObstacleColliders.Contains(other.Collider))
            {
                _aboveObstacleColliders.Remove(other.Collider);
                return;
            }

            if (result.collided && result.collisionType == CollisionType.Enter && other.Collider != null)
                currentStrategy?.Collide(other.Collider);
        }

        public override void EnableCollide()
        {
            base.EnableCollide();
            _collider.enabled = true;
        }

        public override void DisableCollide()
        {
            base.DisableCollide();
            _collider.enabled = false;
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