using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralHelmEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.AstralHelm;
        public int NumberOfEnemyTriggerIncreaseDodgeChance { get; private set; }
        public float DodgeChanceIncreasePercent { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AstralHelmEquipmentMechanicDataConfigItem;
            NumberOfEnemyTriggerIncreaseDodgeChance = dataConfig.numberOfEnemyTriggerIncreaseDodgeChance;
            DodgeChanceIncreasePercent = dataConfig.dodgeChanceIncreasePercent;
        }

        #endregion Class Methods
    }
}