using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Runtime.Animation;
using Runtime.Definition;
using Sirenix.OdinInspector;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class CharacterSkillStageSpriteAnimation
    {
        #region Members

        public SkillActionPhase skillActionPhase;
        public bool isLoop;
        public string mappedPrecastAnimationName;
        public bool hasEventTriggeredAtAFrame;
        [ShowIf(nameof(hasEventTriggeredAtAFrame), true)]
        public int eventTriggeredFrame;

        #endregion Members
    }

    [Serializable]
    public class CharacterSkillSpriteAnimation
    {
        #region Members

        public SkillType skillType;
        public List<CharacterSkillStageSpriteAnimation> characterSkillStageSpriteAnimations;

        #endregion Members
    }

    public class CharacterSkillSpriteAnimationActionPlayer : MonoBehaviour, ICharacterSkillActionPlayer
    {
        #region Members

        [SerializeField]
        private SpriteAnimator _spriteAnimator;
        [SerializeField]
        private CharacterSkillSpriteAnimation[] _spriteAnimations;

        #endregion Members

        #region Class Methods

        public void Init(CharacterModel characterModel)
        {
        }

        public void Play(CharacterPlayedSkillAction characterPlayedSkillAction)
        {
            _spriteAnimator.AnimationStoppedAction = null;
            var skillType = characterPlayedSkillAction.skillType;
            var spriteAnimation = _spriteAnimations.FirstOrDefault(x => x.skillType == skillType);
            if (spriteAnimation != null)
            {
                var skillActionPhase = characterPlayedSkillAction.skillActionPhase;
                var characterSkillStageSpriteAnimation = spriteAnimation.characterSkillStageSpriteAnimations.FirstOrDefault(x => x.skillActionPhase == skillActionPhase);
                if (characterSkillStageSpriteAnimation != null)
                {
                    if (characterSkillStageSpriteAnimation.hasEventTriggeredAtAFrame)
                    {
                        _spriteAnimator.Play(characterSkillStageSpriteAnimation.mappedPrecastAnimationName,
                                             playOneShot: !characterSkillStageSpriteAnimation.isLoop,
                                             eventTriggeredCallbackAction: () => characterPlayedSkillAction.eventTriggeredCallbackAction.Invoke(),
                                             eventTriggeredFrame: characterSkillStageSpriteAnimation.eventTriggeredFrame);
                    }
                    else
                    {
                        _spriteAnimator.Play(characterSkillStageSpriteAnimation.mappedPrecastAnimationName,
                                             playOneShot: !characterSkillStageSpriteAnimation.isLoop);
                    }

                    if (characterPlayedSkillAction.endActionCallbackAction != null)
                        _spriteAnimator.AnimationStoppedAction = () => characterPlayedSkillAction.endActionCallbackAction.Invoke();
                }
                else
                {
                    characterPlayedSkillAction.eventTriggeredCallbackAction?.Invoke();
                    characterPlayedSkillAction.endActionCallbackAction?.Invoke();
                }
            }
            else
            {
                characterPlayedSkillAction.eventTriggeredCallbackAction?.Invoke();
                characterPlayedSkillAction.endActionCallbackAction?.Invoke();
            }
        }

        #endregion Class Methods
    }
}