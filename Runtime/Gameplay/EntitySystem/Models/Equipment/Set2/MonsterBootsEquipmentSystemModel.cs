using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class MonsterBootsEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.MonsterBoots;
        public float BuffSpeedValue { get; private set; }
        public float BuffSpeedDuration { get; private set; }
        public float BuffDodgeChance { get; private set; }
        public bool CanBuffDodgeChance => BuffDodgeChance > 0;
        public bool CanBuff => BuffSpeedDuration > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as MonsterBootsEquipmentMechanicDataConfigItem;
            BuffSpeedValue = dataConfig.buffSpeedValue;
            BuffSpeedDuration = dataConfig.buffSpeedDuration;
            BuffDodgeChance = dataConfig.buffDodgeChance;
        }

        #endregion Class Methods
    }
}