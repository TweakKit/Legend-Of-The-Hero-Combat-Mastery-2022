using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior updates the movement transition for the character with physics.<br/>
    /// </summary>
    public sealed class CharacterMovePhysicsBehavior : CharacterBehavior, IPhysicsUpdatable
    {
        #region Members

        private float _moveSpeed;
        private Rigidbody2D _rigidbody;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            _rigidbody = ownerTransform.GetComponent<Rigidbody2D>();
            if (_rigidbody == null)
            {
                Debug.LogError("Require a Rigidbody 2D component!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            _rigidbody = transform.GetComponent<Rigidbody2D>();
            ownerModel.MovePositionUpdatedEvent += OnMovePositionUpdated;
            ownerModel.DeathEvent += OnDeath;
            var tryGetMoveSpeed = 0.0f;
            if (ownerModel.TryGetStat(StatType.MoveSpeed, out tryGetMoveSpeed))
            {
                _moveSpeed = tryGetMoveSpeed;
                ownerModel.StatChangedEvent += OnStatChanged;
                return true;
            }
#if DEBUGGING
            else
            {
                Debug.LogError($"Require {StatType.MoveSpeed} for this behavior to work!");
                return false;
            }
#else
            else return false;
#endif
        }

        public void Update()
        {
            if (ownerModel.IsMoveable)
            {
                Vector2 nextPosition = _rigidbody.position + ownerModel.MoveDirection * _moveSpeed * Time.fixedDeltaTime;
                _rigidbody.MovePosition(nextPosition);
                ownerModel.Position = _rigidbody.position;
            }
        }

        private void OnMovePositionUpdated()
        {
            _rigidbody.MovePosition(ownerModel.MovePosition);
            ownerModel.Position = _rigidbody.position;
        }

        private void OnStatChanged(StatType statType, float updatedValue)
        {
            if (statType == StatType.MoveSpeed)
                _moveSpeed = updatedValue;
        }

        private void OnDeath(DamageSource damageSource)
            => _rigidbody.velocity = Vector2.zero;

        #endregion Class Methods
    }
}