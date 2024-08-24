using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class DreadStatusEffect : DurationStatusEffect<DreadStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.BuffStat(StatType.LifeSteal, ownerModel.IncreasedLifesteal, StatModifyType.BaseBonus);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.DebuffStat(StatType.LifeSteal, ownerModel.IncreasedLifesteal, StatModifyType.BaseBonus);
        }

        #endregion Class Methods
    }
}