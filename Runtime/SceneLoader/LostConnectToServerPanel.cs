using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Message;
using Runtime.Definition;
using Runtime.Manager;
using Runtime.Extensions;
using Runtime.SceneLoading;
using Runtime.Localization;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;
using TMPro;

namespace Runtime.UI
{
    public class LostConnectToServerPanel : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private TextMeshProUGUI _descriptionText;
        [SerializeField]
        private TextMeshProUGUI _loadingText;
        [SerializeField]
        private Button _reconnectButton;
        [SerializeField]
        private float _showLoadingAnimationDuration = 5.0f;
        [SerializeField]
        private float _changeLoadingTextDelay = 0.5f;
        [SerializeField]
        private CanvasGroup _elementsContainerCanvasGroup;
        private Registry<ConnectToServerLostMessage> _connectToServerLostRegistry;
        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region Properties

        private ServerLostConnectionType CurrentServerLostConnectionType { get; set; }

        #endregion Properties

        #region API Methods

        private void Awake()
        {
            _elementsContainerCanvasGroup.SetActive(false);
            _loadingText.gameObject.SetActive(false);
            _connectToServerLostRegistry = Messenger.Subscriber().Subscribe<ConnectToServerLostMessage>(OnConnectToServerLost);
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        private void OnDestroy()
        {
            _connectToServerLostRegistry.Dispose();
            _gameStateChangedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        private void OnConnectToServerLost(ConnectToServerLostMessage connectToServerLostMessage)
        {
            GameManager.Instance.StopGameFlow(GameFlowTimeControllerType.LostConnectToServer);
            CurrentServerLostConnectionType = connectToServerLostMessage.ServerLostConnectionType;
            _elementsContainerCanvasGroup.SetActive(true);
            _descriptionText.text = GetLostConnectDescription();
            _loadingText.gameObject.SetActive(false);
            _reconnectButton.gameObject.SetActive(true);
            _reconnectButton.onClick.RemoveAllListeners();
            _reconnectButton.onClick.AddListener(() => {
                StopAllCoroutines();
                StartCoroutine(ShowLoadingAnimation());
                connectToServerLostMessage.ReconnectAction?.Invoke();
            });
        }

        private void OnGameStateChanged(GameStateChangedMessage message)
        {
            if (message.GameStateType == GameStateEventType.Reconnected ||
                message.GameStateType == GameStateEventType.HandShakeSucceed)
            {
                GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.LostConnectToServer);
                _elementsContainerCanvasGroup.SetActive(false);
                if (CurrentServerLostConnectionType == ServerLostConnectionType.LostConnectionOutOfGameplay)
                    SceneManager.LoadSceneAsync(SceneNames.START_SCREEN_SCENE_NAME).Forget();
            }
        }

        private string GetLostConnectDescription()
        {
            var returnedDescription = "";
            if (CurrentServerLostConnectionType == ServerLostConnectionType.LostConnectionInGameplay)
                returnedDescription = LocalizationManager.GetLocalize(LocalizeTable.POPUP, LocalizeKeys.REQUEST_FAILED_IN_GAMEPLAY_DESCRIPTION);
            else if (CurrentServerLostConnectionType == ServerLostConnectionType.LostConnectionOutOfGameplay)
                returnedDescription = LocalizationManager.GetLocalize(LocalizeTable.POPUP, LocalizeKeys.REQUEST_FAILED_OUT_OF_GAMEPLAY_DESCRIPTION);
            return returnedDescription;
        }

        private IEnumerator ShowLoadingAnimation()
        {
            _reconnectButton.gameObject.SetActive(false);
            _loadingText.gameObject.SetActive(true);
            _loadingText.text = ".";
            var loadingTextPointsCount = 0;
            float currentLoadingAnimationTime = 0.0f;
            float currentChangeLoadingTextDelay = 0.0f;
            while (currentLoadingAnimationTime < _showLoadingAnimationDuration)
            {
                currentLoadingAnimationTime += Time.unscaledDeltaTime;
                currentChangeLoadingTextDelay += Time.unscaledDeltaTime;
                if (currentChangeLoadingTextDelay >= _changeLoadingTextDelay)
                {
                    currentChangeLoadingTextDelay = 0.0f;
                    loadingTextPointsCount = (++loadingTextPointsCount) % 3;
                    _loadingText.text = loadingTextPointsCount == 0 ? "." : loadingTextPointsCount == 1 ? ".." : "...";
                }
                yield return null;
            }
            _reconnectButton.gameObject.SetActive(true);
            _loadingText.gameObject.SetActive(false);
        }

        #endregion Class Methods
    }
}