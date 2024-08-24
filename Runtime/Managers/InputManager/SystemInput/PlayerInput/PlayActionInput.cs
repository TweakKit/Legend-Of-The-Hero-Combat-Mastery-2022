using System;

namespace UnityEngine.InputSystem.Custom
{
    public enum ButtonInputType
    {
        NormalAttack,
        SpecialAttack,
    }

    public class PlayerActionInput
    {
        #region Members

        private static PlayerActionInput s_instance;
        private static readonly object s_locker = new object();

        #endregion Members

        #region Properties

        public static PlayerActionInput Main
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_locker)
                        s_instance = new PlayerActionInput();
                }

                return s_instance;
            }
        }

        private Action NormalAttackButtonTriggeredAction { get; set; }
        private Action SpecialAttackButtonTriggeredAction { get; set; }

        #endregion Properties

        #region Class Methods

        private PlayerActionInput() { }

        public void Init()
        {
            NormalAttackButtonTriggeredAction = null;
            SpecialAttackButtonTriggeredAction = null;
        }

        public void RegisterNormalAttackButtonTriggeredAction(Action action)
            => NormalAttackButtonTriggeredAction += action;

        public void RegisterSpecialAttackButtonTriggeredAction(Action action)
            => SpecialAttackButtonTriggeredAction += action;

        public void UnregisterNormalAttackButtonTriggeredAction(Action action)
            => NormalAttackButtonTriggeredAction -= action;

        public void UnregisterSpecialAttackButtonTriggeredAction(Action action)
            => SpecialAttackButtonTriggeredAction -= action;

        public void Trigger(ButtonInputType buttonInputType)
        {
            switch (buttonInputType)
            {
                case ButtonInputType.NormalAttack:
                    NormalAttackButtonTriggeredAction?.Invoke();
                    break;

                case ButtonInputType.SpecialAttack:
                    SpecialAttackButtonTriggeredAction?.Invoke();
                    break;
            }
        }

        #endregion Class Methods
    }
}