using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This interface is the input for which type of "animation player" will take care of displaying the animations of the character.<br/>
    /// Examples of those inputs such as:<br/>
    ///     + Sprite Animation Player: Play animations by sprite animation.<br/>
    ///     + Clip Animation Player: Play animations by clip animation (Unity animation).<br/>
    ///     + Timing Player: Play animations by manual timing.<br/>
    ///     + ...
    /// </summary>
    public interface ICharacterAnimationPlayer
    {
        #region Interface Methods

        void Init(CharacterModel ownerModel);
        void Play(CharacterAnimationState state);
        void Pause();
        void Continue();
        void TintColor(Color color);

        #endregion Interface Methods
    }
}