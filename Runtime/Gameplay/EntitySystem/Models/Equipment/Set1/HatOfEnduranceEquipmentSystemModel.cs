using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class HatOfEnduranceEquipmentSystemModel : EquipmentSystemModel
    {
        #region Members

        private float _firstDamageReductionIncreasePercent;
        private float _firstHeatlhPercentTriggerThreshold;
        private float _secondDamageReductionIncreasePercent;
        private float _secondHealthPercentTriggerThreshold;

        #endregion Members

        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.HatOfEndurance;

        public float FirstDamageReductionIncreasePercent => _firstDamageReductionIncreasePercent;
        public float FirstHeatlhPercentTriggerThreshold => _firstHeatlhPercentTriggerThreshold;
        public float SecondDamageReductionIncreasePercent => _secondDamageReductionIncreasePercent;
        public float SecondHealthPercentTriggerThreshold => _secondHealthPercentTriggerThreshold;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as HatOfEnduranceEquipmentMechanicDataConfigItem;
            _firstDamageReductionIncreasePercent = dataConfigItem.firstDamageReductionIncreasePercent;
            _firstHeatlhPercentTriggerThreshold = dataConfigItem.firstHealthPercentTriggerThreshold;
            _secondDamageReductionIncreasePercent = dataConfigItem.secondDamageReductionIncreasePercent;
            _secondHealthPercentTriggerThreshold = dataConfigItem.secondHealthPercentTriggerThreshold;
        }

        #endregion Class Methods
    }
}