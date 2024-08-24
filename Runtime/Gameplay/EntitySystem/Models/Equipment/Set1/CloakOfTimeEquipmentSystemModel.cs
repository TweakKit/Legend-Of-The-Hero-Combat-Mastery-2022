using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class CloakOfTimeEquipmentSystemModel : EquipmentSystemModel
    {
        #region Members

        private float _projectileSpeedDecreasePercent;
        private float _areaCooldown;
        private float _areaLifeTime;

        #endregion Members

        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.CloakOfTime;
        public float ProjectileSpeedDecreasePercent => _projectileSpeedDecreasePercent;
        public float AreaCooldown => _areaCooldown;
        public float AreaLifeTime => _areaLifeTime;
        public bool CanTriggerSpawnArea => AreaCooldown > 0 && AreaLifeTime > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as CloakOfTimeEquipmentMechanicDataConfigItem;
            _projectileSpeedDecreasePercent = dataConfigItem.projectileSpeedDecreasePercent;
            _areaCooldown = dataConfigItem.areaCooldown;
            _areaLifeTime = dataConfigItem.areaLifeTime;
        }

        #endregion Class Methods
    }
}