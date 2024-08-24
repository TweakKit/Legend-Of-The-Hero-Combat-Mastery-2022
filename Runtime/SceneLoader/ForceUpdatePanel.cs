using Runtime.Definition;
using Runtime.Message;
using UnityEngine;
using UnityEngine.UI;
using Core.Foundation.PubSub;

namespace Runtime.UI
{
    public class ForceUpdatePanel : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private Button _forceUpdateButton;
        [SerializeField]
        private GameObject _normalUpdatedContent;
        [SerializeField]
        private GameObject _forceUpdatedContent;
        [SerializeField]
        private Button _forceUpdatedButton;
        [SerializeField]
        private Button _updatedButton;
        [SerializeField]
        private Button _cancelButton;

        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
            _normalUpdatedContent.SetActive(false);
            _forceUpdatedContent.SetActive(false);
            _forceUpdatedButton.onClick.AddListener(OnGoToStore);
            _updatedButton.onClick.AddListener(OnGoToStore);
            _cancelButton.onClick.AddListener(OnCancel);
        }

        private void OnDestroy()
        {
            _gameStateChangedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        private void OnCancel()
        {
            _normalUpdatedContent.SetActive(false);
        }

        private void OnGoToStore()
        {
            Application.OpenURL(Constants.ANDROID_STORE_URL);
        }

        private void OnGameStateChanged(GameStateChangedMessage message)
        {
            if (message.GameStateType == GameStateEventType.ForceVersionUpdated)
            {
                _forceUpdatedContent.SetActive(true);
            }
            else if (message.GameStateType == GameStateEventType.VersionUpdated)
            {
                _normalUpdatedContent.SetActive(true);
            }
        }

        #endregion Class Methods
    }
}