using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class RoyalCrownEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.RoyalCrown;
        public float AffectRange { get; private set; }
        public StatusEffectModel[] SendToEnemiesModifierModels { get; private set; }
        public bool CanAffect { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as RoyalCrownEquipmentMechanicDataConfigItem;
            AffectRange = dataConfigItem.affectRange;
            SendToEnemiesModifierModels = GetModifierModels(nameof(dataConfigItem.sendToEnemiesModifierIdentities));
            CanAffect = AffectRange > 0 && SendToEnemiesModifierModels != null;
        }

        #endregion Class Methods
    }
}