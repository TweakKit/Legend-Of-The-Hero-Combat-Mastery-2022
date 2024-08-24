using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvisibleGlovesEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.InvisibleGloves;
        public float IncreasedDamagePercentToEnemy { get; private set; }
        public float InstantKillRate { get; private set; }
        public float TriggeredIncreaseDamageHealthPercent { get; private set; }
        public float TriggeredInstantKillHealthPercent { get; private set; }
        public bool CanIncreaseDamageTowardEnemy => TriggeredIncreaseDamageHealthPercent > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as InvisibleGlovesEquipmentMechanicDataConfigItem;
            IncreasedDamagePercentToEnemy = dataConfig.increasedDamagePercentToEnemy;
            InstantKillRate = dataConfig.instantKillRate;
            TriggeredIncreaseDamageHealthPercent = dataConfig.triggeredIncreaseDamageHealthPercent;
            TriggeredInstantKillHealthPercent = dataConfig.triggeredInstantKillHealthPercent;
        }

        #endregion Class Methods
    }
}
