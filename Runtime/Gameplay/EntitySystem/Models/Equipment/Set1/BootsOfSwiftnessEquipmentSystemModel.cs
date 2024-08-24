using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class BootsOfSwiftnessEquipmentSystemModel : EquipmentSystemModel
    {
        #region Members

        private float _healthThresholdTrigger;
        private float _moveSpeedIncreaseScale;
        private float _numberOfEnemyTrigger;
        private float _moveSpeedIncrease;

        #endregion Members

        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.BootsOfSwiftness;
        public float HealthThresholdTrigger => _healthThresholdTrigger;
        public float MoveSpeedIncreaseScale => _moveSpeedIncreaseScale;
        public float NumberOfEnemyTrigger => _numberOfEnemyTrigger;
        public float MoveSpeedIncrease => _moveSpeedIncrease;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as BootsOfSwiftnessEquipmentMechanicDataConfigItem;
            _healthThresholdTrigger = dataConfigItem.healthThresholdTrigger;
            _moveSpeedIncrease = dataConfigItem.moveSpeedIncrease;
            _moveSpeedIncreaseScale = dataConfigItem.moveSpeedIncreaseScale;
            _numberOfEnemyTrigger = dataConfigItem.numberOfEnemyTrigger;
        }

        #endregion Class Methods
    }
}