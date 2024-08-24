using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class QuickStatusEffect : DurationStatusEffect<QuickStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.BuffStat(StatType.AttackSpeed, ownerModel.IncreasedAttackSpeed, StatModifyType.BaseMultiply);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.DebuffStat(StatType.AttackSpeed, ownerModel.IncreasedAttackSpeed, StatModifyType.BaseMultiply);
        }

        #endregion Class Methods
    }
}