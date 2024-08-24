using System;
using System.Linq;
using UnityEngine;
using Runtime.Animation;
using Sirenix.OdinInspector;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class StateSpriteAnimation
    {
        #region Members

        public string spriteAnimationName;
        public CharacterAnimationState state;
        public bool isLoop;

        public bool haveEvent;
        [ShowIf(nameof(haveEvent))]
        public int frameTriggeredEvent;
        [ShowIf(nameof(haveEvent))]
        public Transform[] spawnPointsTransform;

        #endregion Members
    }

    public class CharacterSpriteAnimationPlayer : MonoBehaviour, ICharacterAnimationPlayer
    {
        #region Members

        [SerializeField]
        private SpriteAnimator _spriteAnimator;
        [SerializeField]
        private StateSpriteAnimation[] _stateSpriteAnimations;

        #endregion Members

        #region Class Methods

        public void Init(CharacterModel ownerModel)
        {
        }

        public void Play(CharacterAnimationState state)
        {
            var stateAnimation = _stateSpriteAnimations.FirstOrDefault(x => x.state == state);
            if (stateAnimation != null)
                _spriteAnimator.Play(stateAnimation.spriteAnimationName, playOneShot: !stateAnimation.isLoop);
        }

        public void Pause()
            => _spriteAnimator.Stop();

        public void Continue()
            => _spriteAnimator.Resume();

        public void TintColor(Color color)
            => _spriteAnimator?.TintColor(color);

        #endregion Class Methods
    }
}