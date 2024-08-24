using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{

    public class AstralLocketEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.AstralLocket;
        public float AppliedCritChance { get; private set; }
        public float EnemyHealthPercentTriggered { get; private set; }
        public float TriggeredCritChanceFactor { get; private set; }
        public bool CanApplyCritChanceForAllDamage => AppliedCritChance > 0;
        public bool CanTriggerCritChanceFactor => EnemyHealthPercentTriggered > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AstralLocketEquipmentMechanicDataConfigItem;
            AppliedCritChance = dataConfig.appliedCritChance;
            EnemyHealthPercentTriggered = dataConfig.enemyHealthPercentTriggered;
            TriggeredCritChanceFactor = dataConfig.triggeredCritChanceFactor;
        }

        #endregion Class Methods
    }
}
