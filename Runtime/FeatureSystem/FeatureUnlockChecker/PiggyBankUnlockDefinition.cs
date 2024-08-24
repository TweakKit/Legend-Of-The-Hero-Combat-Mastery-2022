using Runtime.Externals.IAP;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct PiggyBankUnlockDefinition
    {
        public bool IsUnlock()
        {
            var firstPiggyBankPack = DataManager.Config.GetProductData(StoreProductType.Piggy_Bank_1);
            var isFirstPiggyBankAvailable = DataManager.Server.IsIAPPackAvailable(firstPiggyBankPack.productId, firstPiggyBankPack.limit);

            var secondPiggyBankPack = DataManager.Config.GetProductData(StoreProductType.Piggy_Bank_2);
            var isSecondPiggyBankAvailable = DataManager.Server.IsIAPPackAvailable(secondPiggyBankPack.productId, secondPiggyBankPack.limit);
            return isFirstPiggyBankAvailable || isSecondPiggyBankAvailable;
        }
    }
}