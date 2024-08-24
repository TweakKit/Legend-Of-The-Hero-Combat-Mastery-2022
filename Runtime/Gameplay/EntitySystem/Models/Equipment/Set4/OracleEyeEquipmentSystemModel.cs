using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class OracleEyeEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.OracleEye;
        public float IncreaseDamagePercent { get; private set; }
        public float IncreaseDamagePercentDuration { get; private set; }
        public bool CanBuffDamage => IncreaseDamagePercent > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as OracleEyeEquipmentMechanicDataConfigItem;
            IncreaseDamagePercent = dataConfigItem.increaseDamagePercent;
            IncreaseDamagePercentDuration = dataConfigItem.increaseDamagePercentDuration;
        }

        #endregion Class Methods
    }
}