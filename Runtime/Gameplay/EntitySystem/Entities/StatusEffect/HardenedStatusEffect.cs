using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class HardenedStatusEffect : DurationStatusEffect<HardenedStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.BuffStat(StatType.Armor, ownerModel.IncreasedArmor, StatModifyType.BaseMultiply);
            receiverModel.DebuffStat(StatType.MoveSpeed, ownerModel.DecreasedMoveSpeed, StatModifyType.BaseMultiply);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.DebuffStat(StatType.Armor, ownerModel.IncreasedArmor, StatModifyType.BaseMultiply);
            receiverModel.BuffStat(StatType.MoveSpeed, ownerModel.DecreasedMoveSpeed, StatModifyType.BaseMultiply);
        }

        #endregion Class Methods
    }
}