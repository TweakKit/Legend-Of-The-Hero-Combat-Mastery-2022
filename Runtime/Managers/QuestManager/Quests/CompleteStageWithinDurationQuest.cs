using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class CompleteStageWithinDurationQuest : Quest<CompleteStageWithinDurationQuestModel>
    {
        #region Members

        private Registry<WaveTimeUpdatedMessage> _waveTimeUpdatedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _waveTimeUpdatedRegistry = Messenger.Subscriber().Subscribe<WaveTimeUpdatedMessage>(OnWaveTimeUpdated);
            HasCompleted = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _waveTimeUpdatedRegistry.Dispose();
        }

        private void OnWaveTimeUpdated(WaveTimeUpdatedMessage message)
        {
            if (message.CurrentGameplayTime > ownerModel.RequiredDuration)
                HasCompleted = false;
        }

        #endregion Class Methods
    }
}