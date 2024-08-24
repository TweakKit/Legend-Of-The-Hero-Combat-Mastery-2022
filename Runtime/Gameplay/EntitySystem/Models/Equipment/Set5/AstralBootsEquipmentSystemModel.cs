using System.Collections;
using System.Collections.Generic;
using Runtime.ConfigModel;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralBootsEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.AstralBoots;
        public float SpeedIncrease { get; private set; }
        public float TimeSpeedIncrease { get; private set; }
        public float DamagePercentIncrease { get; private set; }
        public bool CanBuffSpeed => SpeedIncrease > 0;
        public bool CanBuffDamage => DamagePercentIncrease > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AstralBootsEquipmentMechanicDataConfigItem;
            SpeedIncrease = dataConfig.speedIncrease;
            TimeSpeedIncrease = dataConfig.timeSpeedIncrease;
            DamagePercentIncrease = dataConfig.damagePercentIncrease;
        }

        #endregion Class Methods
    }
}
