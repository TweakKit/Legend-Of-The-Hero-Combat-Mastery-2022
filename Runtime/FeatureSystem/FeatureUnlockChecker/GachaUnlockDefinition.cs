using Runtime.Definition;
using Runtime.Localization;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct GachaUnlockDefinition
    {
        #region Members

        private const int STAGE_REQUIRED = 101003;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var hasCompletedReceiveGachaTokenTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.ReceiveGachaToken);
            return hasCompletedReceiveGachaTokenTutorial;
        }

        public FeatureUnlockData GetUnlockData()
        {
            var isUnlocked = IsUnlock();
            var localizeFormat = LocalizationManager.GetLocalize(LocalizeTable.BASE_MAP, LocalizeKeys.UNLOCKED_GACHA_CONDITION);
            var localizeStageFormat = LocalizationManager.GetLocalize(LocalizeTable.GENERAL, LocalizeKeys.UNLOCK_STAGE_CONDITION);
            var stageName = LocalizeKeys.GetStageName(STAGE_REQUIRED);

            var unlockedRequimentDescription = string.Format(localizeFormat, stageName);
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}