using System;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class RegenStatusEffect : PerIntervalDurationStatusEffect<RegenStatusEffectModel>
    {
        #region Properties

        protected override float Interval => ownerModel.HealInterval;
        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void AffectPerInterval(CharacterModel receiverModel)
        {
            base.AffectPerInterval(receiverModel);
            receiverModel.BuffHp(ownerModel.HealAmountPerInterval);
        }

        #endregion Class Methods
    }
}