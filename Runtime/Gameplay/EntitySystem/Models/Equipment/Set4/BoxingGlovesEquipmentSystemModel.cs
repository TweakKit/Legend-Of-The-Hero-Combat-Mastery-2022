using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class BoxingGlovesEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.BoxingGloves;
        public StatusEffectModel[] CreatedDamageModifierModels { get; private set; }
        public float CreatedDamagePercent { get; private set; }
        public string PrefabExplodeName { get; private set; }
        public bool CanCreatedExplode => (CreatedDamagePercent > 0 || (CreatedDamageModifierModels != null && CreatedDamageModifierModels.Length > 0)) && !string.IsNullOrEmpty(PrefabExplodeName);

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as BoxingGlovesEquipmentMechanicDataConfigItem;
            CreatedDamageModifierModels = GetModifierModels(nameof(dataConfigItem.createdDamageModifierIdentities));
            CreatedDamagePercent = dataConfigItem.createdDamagePercent;
            PrefabExplodeName = dataConfigItem.prefabExplodeName;
        }

        #endregion Class Methods
    }
}