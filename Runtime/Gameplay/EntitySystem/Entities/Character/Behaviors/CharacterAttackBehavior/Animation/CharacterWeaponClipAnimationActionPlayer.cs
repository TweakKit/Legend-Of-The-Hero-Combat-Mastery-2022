using System;
using System.Linq;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class CharacterWeaponClipAnimation
    {
        #region Members

        public CharacterWeaponAnimationType animationType;
        public string mappedAnimationName;

        #endregion Members
    }

    [RequireComponent(typeof(Animator))]
    public class CharacterWeaponClipAnimationActionPlayer : MonoBehaviour, ICharacterWeaponActionPlayer
    {
        #region Members

        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private CharacterWeaponClipAnimation[] _clipAnimations;

        #endregion Members

        #region Properties

        private Action WeaponOperatedPointTriggeredCallbackAction { get; set; }
        private Action WeaponEndActionCallbackAction { get; set; }

        #endregion Properties

        #region Class Methods

        public void Init() { }

        public void Cancel()
            => Play(new CharacterPlayedWeaponAction(CharacterWeaponAnimationType.Idle));

        public void Play(CharacterPlayedWeaponAction characterPlayedWeaponAction)
        {
            ClearEvents();
            var clipAnimation = _clipAnimations.FirstOrDefault(x => x.animationType == characterPlayedWeaponAction.animationType);
            if (clipAnimation != null)
            {
                _animator.Play(clipAnimation.mappedAnimationName, 0, 0);

                if (characterPlayedWeaponAction.operatedPointTriggeredCallbackAction != null)
                    WeaponOperatedPointTriggeredCallbackAction = () => characterPlayedWeaponAction.operatedPointTriggeredCallbackAction.Invoke();

                if (characterPlayedWeaponAction.endActionCallbackAction != null)
                {
                    WeaponEndActionCallbackAction = () => {
                        characterPlayedWeaponAction.endActionCallbackAction.Invoke();
                        Play(new CharacterPlayedWeaponAction(CharacterWeaponAnimationType.Idle));
                    };
                }
            }
            else
            {
#if DEBUGGING
                Debug.LogError("No clip animation on the curent used weapon!");
#endif
            }
        }

        private void ClearEvents()
        {
            WeaponOperatedPointTriggeredCallbackAction = null;
            WeaponEndActionCallbackAction = null;
        }

        #endregion Class Methods

        #region Unity Animation Callback Event Methods

        public void TriggerWeaponOperatedPointActionEvent()
            => WeaponOperatedPointTriggeredCallbackAction?.Invoke();

        public void TriggerWeaponEndActionEvent()
            => WeaponEndActionCallbackAction?.Invoke();

        #endregion Unity Animation Callback Event Methods
    }
}