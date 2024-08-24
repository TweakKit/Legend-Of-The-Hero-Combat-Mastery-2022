using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class HasteStatusEffect : DurationStatusEffect<HasteStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
            receiverModel.BuffStat(StatType.MoveSpeed, ownerModel.IncreasedMoveSpeed, StatModifyType.BaseMultiply);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.DebuffStat(StatType.MoveSpeed, ownerModel.IncreasedMoveSpeed, StatModifyType.BaseMultiply);
        }

        #endregion Class Methods
    }
}