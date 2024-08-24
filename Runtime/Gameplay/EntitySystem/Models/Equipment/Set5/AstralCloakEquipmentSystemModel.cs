using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralCloakEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.AstralCloak;
        public float RecoverHealthPercent { get; private set; }
        public float AttackIncreasePercent { get; private set; }
        public float AttackSpeedIncreasePercent { get; private set; }
        public bool CanBuffWhenRecovered => AttackIncreasePercent > 0 || AttackSpeedIncreasePercent > 0;
        public bool CanRecoverHealth => RecoverHealthPercent > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AstralCloakEquipmentMechanicDataConfigItem;
            RecoverHealthPercent = dataConfig.recoverHealthPercent;
            AttackIncreasePercent = dataConfig.attackIncreasePercent;
            AttackSpeedIncreasePercent = dataConfig.attackSpeedIncreasePercent;
        }

        #endregion Class Methods
    }
}
