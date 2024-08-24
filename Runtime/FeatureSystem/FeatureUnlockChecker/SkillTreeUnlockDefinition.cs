using Runtime.Localization;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct SkillTreeUnlockDefinition
    {
        #region Members

        private const int STAGE_REQUIRED = 101008;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var hasPlayedStage = DataManager.Server.CheckStagePlayed(STAGE_REQUIRED);
            return hasPlayedStage;
        }

        public FeatureUnlockData GetUnlockData()
        {
            var isUnlocked = IsUnlock();
            var localizeFormat = LocalizationManager.GetLocalize(LocalizeTable.HERO_BUTTON, LocalizeKeys.UNLOCK_SKILL_TREE_FEATURE_CONDITION);
            var stageName = LocalizeKeys.GetStageName(STAGE_REQUIRED);
            var unlockedRequimentDescription = string.Format(localizeFormat, stageName);
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}