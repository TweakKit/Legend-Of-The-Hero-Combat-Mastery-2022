using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class LeatherJacketEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.LeatherJacket;
        public float BuffedLifeStealPercent { get; private set; }
        public float Cooldown { get; private set; }
        public float HealthPercentCreateShield { get; private set; }
        public bool CanBuffShield => HealthPercentCreateShield > 0 && Cooldown > 0;
        public bool CanBuffLifeSteal => BuffedLifeStealPercent > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as LeatherJacketEquipmentMechanicDataConfigItem;
            BuffedLifeStealPercent = dataConfig.buffedLifeStealPercent;
            Cooldown = dataConfig.cooldown;
            HealthPercentCreateShield = dataConfig.healthPercentCreateShield;
        }

        #endregion Class Methods
    }
}
