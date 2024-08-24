using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct EquipEquipmentUnlockDefinition
    {
        #region Struct Methods

        public bool IsUnlock()
            => DataManager.Server.HasCompletedTutorial(TutorialType.UnlockEquipment);

        public FeatureUnlockData GetUnlockData()
        {
            var isUnlocked = IsUnlock();
            var unlockedRequimentDescription = "You have to unlock the hero equipment feature first!";
            return new FeatureUnlockData(isUnlocked, unlockedRequimentDescription);
        }

        #endregion Struct Methods
    }
}