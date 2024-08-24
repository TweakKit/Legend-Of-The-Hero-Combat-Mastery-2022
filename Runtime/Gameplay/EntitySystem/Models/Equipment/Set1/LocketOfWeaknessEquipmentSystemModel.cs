using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class LocketOfWeaknessEquipmentSystemModel : EquipmentSystemModel
    {
        #region Members

        private StatusEffectType[] _effectStatusTypes;
        private float _damageIncreasePercent;
        private bool _applyForControlMoveStatus;

        #endregion Members

        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.LocketOfWeakness;
        public StatusEffectType[] EffectStatusTypes => _effectStatusTypes;
        public float DamageIncreasePercent => _damageIncreasePercent;
        public bool ApplyForControlMoveStatus => _applyForControlMoveStatus;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfigItem = equipmentMechanicDataConfigItem as LocketOfWeaknessEquipmentMechanicDataConfigItem;
            _effectStatusTypes = dataConfigItem.effectStatusTypes;
            _damageIncreasePercent = dataConfigItem.damageIncreasePercent;
            _applyForControlMoveStatus = dataConfigItem.applyForControlMoveStatus;
        }

        #endregion Class Methods
    }
}