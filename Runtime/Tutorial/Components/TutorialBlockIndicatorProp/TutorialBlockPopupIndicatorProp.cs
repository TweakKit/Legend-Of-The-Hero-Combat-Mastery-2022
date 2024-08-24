using UnityEngine.UI;
using TMPro;
using Runtime.Localization;

namespace Runtime.Tutorial
{
    /// <summary>
    /// Used by popup indicator and info display indicator.
    /// </summary>
    public class TutorialBlockPopupIndicatorProp : TutorialBlockIndicatorProp
    {
        #region Members

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Button closeButton;
        public Button advanceButton;
        public Button backButton;
        private bool _closeButtonState;
        private bool _advanceButtonState;
        private bool _backButtonState;

        #endregion Members

        #region Class Methods

        public override void Init()
        {
            base.Init();
            closeButton?.onClick.RemoveAllListeners();
            advanceButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            if (closeButton != null)
                _closeButtonState = closeButton.gameObject.activeSelf;
            if (advanceButton != null)
                _advanceButtonState = advanceButton.gameObject.activeSelf;
            if (backButton != null)
                _backButtonState = backButton.gameObject.activeSelf;
        }

        public override void Destroy()
        {
            base.Destroy();
            closeButton?.onClick.RemoveAllListeners();
            advanceButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            if (closeButton != null)
                closeButton.gameObject.SetActive(_closeButtonState);
            if (advanceButton != null)
                advanceButton.gameObject.SetActive(_advanceButtonState);
            if (backButton != null)
                backButton.gameObject.SetActive(_backButtonState);
        }

        public void SetUpContent(bool useTemporaryInfo, string titleId, string descriptionId)
        {
            if (titleText != null)
            {
                if (!string.IsNullOrEmpty(titleId) && !useTemporaryInfo)
                    titleText.text = LocalizationManager.GetLocalize(LocalizeTable.TUTORIAL, titleId);
                else
                    titleText.text = titleId;
            }

            if (descriptionText != null)
            {
                if (!string.IsNullOrEmpty(descriptionId) && !useTemporaryInfo)
                    descriptionText.text = LocalizationManager.GetLocalize(LocalizeTable.TUTORIAL, descriptionId);
                else
                    descriptionText.text = descriptionId;
            }
        }

        #endregion Class Methods
    }
}