using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// Duration status effect affects the target in duration.
    /// Note: This doesn't do any extra task in every frame or/add every interval.
    /// </summary>
    public abstract class DurationStatusEffect<T> : OneShotStatusEffect<T> where T : StatusEffectModel
    {
        #region Members

        protected float currentAffectDuration;

        #endregion Members

        #region Properties

        protected abstract float Duration { get; }

        #endregion Properties

        #region Class Methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            currentAffectDuration = Duration;
            HasFinished = false;
        }

        public override void Update()
        {
            currentAffectDuration -= Time.deltaTime;
            if (currentAffectDuration <= 0 && !HasFinished)
            {
                FinishAffect(affectedModel);
                HasFinished = true;
            }

            if (CheckStopAffect(affectedModel))
                Stop();
        }

        public override void Stop()
        {
            FinishAffect(affectedModel);
            HasFinished = true;
            base.Stop();
        }

        protected virtual void FinishAffect(CharacterModel receiverModel) => ownerModel.FinishedEvent?.Invoke();
        protected virtual bool CheckStopAffect(CharacterModel receiverModel) => receiverModel.IsDead;

        #endregion Class methods
    }
}