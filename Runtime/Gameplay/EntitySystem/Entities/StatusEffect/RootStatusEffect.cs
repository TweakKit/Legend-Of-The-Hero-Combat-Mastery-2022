using System;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class RootStatusEffect : DurationStatusEffect<RootStatusEffectModel>
    {
        #region Members

        private float _lastTotalMultiplyValue;

        #endregion Members

        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
        }

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            _lastTotalMultiplyValue = receiverModel.GetTotalMultiplyStatValue(StatType.MoveSpeed);
            receiverModel.DebuffStat(StatType.MoveSpeed, _lastTotalMultiplyValue, StatModifyType.TotalMultiply);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.BuffStat(StatType.MoveSpeed, _lastTotalMultiplyValue, StatModifyType.TotalMultiply);
        }

        #endregion Class methods
    }
}