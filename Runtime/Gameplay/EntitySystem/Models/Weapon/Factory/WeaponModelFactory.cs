using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public static class WeaponModelFactory
    {
        #region Class Methods

        public static WeaponModel GetWeaponModel(WeaponType weaponType, WeaponData weaponData)
        {
            switch (weaponType)
            {
                case WeaponType.BidetGun:
                    return new BidetGunWeaponModel(weaponData);

                case WeaponType.MagicBall:
                    return new MagicBallWeaponModel(weaponData);

                case WeaponType.Lazer:
                    return new LazerWeaponModel(weaponData);

                case WeaponType.MagicMace:
                    return new MagicMaceWeaponModel(weaponData);

                case WeaponType.SingleHitGun:
                    return new SingleHitGunWeaponModel(weaponData);

                case WeaponType.Shovel:
                    return new ShovelWeaponModel(weaponData);
            }

            return null;
        }

        #endregion Class Methods
    }
}