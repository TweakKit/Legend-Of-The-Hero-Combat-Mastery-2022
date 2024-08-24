using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class PuppyGlovesEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.PuppyGloves;
        public float NextDamageBuffPercent { get; private set; }
        public float NextDamageCritRate { get; private set; }
        public bool CanApplyDamageCritRate => NextDamageCritRate > 0;

        public bool CanBuffDamage => NextDamageBuffPercent > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as PuppyGlovesEquipmentMechanicDataConfigItem;
            NextDamageCritRate = dataConfig.nextDamageCritRate;
            NextDamageBuffPercent = dataConfig.nextDamageBuffPercent;
        }

        #endregion Class Methods

    }
}