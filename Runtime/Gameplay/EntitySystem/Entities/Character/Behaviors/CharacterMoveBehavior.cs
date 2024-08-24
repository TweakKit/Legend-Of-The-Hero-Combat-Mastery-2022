using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior updates the movement transition for the character with non-physics.<br/>
    /// </summary>
    public sealed class CharacterMoveBehavior : CharacterBehavior, IUpdatable
    {
        #region Members

        private float _moveSpeed;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel.MovePositionUpdatedEvent += OnMovePositionUpdated;
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
                Vector3 nextPosition = ownerModel.Position + ownerModel.MoveDirection * _moveSpeed * Time.deltaTime;
                ownerTransform.position = nextPosition;
                ownerModel.Position = ownerTransform.position;
            }
        }

        private void OnMovePositionUpdated()
        {
            ownerTransform.position = ownerModel.MovePosition;
            ownerModel.Position = ownerTransform.position;
        }

        private void OnStatChanged(StatType statType, float updatedValue)
        {
            if (statType == StatType.MoveSpeed)
                _moveSpeed = updatedValue;
        }

        #endregion Class Methods
    }
}