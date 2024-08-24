using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class LazerWeaponModel : CooldownTimeSpecialWeaponModel
    {
        #region Properties

        public override WeaponType WeaponType => WeaponType.Lazer;
        public float SmallLazerAffectWidth { get; private set; }
        public float BigLazerAffectWidth { get; private set; }
        public float BigLazerDamageBonus { get; private set; }
        public float BigLazerRange { get; private set; }
        public DamageFactor[] BigLazerDamageFactors { get; private set; }
        public int AddBonusDamageForEnemiesCount { get; private set; }
        public float HitEnemiesAddedDamagePercent { get; private set; }
        public bool CanAddMoreDamageAfterHitSomeEnemies { get; private set; }
        public bool CanLockEneniesInPlace { get; private set; }
        public StatusEffectModel[] LockEnemiesModifierModels { get; private set; }
        public StatusEffectModel[] BigLazerModifierModels { get; private set; }

        #endregion Properties

        #region Class Methods

        public LazerWeaponModel(WeaponData weaponData) : base(weaponData)
        {
            LazerWeaponDataConfigItem dataConfigItem =  weaponData.weaponConfigItem as LazerWeaponDataConfigItem;
            LazerEquipmentMechanicDataConfigItem mechanicDataConfigItem = weaponData.mechanicDataConfigItem as LazerEquipmentMechanicDataConfigItem;

            SpecialAttackCooldownTime = dataConfigItem.specialCooldown;
            SmallLazerAffectWidth = dataConfigItem.smallLazerAffectWidth;
            BigLazerRange = dataConfigItem.bigLazerRange;
            BigLazerAffectWidth = dataConfigItem.bigLazerAffectWidth;
            BigLazerDamageBonus = dataConfigItem.bigLazerDamageBonus;
            BigLazerDamageFactors = dataConfigItem.bigLazerDamageFactors;
            BigLazerModifierModels = weaponData.GetModifierModels(nameof(dataConfigItem.bigLazerModifierIdentities));

            AddBonusDamageForEnemiesCount = mechanicDataConfigItem.addBonusDamageForEnemiesCount;
            HitEnemiesAddedDamagePercent = mechanicDataConfigItem.hitEnemiesAddedDamagePercent;
            CanAddMoreDamageAfterHitSomeEnemies = mechanicDataConfigItem.addBonusDamageForEnemiesCount > 0 && mechanicDataConfigItem.hitEnemiesAddedDamagePercent > 0;

            LockEnemiesModifierModels = weaponData.GetModifierModels(nameof(mechanicDataConfigItem.lockEnemiesModifierIdentities));
            CanLockEneniesInPlace = LockEnemiesModifierModels != null;

            if (CanLockEneniesInPlace)
            {
                foreach (var lockEnemyModifierModel in LockEnemiesModifierModels)
                    lockEnemyModifierModel.AddDuration(mechanicDataConfigItem.lockEnemiesStackAddedDuration);
            }
        }

        #endregion Class Methods
    }
}