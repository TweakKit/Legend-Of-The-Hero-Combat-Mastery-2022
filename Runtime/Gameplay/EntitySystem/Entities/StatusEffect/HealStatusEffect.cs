using System;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class HealStatusEffect : OneShotStatusEffect<HealStatusEffectModel>
    {
        #region Properties


        #endregion Properties

        #region Class methods

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.BuffHp(ownerModel.HealAmount, damageSource: ownerModel.DamageSource);
            ownerModel.FinishedEvent?.Invoke();
        }

        #endregion Class methods
    }
}