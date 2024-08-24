using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpikeVestEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.SpikeVest;
        public float DelayToCreateProtectiveCircle { get; private set; }
        public float ProtectiveCircleExistTime { get; private set; }
        public StatusEffectModel[] ProtectiveCircleCounterModifierIdentities { get; private set; }
        public bool CanCreateProtectiveCircle { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as SpikeVestEquipmentMechanicDataConfigItem;
            DelayToCreateProtectiveCircle = dataConfigItem.delayToCreateProtectiveCircle;
            ProtectiveCircleExistTime = dataConfigItem.protectiveCircleExistTime;
            ProtectiveCircleCounterModifierIdentities = GetModifierModels(nameof(dataConfigItem.protectiveCircleCounterModifierIdentities));
            CanCreateProtectiveCircle = DelayToCreateProtectiveCircle > 0 && ProtectiveCircleExistTime > 0 && ProtectiveCircleCounterModifierIdentities != null;
        }

        #endregion Class Methods
    }
}