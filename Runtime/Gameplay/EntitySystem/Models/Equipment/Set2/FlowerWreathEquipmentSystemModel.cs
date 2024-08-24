using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlowerWreathEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.FlowerWreath;
        public float RateConvertDamageToHealth { get; private set; }
        public float DamageToHealthPercent { get; private set; }
        public float HealCritPercent { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as FlowerWreathEquipmentMechanicDataConfigItem;
            RateConvertDamageToHealth = dataConfig.rateConvertDamageToHealth;
            DamageToHealthPercent = dataConfig.damageToHealthPercent;
            HealCritPercent = dataConfig.healCritPercent;
        }

        #endregion Class Methods
    }
}