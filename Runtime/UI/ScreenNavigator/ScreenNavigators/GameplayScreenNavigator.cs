using UnityScreenNavigator.Runtime.Core.Shared.Views;
using Runtime.Definition;
using Runtime.Message;
using Runtime.Manager;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;
using System;

namespace Runtime.UI
{
    public class GameplayScreenNavigator : ScreenNavigator
    {
        #region Members

        private Registry<ReviveConfirmDisplayMessage> _reviveConfirmDisplayRegistry;
        private Registry<GameWinMessage> _gameWinRegistry;
        private Registry<GameLoseMessage> _gameLoseRegistry;
        private Registry<SurvivalEndMessage> _survivalEndRegistry;

        #endregion Members

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _reviveConfirmDisplayRegistry = Messenger.Subscriber().Subscribe<ReviveConfirmDisplayMessage>(OnReviveConfirmDisplay);
            _gameWinRegistry = Messenger.Subscriber().Subscribe<GameWinMessage>(OnGameWin);
            _gameLoseRegistry = Messenger.Subscriber().Subscribe<GameLoseMessage>(OnGameLose);
            _survivalEndRegistry = Messenger.Subscriber().Subscribe<SurvivalEndMessage>(OnSurvivalEnd);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _reviveConfirmDisplayRegistry.Dispose();
            _gameWinRegistry.Dispose();
            _gameLoseRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        protected override async UniTask HandleBackKeyOperationAsync()
        {
            if (!GameManager.Instance.IsGameOver)
            {
                if (IsOpeningATutorialModal)
                    await RunPopTopmostTutorialModalAsync(false);
                else if (IsOpeningAModal)
                    await RunPopTopmostModalAsync(false);
                else
                    Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.StopGameplayByBackKey));
            }
        }

        private void OnReviveConfirmDisplay(ReviveConfirmDisplayMessage reviveConfirmDisplayMessage)
        {
            var modalData = new ReviveConfirmModalData(reviveConfirmDisplayMessage.CanceledReviveCallbackAction,
                                                    reviveConfirmDisplayMessage.RevivedSuccessfullyCallbackAction);
            var windowOptions = new WindowOptions(ModalIds.REVIVE_CONFIRM, true);
            LoadSingleModal(windowOptions, modalData).Forget();
        }

        private void OnGameWin(GameWinMessage gameWinMessage)
        {
            var modalData = new VictoryModalData(gameWinMessage.Data);
            var option = new WindowOptions(ModalIds.VICTORY_MODAL, true);
            LoadSingleModal(option, modalData).Forget();
        }

        private void OnSurvivalEnd(SurvivalEndMessage message)
        {
            var modalData = new SurvivalResultModalData(false, message.WaveIndex, message.Data);
            var option = new WindowOptions(ModalIds.SURVIVAL_RESULT_MODAL, false);
            LoadSingleModal(option, modalData).Forget();
        }

        private void OnGameLose(GameLoseMessage gameLoseMessage)
        {
            var modalData = new DefeatModalData(gameLoseMessage.GameModeType, gameLoseMessage.Data);
            var option = new WindowOptions(ModalIds.DEFEAT_MODAL, false);
            LoadSingleModal(option, modalData).Forget();
        }

        #endregion Class Methods
    }
}