using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralGauntletEquipmentSystemModel : EquipmentSystemModel
    {
        #region Properties

        public override EquipmentSystemType EquipmentSystemType => EquipmentSystemType.AstralGaunlet;
        public float EffectDuration { get; private set; }
        public float Cooldown { get; private set; }
        public float IncreaseDamageReceivePercent { get; private set; }
        public float RangeEffectAfterEnemyDie { get; private set; }

        public bool CanApplyEffect => IncreaseDamageReceivePercent > 0 && Cooldown > 0 && EffectDuration > 0;
        public bool CanApplyArroundAfterFinished => RangeEffectAfterEnemyDie > 0;

        #endregion Properties

        #region Class Methods

        public override void Init(EquipmentMechanicDataConfigItem equipmentMechanicDataConfigItem)
        {
            var dataConfig = equipmentMechanicDataConfigItem as AstralGaunletEquipmentMechanicDataConfigItem;
            EffectDuration = dataConfig.effectDuration;
            Cooldown = dataConfig.cooldown;
            IncreaseDamageReceivePercent = dataConfig.increaseDamageReceivePercent;
            RangeEffectAfterEnemyDie = dataConfig.rangeEffectAfterEnemyDie;
        }

        #endregion Class Methods
    }
}