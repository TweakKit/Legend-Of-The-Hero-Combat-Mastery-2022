using Runtime.Definition;
using Runtime.Localization;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct StructureUnlockDefinition
    {
        #region Members

        public const int UNLOCK_HEADQUARTER_STRUCTURE_REQUIRED_STAGE_ID = 101002;
        public const int UNLOCK_GACHA_STRUCTURE_REQUIRED_STAGE_ID = 101003;
        public const int UNLOCK_STORAGE_STRUCTURE_REQUIRED_STAGE_ID = 101001;
        public const int UNLOCK_JUICY_FACTORY_STRUCTURE_REQUIRED_STAGE_ID = 101008;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock(StructureType structureType)
        {
            switch (structureType)
            {
                case StructureType.Headquarter:
                    return true;

                case StructureType.Gacha:
                    return DataManager.Server.CheckStagePlayed(UNLOCK_GACHA_STRUCTURE_REQUIRED_STAGE_ID);

                case StructureType.Storage:
                    return true;

                case StructureType.JuiceFactory:
                    return DataManager.Server.CheckStagePlayed(UNLOCK_JUICY_FACTORY_STRUCTURE_REQUIRED_STAGE_ID);

                case StructureType.Order:
                    return DataManager.Server.HasCompletedTutorial(TutorialType.UseOrderStructure);
            }
            return false;
        }

        public FeatureUnlockData GetUnlockData(StructureType structureType)
        {
            var isUnlocked = IsUnlock(structureType);
            var unlockStageId = -1;
            switch (structureType)
            {
                case StructureType.Headquarter:
                    unlockStageId = UNLOCK_HEADQUARTER_STRUCTURE_REQUIRED_STAGE_ID;
                    break;

                case StructureType.Gacha:
                    unlockStageId = UNLOCK_GACHA_STRUCTURE_REQUIRED_STAGE_ID;
                    break;

                case StructureType.Storage:
                    unlockStageId = UNLOCK_STORAGE_STRUCTURE_REQUIRED_STAGE_ID;
                    break;

                case StructureType.JuiceFactory:
                    unlockStageId = UNLOCK_JUICY_FACTORY_STRUCTURE_REQUIRED_STAGE_ID;
                    break;

                case StructureType.Order:
                    unlockStageId = UNLOCK_JUICY_FACTORY_STRUCTURE_REQUIRED_STAGE_ID;
                    break;
            }

            var localizeStageFormat = LocalizationManager.GetLocalize(LocalizeTable.GENERAL, LocalizeKeys.UNLOCK_STAGE_CONDITION);
            var stageName = LocalizeKeys.GetStageName(unlockStageId);
            var unlockedRequimentDescription = string.Format(localizeStageFormat, stageName);
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}