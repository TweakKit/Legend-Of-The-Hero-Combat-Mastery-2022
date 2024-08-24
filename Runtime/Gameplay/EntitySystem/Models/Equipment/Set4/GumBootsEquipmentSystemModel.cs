using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class GumBootsEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.GumBoots;
        public float DelayToCreateDamageZone { get; private set; }
        public float DamageZoneExistTime { get; private set; }
        public float DamageZoneTriggeredInterval { get; private set; }
        public float CreatedDamagePercent { get; private set; }
        public StatusEffectModel[] DamageZoneModifierIdentities { get; private set; }
        public bool CanCreateDamageZone { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as GumBootsEquipmentMechanicDataConfigItem;
            DelayToCreateDamageZone = dataConfigItem.delayToCreateDamageZone;
            DamageZoneExistTime = dataConfigItem.damageZoneExistTime;
            DamageZoneTriggeredInterval = dataConfigItem.damageZoneTriggeredInterval;
            CreatedDamagePercent = dataConfigItem.createdDamagePercent;
            DamageZoneModifierIdentities = GetModifierModels(nameof(dataConfigItem.damageZoneModifierIdentities));
            CanCreateDamageZone = DelayToCreateDamageZone > 0 && DamageZoneExistTime > 0;
        }

        #endregion Class Methods
    }
}