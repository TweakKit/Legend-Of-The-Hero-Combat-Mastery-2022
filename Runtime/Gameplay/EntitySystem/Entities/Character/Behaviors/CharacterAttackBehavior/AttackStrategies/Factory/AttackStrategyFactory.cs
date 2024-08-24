using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public static class AttackStrategyFactory
    {
        #region Class Methods

        public static IAttackStrategy GetAttackStrategy(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case WeaponType.MagicBall:
                    return new MagicBallAttackStrategy();

                case WeaponType.Lazer:
                    return new LazerAttackStrategy();

                case WeaponType.MagicMace:
                    return new MagicMaceAttackStrategy();

                case WeaponType.SingleHitGun:
                    return new SingleHitGunAttackStrategy();

                case WeaponType.Shovel:
                    return new ShovelAttackStrategy();

                default:
                    return new BidetGunAttackStrategy();
            }
        }

        #endregion Class Methods
    }
}