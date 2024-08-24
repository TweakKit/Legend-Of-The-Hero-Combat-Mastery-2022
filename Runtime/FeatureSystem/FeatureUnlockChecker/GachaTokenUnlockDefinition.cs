using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct GachaTokenUnlockDefinition
    {
        #region Members

        public const int REQUIRED_STAGE_ID = 101003;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var hasPlayedStage = DataManager.Server.CheckStagePlayed(REQUIRED_STAGE_ID);
            return hasPlayedStage;
        }

        #endregion Struct Methods
    }
}