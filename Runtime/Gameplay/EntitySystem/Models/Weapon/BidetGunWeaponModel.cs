using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class BidetGunWeaponModel : CountdownCounterSpecialWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.BidetGun;
        public int NumberOfProjectiles { get; private set; }
        public float ProjectileCenterAngleOffset { get; private set; }
        public float ProjectileSpeed { get; private set; }
        public int AttackCountTriggerSpecial { get; private set; }
        public int BonusNumberOfProjectiles { get; private set; }
        public float SpecialDamageBonus { get; private set; }
        public DamageFactor[] SpecialDamageFactors { get; private set; }
        public StatusEffectModel[] SpecialDamageModifierModels { get; private set; }

        #endregion Properties

        #region Class Methods

        public BidetGunWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            BidetGunWeaponDataConfigItem dataConfigItem =  weaponData.weaponConfigItem as BidetGunWeaponDataConfigItem;
            NumberOfProjectiles = dataConfigItem.numberOfProjectiles;
            ProjectileCenterAngleOffset = dataConfigItem.shootCenterAngleOffset;
            ProjectileSpeed = dataConfigItem.projectileSpeed;
            AttackCountTriggerSpecial = dataConfigItem.attackCountToTriggerSpecial;
            BonusNumberOfProjectiles = dataConfigItem.bonusProjectileNumber;
            SpecialDamageBonus = dataConfigItem.specialDamageBonus;
            SpecialDamageFactors = dataConfigItem.specialDamageFactors;
            SpecialDamageModifierModels = weaponData.GetModifierModels(nameof(dataConfigItem.specialDamageModifierIdentities));

            CountdownCounterCount = AttackCountTriggerSpecial;
            CurrentCoundownCounterCount = 0;
            IsSpecialAttackReady = CurrentCoundownCounterCount == 0;
        }

        #endregion Class Methods
    }
}