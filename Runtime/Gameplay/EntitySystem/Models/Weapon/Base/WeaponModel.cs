using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public enum WeaponSpecialActivateType
    {
        CooldownTime,
        CountdownCounter,
        ConditionalTrigger,
        CountdownCounterForSpecialWithDuration,
        CooldownTimeForSpecialWithDuration,
    }

    public abstract class WeaponModel
    {
        #region Properties

        public abstract WeaponType WeaponType { get; }
        public abstract WeaponSpecialActivateType WeaponSpecialActivateType { get; }
        public float AttackSpeedPercent { get; protected set; }
        public float AttackRange { get; protected set; }
        public float DamageBonus { get; protected set; }
        public DamageFactor[] DamageFactors { get; protected set; }
        public StatusEffectModel[] DamageModifierModels { get; protected set; }
        public bool IsSpecialAttackReady { get; set; }

        #endregion Properties

        #region Class Methods

        public WeaponModel(WeaponData weaponData)
        {
            AttackSpeedPercent = weaponData.weaponConfigItem.AttackSpeedPercent;
            AttackRange = weaponData.weaponConfigItem.AttackRange;
            DamageBonus = weaponData.weaponConfigItem.DamageBonus;
            DamageFactors = weaponData.weaponConfigItem.DamageFactors;
            DamageModifierModels = weaponData.GetModifierModels(nameof(weaponData.weaponConfigItem.DamageModifierIdentities));
        }

        #endregion Class Methods
    }

    public abstract class CooldownTimeSpecialWeaponModel : WeaponModel
    {
        #region Properties

        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.CooldownTime;
        public float CurrentSpecialAttackCooldownTime { get; set; }
        public float SpecialAttackCooldownTime { get; set; }

        #endregion Properties

        public CooldownTimeSpecialWeaponModel(WeaponData weaponData)
            : base(weaponData)
        {
            IsSpecialAttackReady = true;
            CurrentSpecialAttackCooldownTime = 0.0f;
        }
    }

    public abstract class CountdownCounterSpecialWeaponModel : WeaponModel
    {
        #region Properties

        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.CountdownCounter;
        public int CurrentCoundownCounterCount { get; set; }
        public int CountdownCounterCount { get; set; }

        #endregion Properties

        public CountdownCounterSpecialWeaponModel(WeaponData weaponData)
            : base(weaponData) { }
    }

    public abstract class CountdownCounterSpecialWithDurationWeaponModel : CountdownCounterSpecialWeaponModel
    {
        #region Properties

        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.CountdownCounterForSpecialWithDuration;
        public bool HasTriggeredSpecialAttack { get; set; }
        public float CurrentSpecialAttackDuration { get; set; }
        public float SpecialAttackDuration { get; set; }

        #endregion Properties

        public CountdownCounterSpecialWithDurationWeaponModel(WeaponData weaponData)
            : base(weaponData)
        => HasTriggeredSpecialAttack = false;
    }

    public abstract class CountdownTimeSpecialWithDurationWeaponModel : CooldownTimeSpecialWeaponModel
    {
        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.CooldownTimeForSpecialWithDuration;
        public bool HasTriggeredSpecialAttack { get; set; }
        public float CurrentSpecialAttackDuration { get; set; }
        public float SpecialAttackDuration { get; set; }

        protected CountdownTimeSpecialWithDurationWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            HasTriggeredSpecialAttack = false;
        }
    }

    public abstract class ConditionalTriggerSpecialWeaponModel : WeaponModel
    {
        #region Properties

        public override WeaponSpecialActivateType WeaponSpecialActivateType => WeaponSpecialActivateType.ConditionalTrigger;

        #endregion Properties

        public ConditionalTriggerSpecialWeaponModel(WeaponData weaponData)
            : base(weaponData)
            => IsSpecialAttackReady = false;
    }

    public class WeaponData
    {
        #region Members

        public IWeaponDataConfigItem weaponConfigItem;
        public EquipmentMechanicDataConfigItem mechanicDataConfigItem;
        public Dictionary<string, StatusEffectModel[]> modifierModelsDictionary;

        #endregion Members

        #region Class Methods

        public WeaponData(IWeaponDataConfigItem weaponConfigItem, EquipmentMechanicDataConfigItem mechanicDataConfigItem, Dictionary<string, StatusEffectModel[]> modifierModelsDictionary)
        {
            this.weaponConfigItem = weaponConfigItem;
            this.modifierModelsDictionary = modifierModelsDictionary;
            this.mechanicDataConfigItem = mechanicDataConfigItem;
        }

        public StatusEffectModel[] GetModifierModels(string key)
        {
            if (modifierModelsDictionary != null)
            {
                if (modifierModelsDictionary.ContainsKey(key))
                    return modifierModelsDictionary[key];
                else
                    return null;
            }
            else return null;
        }

        #endregion Class Methods
    }
}