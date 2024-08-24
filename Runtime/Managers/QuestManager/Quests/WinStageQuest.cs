using Runtime.Message;
using Runtime.Definition;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class WinStageQuest : Quest<WinStageQuestModel>
    {
        #region Members

        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        public override void Dispose()
        {
            base.Dispose();
            _gameStateChangedRegistry.Dispose();
        }

        private void OnGameStateChanged(GameStateChangedMessage gameStateChangedMessage)
        {
            if (!HasCompleted)
            {
                if (gameStateChangedMessage.GameStateType == GameStateEventType.WinStage)
                    Complete();
            }
        }

        #endregion Class Methods }
    }
}