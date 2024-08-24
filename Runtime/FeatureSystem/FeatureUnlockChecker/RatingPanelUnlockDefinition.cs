using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct RatingPanelUnlockDefinition
    {
        #region Members

        private const int UNLOCK_STAGE = 101009;

        #endregion Members

        #region Class Methods

        public bool IsUnlock()
            => DataManager.Local.RatingStar <= 0 && DataManager.Server.CheckStageCompleted(UNLOCK_STAGE);

        #endregion Class Methods
    }
}