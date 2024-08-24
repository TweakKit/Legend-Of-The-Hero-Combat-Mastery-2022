using System;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class StunStatusEffect : DurationStatusEffect<StunStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingStun();
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingStun();
        }

        #endregion Class methods
    }
}