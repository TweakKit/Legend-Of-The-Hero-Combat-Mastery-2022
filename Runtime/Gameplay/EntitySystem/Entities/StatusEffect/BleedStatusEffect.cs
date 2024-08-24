namespace Runtime.Gameplay.EntitySystem
{
    public class BleedStatusEffect : PerIntervalDurationStatusEffect<BleedStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;
        protected override float Interval => ownerModel.Interval;

        #endregion Properties

        #region Class methods

        protected override void AffectPerInterval(CharacterModel receiverModel)
        {
            base.AffectPerInterval(receiverModel);
            receiverModel.DebuffHp(ownerModel.Damage, DamageSource.FromOther, DamageProperty.None, null);
        }

        #endregion Class methods
    }
}