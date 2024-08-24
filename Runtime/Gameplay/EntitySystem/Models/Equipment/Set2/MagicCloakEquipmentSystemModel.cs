using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicCloakEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.MagicCloak;
        public float RateConvertDamageReceivedToHealth { get; private set; }
        public float DamageReceivedToHealthPercent { get; private set; }
        public float TriggerHealthPercent { get; private set; }
        public float RateBonusConvert { get; private set; }
        public bool CanApplyRateBonusConvert => RateBonusConvert > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as MagicCloakEquipmentMechanicDataConfigItem;
            RateConvertDamageReceivedToHealth = dataConfig.rateConvertDamageReceivedToHealth;
            DamageReceivedToHealthPercent = dataConfig.damageReceivedToHealthPercent;
            TriggerHealthPercent = dataConfig.triggerHealthPercent;
            RateBonusConvert = dataConfig.rateBonusConvert;
        }

        #endregion Class Methods
    }
}