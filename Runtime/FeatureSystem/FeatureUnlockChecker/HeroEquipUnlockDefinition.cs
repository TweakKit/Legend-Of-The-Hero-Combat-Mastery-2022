using Runtime.Localization;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct HeroEquipUnlockDefinition
    {
        #region Members

        public const int REQUIRED_HERO_LEVEL = 1;
        public const int REQUIRED_NUMBER_OF_EQUIPMENTS = 2;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var heroLevel = DataManager.Server.HeroLevel;
            var numberOfEquipments = DataManager.Server.EquipmentsData.Count;
            return heroLevel >= REQUIRED_HERO_LEVEL && numberOfEquipments >= REQUIRED_NUMBER_OF_EQUIPMENTS;
        }

        public FeatureUnlockData GetUnlockData()
        {
            var isUnlocked = IsUnlock();
            var localizeFormat = LocalizationManager.GetLocalize(LocalizeTable.BASE_MAP, LocalizeKeys.UNLOCKED_HERO_CONDITION);
            var numberOfEquipments = DataManager.Server.EquipmentsData.Count;
            var unlockedRequimentDescription = string.Format(localizeFormat, numberOfEquipments);
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}