using UnityEngine;
using UnityEngine.UI;
using Runtime.Extensions;
using Runtime.Message;
using Runtime.Definition;
using Runtime.Manager;
using Runtime.Localization;
using Core.Foundation.PubSub;

namespace Runtime.UI
{
    public class NotifyServerMaintenancePanel : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private Button _headToDiscordButton;
        [SerializeField]
        private GameObject _warningPanel;
        [SerializeField]
        private TextTicker _warningTextTicker;
        [SerializeField]
        private CanvasGroup _maintenanceElementsCanvasGroup;
        private Registry<NotifyServerMaintenanceMessage> _notifyServerMaintenanceRegistry;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _headToDiscordButton.onClick.AddListener(OnClickHeadToDiscordButton);
            _warningPanel.SetActive(false);
            _warningTextTicker.enabled = false;
            _maintenanceElementsCanvasGroup.SetActive(false);
            _notifyServerMaintenanceRegistry = Messenger.Subscriber().Subscribe<NotifyServerMaintenanceMessage>(OnNotifyServerMaintenance);
        }

        private void OnDestroy()
        {
            _notifyServerMaintenanceRegistry.Dispose();
            GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.NotifyMaintenance);
        }

        #endregion API Methods

        #region Class Methods

        private void OnClickHeadToDiscordButton()
            => Application.OpenURL(SocialConstants.DISCORD_LINK);

        private void OnNotifyServerMaintenance(NotifyServerMaintenanceMessage message)
        {
            if (message.NotifyInGameplay)
            {
                var warningFormat = LocalizationManager.GetLocalize(LocalizeTable.GENERAL, LocalizeKeys.SERVER_MAINTENANCE_DESCRIPTION);
                _maintenanceElementsCanvasGroup.SetActive(false);
                _warningPanel.SetActive(true);
                _warningTextTicker.enabled = true;
                _warningTextTicker.SetContent(warningFormat, message.RemainTimeToMaintain);
            }
            else
            {
                GameManager.Instance.StopGameFlow(GameFlowTimeControllerType.NotifyMaintenance);
                _maintenanceElementsCanvasGroup.SetActive(true);
                _warningTextTicker.enabled = false;
                _warningPanel.SetActive(false);
            }
        }

        #endregion Class Methods
    }
}