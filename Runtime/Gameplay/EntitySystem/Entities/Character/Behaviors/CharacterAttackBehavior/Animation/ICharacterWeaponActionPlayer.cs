using System;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This interface is the input for which type of "action player" will take care of when to trigger the attack related behaviors.<br/>
    /// Examples of those inputs such as:<br/>
    ///     + Sprite Animation Action Player: Play actions by sprite animation.<br/>
    ///     + Clip Animation Action Player: Play actions by clip animation (Unity animation).<br/>
    ///     + Timing Action Player: Play actions by manual timing.<br/>
    ///     + ...
    /// </summary>
    public interface ICharacterWeaponActionPlayer
    {
        #region Interface Methods

        public void Init();
        public void Play(CharacterPlayedWeaponAction characterPlayedWeaponAction);
        public void Cancel();

        #endregion Interface Methods
    }

    public struct CharacterPlayedWeaponAction
    {
        #region Members

        public CharacterWeaponAnimationType animationType;
        public Action operatedPointTriggeredCallbackAction;
        public Action endActionCallbackAction;

        #endregion Members

        #region Struct Methods

        public CharacterPlayedWeaponAction(CharacterWeaponAnimationType animationType)
        {
            this.animationType = animationType;
            this.operatedPointTriggeredCallbackAction = null;
            this.endActionCallbackAction = null;
        }

        public CharacterPlayedWeaponAction(CharacterWeaponAnimationType animationType, Action operatedPointTriggeredCallbackAction, Action endActionCallbackAction)
        {
            this.animationType = animationType;
            this.operatedPointTriggeredCallbackAction = operatedPointTriggeredCallbackAction;
            this.endActionCallbackAction = endActionCallbackAction;
        }

        #endregion Struct Methods
    }
}