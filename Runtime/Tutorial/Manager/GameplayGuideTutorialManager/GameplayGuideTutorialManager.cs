using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Message;
using Runtime.Tracking;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    public class GameplayGuideTutorialManager : TutorialManager
    {
        #region Members

        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.Gameplay;

        #endregion Properties

        #region API Methods

        private void OnDestroy()
            => _tutorialTargetObjectAppearedRegistry.Dispose();

        #endregion API Methods

        #region Class Methods

        public override void StartTutorialRuntime()
        {
            base.StartTutorialRuntime();
            _tutorialTargetObjectAppearedRegistry = Messenger.Subscriber().Subscribe<TutorialTargetObjectAppearedMessage>(OnTutorialTargetObjectAppeared);
            StartTutorial();
        }

        private void StartTutorial()
        {
            currentSequenceStep = DataManager.Server.GetTutorialCompletedStep(TutorialType);
            PlaySequence();
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.NextStageButton)
            {
                var nextStageButtonGameObject = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
                SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenNextStageButton, nextStageButtonGameObject);
            }
        }

        #endregion Class Methods

        #region Unity Event Callback Methods

        public override void SaveSequence()
        {
            string tutorialSequenceId = currentPlayingSequenceData.sequenceName;
            SendTutorialTracking(tutorialSequenceId, currentSequenceStep);
            DataManager.Local.SetPlayedGameplayTutorialSequence(tutorialSequenceId);

            var markTutCompletedAsSequenceSaved = currentPlayingSequenceData.markTutCompletedAsSequenceSaved;
            if (markTutCompletedAsSequenceSaved)
            {
                string endTutorialSequencesId = Constants.END_GAMEPLAY_TUTORIAL_SEQUENCE_ID;
                DataManager.Local.SetPlayedGameplayTutorialSequence(endTutorialSequencesId);
            }
        }

        public override void SaveNextSequence()
        {
            var nextSequence = GetSequence(currentSequenceStep);
            if (nextSequence != null)
            {
                string nextTutorialSequenceId = nextSequence.sequenceName;
                SendTutorialTracking(nextTutorialSequenceId, currentSequenceStep);
                DataManager.Local.SetPlayedGameplayTutorialSequence(nextTutorialSequenceId);
            }
        }

        public override void SaveEndTutorialSequences()
        {
            string endTutorialSequencesId = currentPlayingSequenceData.sequenceName;
            SendTutorialTracking(endTutorialSequencesId, currentSequenceStep);
            endTutorialSequencesId = Constants.END_GAMEPLAY_TUTORIAL_SEQUENCE_ID;
            DataManager.Local.SetPlayedGameplayTutorialSequence(endTutorialSequencesId);
#if TRACKING
            AppsFlyerManager.Instance.TrackCompleteTutorial(DataManager.Local.UserId);
#endif
            ResetTutorialContainer();
        }

        #endregion Unity Event Callback Methods
    }
}