using System;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class ToughStatusEffect : DurationStatusEffect<ToughStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.BuffStat(StatType.AttackDamage, ownerModel.IncreasedAttackDamage, StatModifyType.BaseMultiply);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.DebuffStat(StatType.AttackDamage, ownerModel.IncreasedAttackDamage, StatModifyType.BaseMultiply);
        }

        #endregion Class Methods
    }
}