using System;
using System.Linq;
using Runtime.Animation;
using Runtime.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class ExtendStateSpriteAnimation : StateSpriteAnimation 
    {
        public bool rotateByFaceDirection;
    }

    [Serializable]
    public class ExtendSpriteAnimator
    {
        public SpriteAnimator spriteAnimator;
        public bool canRotate;
        [ShowIf(nameof(canRotate))]
        public Transform rotateTransform;
        [ShowIf(nameof(canRotate))]
        public float rotateSpeed;
    }

    public class CharacterExtendSpriteAnimationPlayer : MonoBehaviour, ICharacterAnimationPlayer
    {
        #region Members

        [SerializeField]
        private ExtendSpriteAnimator[] _spriteAnimators;
        [SerializeField]
        private ExtendStateSpriteAnimation[] _stateSpriteAnimations;

        private bool _canRotate;
        private CharacterModel _ownerModel;

        #endregion Members

        #region API Methods

        private void Update()
        {
            if (_canRotate)
            {
                var toRotation = (-_ownerModel.FaceDirection).ToQuaternion(0);
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

        public void Init(CharacterModel ownerModel)
        {
            _ownerModel = ownerModel;
            _canRotate = false;
        }

        public void Play(CharacterAnimationState state)
        {
            var stateAnimation = _stateSpriteAnimations.FirstOrDefault(x => x.state == state);

            if(_canRotate && stateAnimation != null && !stateAnimation.rotateByFaceDirection)
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

            _canRotate = stateAnimation == null ? false : stateAnimation.rotateByFaceDirection;

            if (stateAnimation != null)
            {
                foreach (var item in _spriteAnimators)
                {
                    item.spriteAnimator.Play(stateAnimation.spriteAnimationName, playOneShot: !stateAnimation.isLoop);
                }
            }
        }

        public void Pause()
        {
            foreach (var item in _spriteAnimators)
            {
                item.spriteAnimator.Stop();
            }
        }

        public void Continue()
        {
            foreach (var item in _spriteAnimators)
            {
                item.spriteAnimator.Resume();
            }
        }

        public void TintColor(Color color)
        {
            foreach (var item in _spriteAnimators)
            {
                item?.spriteAnimator?.TintColor(color);
            }
        }

        #endregion Class Methods
    }
}