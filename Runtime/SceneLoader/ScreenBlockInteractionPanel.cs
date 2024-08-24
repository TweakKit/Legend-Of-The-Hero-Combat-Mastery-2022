using UnityEngine;
using UnityEngine.UI;
using Runtime.Message;
using Runtime.Manager;
using Core.Foundation.PubSub;
using Runtime.Localization;

namespace Runtime.SceneLoading
{
    public class ScreenBlockInteractionPanel : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private GameObject _blockInteractionPanel;
        [SerializeField]
        private Button _clickButton;
        [SerializeField]
        private float _timeOffsetToDisplayToast;

        private float _timeEnable;
        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _clickButton.onClick.AddListener(OnClickButton);
            _blockInteractionPanel.SetActive(false);
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        private void OnDestroy()
            => _gameStateChangedRegistry.Dispose();

        #endregion API Methods

        #region Class Methods

        private void OnClickButton()
        {
            if(Time.time - _timeEnable > _timeOffsetToDisplayToast)
            {
                ToastController.Instance.Show(LocalizationManager.GetLocalize(LocalizeTable.POPUP, LocalizeKeys.YOUR_REQUEST_IS_PROCESSING));
            }
        }

        private void OnGameStateChanged(GameStateChangedMessage message)
        {
            if (message.GameStateType == Definition.GameStateEventType.Reconnected)
                _blockInteractionPanel.SetActive(false);
            else if (message.GameStateType == Definition.GameStateEventType.StartProcessingFromServer)
            {
                _timeEnable = Time.time;
                _blockInteractionPanel.SetActive(true);
                
            }
            else if (message.GameStateType == Definition.GameStateEventType.EndProcessingFromServer)
                _blockInteractionPanel.SetActive(false);
        }

        #endregion Class Methods
    }
}