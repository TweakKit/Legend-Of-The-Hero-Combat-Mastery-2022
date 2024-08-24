using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicMaceWeaponModel : CooldownTimeSpecialWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.MagicMace;

        public float StackedDamageStatusEffectConvertValue { get; private set; }
        public bool CanCauseMoreDamageForEffectedEnemies { get; private set; }
        public float BonusDamagePercentForEffectedEnemies { get; private set; }
        public bool CanAddMoreFreezingTimeForEnemies { get; private set; }
        public int MaxStackedDamageStatusEffectsCount { get; private set; }
        public float MaxStackedDamageStatusEffectBonusDuration { get; private set; }
        public bool CanCauseScaleDamageForLimitedEnemies { get; private set; }
        public float ScaleDamageWhenAttackLimitedEnemy { get; private set; }
        public int LimitedEnemyTriggerScaleDamageCount { get; private set; }
        public StatusEffectModel[] ConvertStatusEffectModels { get; private set; }

        #endregion Properties

        #region Class Methods

        public MagicMaceWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            MagicMaceWeaponDataConfigItem dataConfigItem =  weaponData.weaponConfigItem as MagicMaceWeaponDataConfigItem;
            MagicMaceEquipmentMechanicDataConfigItem mechanicDataConfigItem = weaponData.mechanicDataConfigItem as MagicMaceEquipmentMechanicDataConfigItem;

            ConvertStatusEffectModels = weaponData.GetModifierModels(nameof(dataConfigItem.triggeredStatusEffect));
            SpecialAttackCooldownTime = dataConfigItem.specialCooldown;

            MaxStackedDamageStatusEffectsCount = dataConfigItem.maxStackedStatusEffected;

            CanCauseMoreDamageForEffectedEnemies = mechanicDataConfigItem.bonusDamagePercentForEffectedEnemies > 0;
            BonusDamagePercentForEffectedEnemies = mechanicDataConfigItem.bonusDamagePercentForEffectedEnemies;

            CanAddMoreFreezingTimeForEnemies = mechanicDataConfigItem.maxStackedDamageStatusEffectBonusDuration > 0;
            MaxStackedDamageStatusEffectBonusDuration = mechanicDataConfigItem.maxStackedDamageStatusEffectBonusDuration;

            CanCauseScaleDamageForLimitedEnemies = mechanicDataConfigItem.scaleDamageWhenAttackLimitedEnemy > 0 && mechanicDataConfigItem.limitedEnemyTriggerScaleDamageCount > 0;
            ScaleDamageWhenAttackLimitedEnemy = mechanicDataConfigItem.scaleDamageWhenAttackLimitedEnemy;
            LimitedEnemyTriggerScaleDamageCount = mechanicDataConfigItem.limitedEnemyTriggerScaleDamageCount;

            foreach (var statusModel in DamageModifierModels)
                statusModel.SetMaxStack(MaxStackedDamageStatusEffectsCount);
        }

        #endregion Class Methods
    }
}