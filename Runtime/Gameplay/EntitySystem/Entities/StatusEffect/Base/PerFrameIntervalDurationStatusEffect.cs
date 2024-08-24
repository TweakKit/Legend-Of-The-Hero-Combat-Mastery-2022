using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// Per frame and interval duration status effect also runs tasks in every frame and every interval.
    /// </summary>
    public abstract class PerFrameIntervalDurationStatusEffect<T> : DurationStatusEffect<T> where T : StatusEffectModel
    {
        #region Members

        private float _currentAffectInterval;

        #endregion Members

        #region Properties

        protected abstract float Interval { get; }

        #endregion Properties

        #region Class Methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            _currentAffectInterval = 0.0f;
        }

        public override void Update()
        {
            _currentAffectInterval += Time.deltaTime;
            AffectPerFrame(affectedModel);
            if (currentAffectDuration > 0 && _currentAffectInterval >= Interval)
            {
                _currentAffectInterval = 0.0f;
                AffectPerInterval(affectedModel);
            }
            base.Update();
        }

        protected virtual void AffectPerFrame(CharacterModel receiverModel) { }
        protected virtual void AffectPerInterval(CharacterModel receiverModel) { }

        #endregion Class methods
    }
}