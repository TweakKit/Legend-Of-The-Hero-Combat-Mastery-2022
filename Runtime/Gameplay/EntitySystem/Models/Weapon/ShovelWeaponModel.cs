using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class ShovelWeaponModel : CooldownTimeSpecialWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.Shovel;
        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.CooldownTime;

        public float IncreaseSpeed { get; private set; }
        public float DurationIncreaseSpeed { get; private set; }
        public int MaxStackIncreaseSpeed { get; private set; }
        public bool CanIncreaseSpeed => IncreaseSpeed > 0 && DurationIncreaseSpeed > 0 && MaxStackIncreaseSpeed > 0;

        public int NumberDamageToTriggerStatus { get; private set; }
        public StatusEffectModel[] TriggeredDamageModifierModels { get; private set; }
        public bool CanTriggerStatus => NumberDamageToTriggerStatus > 0 && TriggeredDamageModifierModels != null && TriggeredDamageModifierModels.Length > 0;

        public float SpecialDuration { get; private set; }
        public float IntervalBetweenSpecialDamage { get; private set; }
        public float SpecialDamageBonus { get; private set; }
        public DamageFactor[] SpecialDamageFactors { get; private set; }
        public StatusEffectModel[] SpecialModifierModels { get; private set; }

        #endregion Properties

        #region Class Methods

        public ShovelWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            ShovelWeaponDataConfigItem dataConfigItem = weaponData.weaponConfigItem as ShovelWeaponDataConfigItem;
            ShovelEquipmentMechanicDataConfigItem mechanicDataConfigItem = weaponData.mechanicDataConfigItem as ShovelEquipmentMechanicDataConfigItem;
            IncreaseSpeed = mechanicDataConfigItem.increaseSpeed;
            DurationIncreaseSpeed = mechanicDataConfigItem.durationIncreaseSpeed;
            MaxStackIncreaseSpeed = mechanicDataConfigItem.maxStackIncreaseSpeed;
            NumberDamageToTriggerStatus = mechanicDataConfigItem.numberDamageToTriggerStatus;
            TriggeredDamageModifierModels = weaponData.GetModifierModels(nameof(mechanicDataConfigItem.triggeredStatusEffect));

            SpecialAttackCooldownTime = dataConfigItem.specialCooldown;
            SpecialDuration = dataConfigItem.specialDuration;
            IntervalBetweenSpecialDamage = dataConfigItem.intervalBetweenSpecialDamage;
            SpecialDamageBonus = dataConfigItem.specialDamageBonus;
            SpecialDamageFactors = dataConfigItem.specialDamageFactors;
            SpecialModifierModels = weaponData.GetModifierModels(nameof(dataConfigItem.specialDamageModifierIdentities));
        }

        #endregion Class Methods
    }
}