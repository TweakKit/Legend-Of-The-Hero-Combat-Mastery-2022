using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class AntlerEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.Antler;
        public float TriggeredRegenHealthPercent { get; private set; }
        public float RegenHealthPercentPerSecond { get; private set; }
        public float TriggeredScaleHealHealthPercent { get; private set; }
        public float ScaleHealEffectValue { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AntlerEquipmentMechanicDataConfigItem;
            TriggeredRegenHealthPercent = dataConfig.triggeredRegenHealthPercent;
            RegenHealthPercentPerSecond = dataConfig.regenHealthPercentPerSecond;
            TriggeredScaleHealHealthPercent = dataConfig.triggeredScaleHealHealthPercent;
            ScaleHealEffectValue = dataConfig.scaleHealEffectValue;
        }

        #endregion Class Methods
    }

}