using Runtime.Definition;

namespace Runtime.FeatureSystem
{
    public static class FeatureUnlockChecker
    {
        #region Class Methods

        public static bool IsRatingPanelUnlocked()
            => new RatingPanelUnlockDefinition().IsUnlock();

        public static bool IsStarterPackUnlocked()
            => new StarterPackUnlockDefinition().IsUnlock();

        public static bool IsPiggyBankUnlocked()
            => new PiggyBankUnlockDefinition().IsUnlock();

        public static bool IsSurveyUnlocked()
            => new SurveyUnlockDefinition().IsUnlock();

        public static bool IsGachaTokenUnlocked()
            => new GachaTokenUnlockDefinition().IsUnlock();

        public static bool IsUnlockGachaUnlocked()
            => new GachaUnlockDefinition().IsUnlock();

        public static FeatureUnlockData GetUnlockGachaUnlockedData()
            => new GachaUnlockDefinition().GetUnlockData();

        public static bool IsGachaEquipmentUnlocked()
            => new GachaEquipmentUnlockDefinition().IsUnlock();

        public static bool IsHeroEquipUnlocked()
            => new HeroEquipUnlockDefinition().IsUnlock();

        public static FeatureUnlockData GetHeroEquipUnlockedData()
            => new HeroEquipUnlockDefinition().GetUnlockData();

        public static bool IsEquipEquipmentUnlocked()
            => new EquipEquipmentUnlockDefinition().IsUnlock();

        public static bool IsSkillTreeUnlocked()
            => new SkillTreeUnlockDefinition().IsUnlock();

        public static FeatureUnlockData GetSkillTreeUnlockedData()
            => new SkillTreeUnlockDefinition().GetUnlockData();

        public static bool IsLockForeverStarterPackUnlocked()
            => new LockForeverStarterPackUnlockDefinition().IsUnlock();

        public static bool IsStructureUnlocked(StructureType structureType)
            => new StructureUnlockDefinition().IsUnlock(structureType);

        public static FeatureUnlockData GetStructureUnlockedData(StructureType structureType)
            => new StructureUnlockDefinition().GetUnlockData(structureType);

        public static bool IsLockMainQuestUnlocked()
            => new LockMainQuestDefinition().IsUnLock();

        #endregion Class Methods
    }

    public struct FeatureUnlockData
    {
        #region Members

        public bool isUnlocked;
        public string unlockedRequimentDescription;

        #endregion Members

        #region Struct Methods

        public FeatureUnlockData(bool isUnlocked, string unlockedRequimentDescription)
        {
            this.isUnlocked = isUnlocked;
            this.unlockedRequimentDescription = unlockedRequimentDescription;
        }

        #endregion Struct Methods
    }
}