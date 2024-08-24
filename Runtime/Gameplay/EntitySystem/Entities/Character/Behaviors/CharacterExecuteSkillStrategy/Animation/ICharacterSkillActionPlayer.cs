using System;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This interface is the input for which type of "action player" will take care of when to trigger the skill related behaviors.<br/>
    /// Examples of those inputs such as:<br/>
    ///     + Sprite Animation Action Player: Play actions by sprite animation.<br/>
    ///     + Clip Animation Action Player: Play actions by clip animation (Unity animation).<br/>
    ///     + Timing Action Player: Play actions by manual timing.<br/>
    ///     + ...
    /// </summary>
    public interface ICharacterSkillActionPlayer
    {
        #region Interface Methods

        void Init(CharacterModel characterModel);
        void Play(CharacterPlayedSkillAction characterPlayedSkillAction);

        #endregion Interface Methods
    }

    public struct CharacterPlayedSkillAction
    {
        #region Members

        public SkillType skillType;
        public SkillActionPhase skillActionPhase;
        public Action eventTriggeredCallbackAction;
        public Action endActionCallbackAction;

        #endregion Members

        #region Struct Methods

        public CharacterPlayedSkillAction(SkillType skillType, SkillActionPhase skillActionPhase, Action eventTriggeredCallbackAction, Action endActionCallbackAction)
        {
            this.skillType = skillType;
            this.skillActionPhase = skillActionPhase;
            this.eventTriggeredCallbackAction = eventTriggeredCallbackAction;
            this.endActionCallbackAction = endActionCallbackAction;
        }

        #endregion Struct Methods
    }
}