using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct SurveyUnlockDefinition
    {
        #region Members

        private const int REQUIRED_STAGE_ID = 101004;

        #endregion Members

        #region Struct Methods

        public bool IsUnlock()
        {
            var isRequiredStageCompleted = DataManager.Server.CheckStageCompleted(REQUIRED_STAGE_ID);
            var checkSubmittedSurvey = DataManager.Local.CheckSubmittedSurvey(Constants.CURRENT_SURVEY);
            return isRequiredStageCompleted && !checkSubmittedSurvey;
        }

        #endregion Struct Methods
    }
}