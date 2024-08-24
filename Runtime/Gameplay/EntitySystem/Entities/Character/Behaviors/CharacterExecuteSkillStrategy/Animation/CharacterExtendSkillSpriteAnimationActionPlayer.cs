using System;
using System.Collections.Generic;
using System.Linq;
using Runtime.Animation;
using Runtime.Definition;
using Runtime.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class CharacterExtendSkillStageSpriteAnimation : CharacterSkillStageSpriteAnimation
    {
        public bool rotateByFaceDirection;
    }

    [Serializable]
    public class CharacterExtendSkillSpriteAnimation
    {
        #region Members

        public SkillType skillType;
        public List<CharacterExtendSkillStageSpriteAnimation> characterSkillStageSpriteAnimations;

        #endregion Members
    }

    [Serializable]
    public class ExtendSkillSpriteAnimator
    {
        public SpriteAnimator spriteAnimator;
        public bool canRotate;
        [ShowIf(nameof(canRotate))]
        public Transform rotateTransform;
        [ShowIf(nameof(canRotate))]
        public float rotateSpeed;
    }

    public class CharacterExtendSkillSpriteAnimationActionPlayer : MonoBehaviour, ICharacterSkillActionPlayer
    {
        #region Members

        [SerializeField]
        private SpriteAnimator _controlSpriteAnimator;
        [SerializeField]
        private ExtendSkillSpriteAnimator[] _spriteAnimators;
        [SerializeField]
        private CharacterExtendSkillSpriteAnimation[] _spriteAnimations;

        private CharacterModel _ownerModel;
        private bool _canRotate;

        #endregion Members

        #region API Methods

        private void Update()
        {
            if (_canRotate)
            {
                Quaternion toRotation = Quaternion.identity;
                if (_ownerModel.FaceRight)
                {
                    toRotation = (_ownerModel.FaceDirection).ToQuaternion(0);
                }
                else
                {
                    toRotation = (-_ownerModel.FaceDirection).ToQuaternion(0);
                }
                foreach (var item in _spriteAnimators)
                {
                    if (item.canRotate)
                    {
                        var itemTransform = item.rotateTransform;
                        itemTransform.rotation = Quaternion.RotateTowards(itemTransform.rotation, toRotation, item.rotateSpeed * Time.deltaTime);
                        var degree = Quaternion.Angle(itemTransform.rotation, Quaternion.identity);
                        if (degree > 90 || degree < -90)
                            itemTransform.localScale = new Vector3(1, -1, 1);
                        else
                            itemTransform.localScale = new Vector3(1, 1, 1);
                    }
                }
            }
        }

        #endregion API Methods

        #region Class Methods

        public void Init(CharacterModel characterModel)
        {
            _ownerModel = characterModel;
            _canRotate = false;
        }

        public void Play(CharacterPlayedSkillAction characterPlayedSkillAction)
        {
            _controlSpriteAnimator.AnimationStoppedAction = null;
            var skillType = characterPlayedSkillAction.skillType;
            var spriteAnimation = _spriteAnimations.FirstOrDefault(x => x.skillType == skillType);
            if (spriteAnimation != null)
            {
                var skillActionPhase = characterPlayedSkillAction.skillActionPhase;
                var characterSkillStageSpriteAnimation = spriteAnimation.characterSkillStageSpriteAnimations.FirstOrDefault(x => x.skillActionPhase == skillActionPhase);
                if (characterSkillStageSpriteAnimation != null)
                {
                    if (_canRotate && !characterSkillStageSpriteAnimation.rotateByFaceDirection)
                    {
                        foreach (var item in _spriteAnimators)
                        {
                            if (item.canRotate)
                            {
                                var itemTransform = item.rotateTransform;
                                itemTransform.rotation = Quaternion.identity;
                            }
                        }
                    }
                    _canRotate = characterSkillStageSpriteAnimation.rotateByFaceDirection;

                    if (characterSkillStageSpriteAnimation.hasEventTriggeredAtAFrame)
                    {
                        _controlSpriteAnimator.Play(characterSkillStageSpriteAnimation.mappedPrecastAnimationName,
                                             playOneShot: !characterSkillStageSpriteAnimation.isLoop,
                                             eventTriggeredCallbackAction: () => characterPlayedSkillAction.eventTriggeredCallbackAction.Invoke(),
                                             eventTriggeredFrame: characterSkillStageSpriteAnimation.eventTriggeredFrame);
                    }
                    else
                    {
                        _controlSpriteAnimator.Play(characterSkillStageSpriteAnimation.mappedPrecastAnimationName,
                                             playOneShot: !characterSkillStageSpriteAnimation.isLoop);
                    }

                    foreach (var item in _spriteAnimators)
                    {
                        if(!item.spriteAnimator.Equals(_controlSpriteAnimator))
                            item.spriteAnimator.Play(characterSkillStageSpriteAnimation.mappedPrecastAnimationName, playOneShot: !characterSkillStageSpriteAnimation.isLoop);
                    }

                    if (characterPlayedSkillAction.endActionCallbackAction != null)
                        _controlSpriteAnimator.AnimationStoppedAction = () => characterPlayedSkillAction.endActionCallbackAction.Invoke();
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