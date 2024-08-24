using System;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class TrappedStatusEffect : PerIntervalDurationStatusEffect<TrappedStatusEffectModel>
    {
        #region Properties

        protected EntityModel senderModel;
        protected override float Interval => ownerModel.DamageInterval;
        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            this.senderModel = senderModel;
        }

        protected override void AffectPerInterval(CharacterModel receiverModel)
        {
            base.AffectPerInterval(receiverModel);
#if DEBUGGING
            Debug.Log($"damage_log|| owner: trapEffect | interval damage: {ownerModel.Damage}");
#endif
            receiverModel.DebuffHp(ownerModel.Damage, DamageSource.FromTrap, ownerModel.StatusEffectType == StatusEffectType.TrappedPoison ? DamageProperty.Poison : DamageProperty.None, senderModel);
        }

        protected override bool CheckStopAffect(CharacterModel receiverModel)
        {
            switch (ownerModel.TrappedSourceType)
            {
                case TrapType.Poison:
                    return !receiverModel.IsInTrappedPoisonStatus || base.CheckStopAffect(receiverModel);

                case TrapType.Fire:
                default:
                    return base.CheckStopAffect(receiverModel);
            }
        }

        #endregion Class methods
    }
}