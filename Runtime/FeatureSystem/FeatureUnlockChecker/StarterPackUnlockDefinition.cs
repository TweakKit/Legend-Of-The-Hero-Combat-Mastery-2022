using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct StarterPackUnlockDefinition
    {
        #region Members

        private const int REQUIRED_STAGE_ID = 101006;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var starterPackProductData = DataManager.Config.GetProductData(DataManager.RemoteConfig.GetStarterPackProductType());
            var isStarterPackAvailable = DataManager.Server.IsIAPPackAvailable(starterPackProductData.productId, starterPackProductData.limit);
            return DataManager.Server.CheckStagePlayed(REQUIRED_STAGE_ID) && isStarterPackAvailable;
        }

        #endregion Struct Methods
    }
}