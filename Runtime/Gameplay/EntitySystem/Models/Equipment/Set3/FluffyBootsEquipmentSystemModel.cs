using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class FluffyBootsEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.FluffyBoots;
        public float IncreasedDamagePercent { get; private set; }
        public float IncreaseMoveSpeedValue { get; private set; }
        public float LifeTimeIncreasedMoveSpeed { get; private set; }
        public float CountTimeTriggerIncreaseDamage { get; private set; }
        public bool CanBuffDamage => IncreasedDamagePercent > 0;
        public bool CanBuffSpeed => LifeTimeIncreasedMoveSpeed > 0 && IncreaseMoveSpeedValue > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as FluffyBootsEquipmentMechanicDataConfigItem;
            IncreasedDamagePercent = dataConfig.increasedDamagePercent;
            IncreaseMoveSpeedValue = dataConfig.increaseMoveSpeedValue;
            LifeTimeIncreasedMoveSpeed = dataConfig.lifeTimeIncreasedMoveSpeed;
            CountTimeTriggerIncreaseDamage = dataConfig.countTimeTriggerIncreaseDamage;
        }

        #endregion Class Methods
    }
}