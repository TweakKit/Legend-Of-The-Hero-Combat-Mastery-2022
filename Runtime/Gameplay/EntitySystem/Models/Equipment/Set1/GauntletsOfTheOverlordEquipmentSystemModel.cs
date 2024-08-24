using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class GauntletsOfTheOverlordEquipmentSystemModel : EquipmentSystemModel
    {
        #region Members

        private int _numberOfEnemyTriggerIncreaseAttack;
        private float _attackIncreasePercent;
        private int _numberOfEnemyTriggerExplodeDamage;
        private float _explodeDamagePercent;

        #endregion Members

        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.GauntletsOfTheOverlord;
        public int NumberOfEnemyTriggerIncreaseAttack => _numberOfEnemyTriggerIncreaseAttack;
        public float AttackIncreasePercent => _attackIncreasePercent;
        public int NumberOfEnemyTriggerExplodeDamage => _numberOfEnemyTriggerExplodeDamage;
        public float ExplodeDamagePercent => _explodeDamagePercent;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as GauntletsOfTheOverlordEquipmentMechanicDataConfigItem;
            _numberOfEnemyTriggerExplodeDamage = dataConfigItem.numberOfEnemyTriggerExplodeDamage;
            _numberOfEnemyTriggerIncreaseAttack = dataConfigItem.numberOfEnemyTriggerIncreaseAttack;
            _attackIncreasePercent = dataConfigItem.attackIncreasePercent;
            _explodeDamagePercent = dataConfigItem.explodeDamagePercent;
        }

        #endregion Class Methods
    }
}