using System.Linq;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicBallWeaponModel : ConditionalTriggerSpecialWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.MagicBall;
        public float NormalFlyRange { get; private set; }
        public float NormalFlySpeed { get; private set; }
        public int NumberOfProjectiles { get; private set; }
        public float ProjectileCenterOffsetRadius { get; private set; }
        public float ProjectileFlySpeed { get; private set; }
        public float ProjectileFlyRange { get; private set; }
        public float ProjectileIdleRotateSpeed { get; private set; }
        public float ProjectileFireRotateSpeed { get; private set; }
        public float ProjectileFlyDamageBonus { get; private set; }
        public DamageFactor[] ProjectileFlyDamageFactors { get; private set; }
        public StatusEffectModel[] ProjectileFlyDamageModifierModels { get; private set; }
        public bool CanAddBonusDamageForProjectileAfterFiredSome { get; private set; }
        public float BonusDamagePercentAfterFireProjectiles { get; private set; }
        public int BonusDamageAfterFireProjectilesCount { get; private set; }
        public int ConsecutiveBonusDamageAfterFireProjectilesCount { get; private set; }
        public bool AllowProjectileIdleToDamage { get; private set; }
        public float ProjectileIdleDamageBonus { get; private set; }
        public DamageFactor[] ProjectileIdleDamageFactors { get; private set; }
        public StatusEffectModel[] ProjectileIdleDamageModifierModels { get; private set; }

        #endregion Properties

        #region Class Methods

        public MagicBallWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            MagicBallWeaponDataConfigItem dataConfigItem =  weaponData.weaponConfigItem as MagicBallWeaponDataConfigItem;
            MagicBallEquipmentMechanicDataConfigItem mechanicDataConfigItem = weaponData.mechanicDataConfigItem as MagicBallEquipmentMechanicDataConfigItem;

            NormalFlyRange = dataConfigItem.normalFlyRange;
            NormalFlySpeed = dataConfigItem.normalFlySpeed;
            NumberOfProjectiles = dataConfigItem.numberOfProjectiles;
            ProjectileCenterOffsetRadius = dataConfigItem.projectileCenterOffsetRadius;
            ProjectileFlySpeed = dataConfigItem.projectileFlySpeed;
            ProjectileFlyRange = dataConfigItem.projectileFlyRange;
            ProjectileIdleRotateSpeed = dataConfigItem.projectileIdleRotateSpeed;
            ProjectileFireRotateSpeed = dataConfigItem.projectileFireRotateSpeed;
            ProjectileFlyDamageBonus = dataConfigItem.projectileFlyDamageBonus;
            ProjectileFlyDamageFactors = dataConfigItem.projectileFlyDamageFactors;
            ProjectileFlyDamageModifierModels = weaponData.GetModifierModels(nameof(dataConfigItem.projectileFlyDamageModifierIdentities));

            BonusDamagePercentAfterFireProjectiles = mechanicDataConfigItem.bonusDamagePercentAfterFireProjectiles;
            BonusDamageAfterFireProjectilesCount = mechanicDataConfigItem.bonusDamageAfterFireProjectilesCount;
            CanAddBonusDamageForProjectileAfterFiredSome = mechanicDataConfigItem.bonusDamagePercentAfterFireProjectiles > 0 && mechanicDataConfigItem.bonusDamageAfterFireProjectilesCount > 0;

            ConsecutiveBonusDamageAfterFireProjectilesCount = mechanicDataConfigItem.consecutiveBonusDamageAfterFireProjectilesCount;

            ProjectileIdleDamageBonus = mechanicDataConfigItem.projectileIdleDamageBonus;
            ProjectileIdleDamageFactors = mechanicDataConfigItem.projectileIdleDamageFactors;
            if (mechanicDataConfigItem.projectileIdleDamageModifierIdentities != null)
                ProjectileIdleDamageModifierModels = weaponData.GetModifierModels(nameof(mechanicDataConfigItem.projectileIdleDamageModifierIdentities));

            AllowProjectileIdleToDamage = mechanicDataConfigItem.projectileIdleDamageBonus > 0 ||
                                        (mechanicDataConfigItem.projectileIdleDamageFactors != null && mechanicDataConfigItem.projectileIdleDamageFactors.Any(x => x.damageFactorValue > 0)) || 
                                        ProjectileIdleDamageModifierModels != null;

            NumberOfProjectiles += mechanicDataConfigItem.bonusNumberOfProjectiles;
        }

        #endregion Class Methods
    }
}