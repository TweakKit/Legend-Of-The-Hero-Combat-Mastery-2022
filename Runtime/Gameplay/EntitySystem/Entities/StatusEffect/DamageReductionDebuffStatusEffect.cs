using Cysharp.Threading.Tasks;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class DamageReductionDebuffStatusEffect : DurationStatusEffect<DamageReductionDebuffStatusEffectModel>
    {
        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region API Methods


        #endregion API Methods

        #region Class Methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.DebuffStat(StatType.DamageReduction, ownerModel.DebuffPercent, StatModifyType.BaseBonus);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.BuffStat(StatType.DamageReduction, ownerModel.DebuffPercent, StatModifyType.BaseBonus);
        }

        #endregion Class Methods
    }
}