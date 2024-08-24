using System;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class FreezeStatusEffect : DurationStatusEffect<FreezeStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingFreeze();
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingFreeze();
        }

        #endregion Class methods
    }
}