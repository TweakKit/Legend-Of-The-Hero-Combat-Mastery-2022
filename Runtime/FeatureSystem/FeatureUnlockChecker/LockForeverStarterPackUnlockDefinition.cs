namespace Runtime.FeatureSystem
{
    public struct LockForeverStarterPackUnlockDefinition
    {
        #region Members

        public const int REQUIRED_HERO_LEVEL = 20;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var heroLevel = Manager.Data.DataManager.Server.HeroLevel;
            return heroLevel >= REQUIRED_HERO_LEVEL;
        }

        #endregion Struct Methods
    }
}