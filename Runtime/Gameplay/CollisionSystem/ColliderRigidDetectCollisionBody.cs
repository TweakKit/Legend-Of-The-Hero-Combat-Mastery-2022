using UnityEngine;

namespace Runtime.Gameplay.CollisionDetection
{
    // It created out of the system, just in this case.

    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class ColliderRigidDetectCollisionBody : MonoBehaviour, ICollisionBody
    {
        #region Members

        [SerializeField]
        private Collider2D _collider;

        #endregion Members

        #region Properties

        public int RefId { get; set; } = -100;

        public ICollisionShape CollisionShape => null;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => null;

        public CollisionBodyType CollisionBodyType => CollisionBodyType.Default;

        public Collider2D Collider => _collider;

        public Vector2 CollisionSystemPosition => transform.position;

        #endregion Properties

        #region API Methods

#if UNITY_EDITOR
        private void OnValidate()
        {
            _collider = GetComponent<Collider2D>();
        }
#endif

        private void OnTriggerEnter2D(Collider2D collision)
        {
            var collisionBody = collision.GetComponent<ICollisionBody>();

            if (collisionBody != null)
            {
                var collisionResult = new CollisionResult();
                collisionResult.collided = true;
                collisionResult.collisionType = CollisionType.Enter;
                collisionBody.OnCollision(collisionResult, this);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            var collisionBody = collision.GetComponent<ICollisionBody>();

            if (collisionBody != null)
            {
                var collisionResult = new CollisionResult();
                collisionResult.collided = true;
                collisionResult.collisionType = CollisionType.Exit;
                collisionBody.OnCollision(collisionResult, this);
            }
        }

        #endregion API Methods

        #region Class Methods

        public void OnCollision(CollisionResult result, ICollisionBody other)
        {
        }

        #endregion Class Methods
    }
}
