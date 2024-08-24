using UnityEngine;
using UnityEngine.InputSystem.Custom;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior updates the data input for the character.<br/>
    /// The data input returned will be based on the input from the player.
    /// This behavior is used for characters those are directly controlled by the player.
    /// </summary>
    public sealed class CharacterControlInputBehavior : CharacterBehavior, IUpdatable, IDisable
    {
        #region Members

        private PlayerActionInput _playerActionInput;
#if UNITY_EDITOR
        private PlayerMovementInput _playerMovementInput;
#else
        private PlayerMovementInputWithoutKeyBoard _playerMovementInput;
#endif

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
#if UNITY_EDITOR
            _playerMovementInput = new PlayerMovementInput();
#else
            _playerMovementInput = new PlayerMovementInputWithoutKeyBoard();
#endif
            _playerMovementInput.Enable();
            _playerActionInput = PlayerActionInput.Main;
            _playerActionInput.Init();
            _playerActionInput.RegisterNormalAttackButtonTriggeredAction(OnNormalAttackButtonTriggered);
            _playerActionInput.RegisterSpecialAttackButtonTriggeredAction(OnSpecialAttackButtonTriggered);
            return true;
        }

        public void Update()
        {
            if (ownerModel.IsControllable)
            {
                var inputDirection = _playerMovementInput.MainAction.Move.ReadValue<Vector2>();
                ownerModel.SetMoveDirection(inputDirection.normalized);
            }
        }

        public void OnNormalAttackButtonTriggered()
        {
            if (ownerModel.IsControllable)
                ownerModel.ActionTriggeredEvent.Invoke(ActionInputType.Attack);
        }

        public void OnSpecialAttackButtonTriggered()
        {
            if (ownerModel.IsControllable)
                ownerModel.ActionTriggeredEvent.Invoke(ActionInputType.SpecialAttack);
        }

        public void Disable()
        {
            _playerActionInput.UnregisterNormalAttackButtonTriggeredAction(OnNormalAttackButtonTriggered);
            _playerActionInput.UnregisterSpecialAttackButtonTriggeredAction(OnSpecialAttackButtonTriggered);
            _playerMovementInput.Disable();
        }

        #endregion Class Methods
    }
}