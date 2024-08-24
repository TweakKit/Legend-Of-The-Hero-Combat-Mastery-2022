using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct GachaEquipmentUnlockDefinition
    {
        #region Struct Methods

        public bool IsUnlock()
            => DataManager.Server.HasCompletedTutorial(TutorialType.UnlockGacha);

        public FeatureUnlockData GetUnlockData()
        {
            var isUnlocked = IsUnlock();
            var unlockedRequimentDescription = "You have to unlock the gacha feature first!";
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}