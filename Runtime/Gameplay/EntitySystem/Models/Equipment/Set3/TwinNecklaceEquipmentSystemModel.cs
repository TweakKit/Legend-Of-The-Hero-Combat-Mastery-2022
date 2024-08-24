using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class TwinNecklaceEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.TwinNecklace;
        public float TriggeredHealthPercent { get; private set; }
        public float IncreaseDamagePercent { get; private set; }
        public float IncreaseMoveSpeed { get; private set; }
        public bool CanBuffDamage => TriggeredHealthPercent > 0;
        public bool CanBuffSpeed => IncreaseMoveSpeed > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as TwinNecklaceEquipmentMechanicDataConfigItem;
            TriggeredHealthPercent = dataConfig.triggeredHealthPercent;
            IncreaseDamagePercent = dataConfig.increaseDamagePercent;
            IncreaseMoveSpeed = dataConfig.increaseMoveSpeed;
        }

        #endregion Class Methods
    }
}