using System.Linq;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SingleHitGunWeaponModel : CountdownTimeSpecialWithDurationWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.SingleHitGun;
        public float ProjectileSpeed { get; private set; }
        public float SpecialAttackDecreaseMovementSpeedPercent { get; private set; }
        public float SpecialAttackIncreaseAttackSpeedPercent { get; private set; }
        public bool CanCatchTargetsInSpecialAttackRangeToAffect { get; private set; }
        public StatusEffectModel[] SpecialAttackAffectedModifierModels { get; private set; }
        public float SpecialAttackBonusAttackRangePercent { get; private set; }
        public bool CanExtendAttackRangeWhenInSpecialAttackState => SpecialAttackBonusAttackRangePercent > 0;

        #endregion Properties

        #region Class Methods

        public SingleHitGunWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            SingleHitGunWeaponDataConfigItem dataConfigItem =  weaponData.weaponConfigItem as SingleHitGunWeaponDataConfigItem;
            SingleHitGunEquipmentMechanicDataConfigItem mechanicDataConfigItem = weaponData.mechanicDataConfigItem as SingleHitGunEquipmentMechanicDataConfigItem;

            SpecialAttackCooldownTime = dataConfigItem.specialCooldown;

            ProjectileSpeed = dataConfigItem.projectileSpeed;
            SpecialAttackDecreaseMovementSpeedPercent = dataConfigItem.specialAttackDecreaseMovementSpeedPercent;
            SpecialAttackIncreaseAttackSpeedPercent = dataConfigItem.specialAttackIncreaseAttackSpeedPercent;

            SpecialAttackAffectedModifierModels = weaponData.GetModifierModels(nameof(mechanicDataConfigItem.specialAttackAffectedModifierIdentities));
            CanCatchTargetsInSpecialAttackRangeToAffect = SpecialAttackAffectedModifierModels != null;

            SpecialAttackBonusAttackRangePercent = mechanicDataConfigItem.specialAttackBonusAttackRangePercent;
            SpecialAttackDuration = dataConfigItem.specialAttackDuration + mechanicDataConfigItem.specialAttackBonusInStateDuration;

            CurrentSpecialAttackDuration = SpecialAttackDuration;
        }

        #endregion Class Methods
    }
}