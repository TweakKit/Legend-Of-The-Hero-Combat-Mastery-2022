using UnityEngine;
using Runtime.Localization;
using TMPro;

namespace Runtime.Tutorial
{
    public class TutorialBlockTimeDelayIndicator : TutorialBlockTimeIndicator<TutorialBlockTimeDelayIndicatorData>
    {
        #region Members

        public GameObject blockerImage;
        public GameObject tipTextContainer;
        public TextMeshProUGUI tipText;
        private int _timerID = -1;

        #endregion Members

        #region Class Methods

        public override void Stop()
        {
            if (_timerID != -1)
                TutorialNavigator.CurrentTutorial.TutorialTimer.RemoveTimer(_timerID);
            base.Stop();
        }

        protected override void PostInit()
        {
            base.PostInit();

            _timerID = -1;
            if (OwnerBlockIndicatorData != null)
                _timerID = TutorialNavigator.CurrentTutorial.TutorialTimer.AddTimer(OwnerBlockIndicatorData.delayTime, 1, StopTutorial);

            blockerImage.SetActive(OwnerBlockIndicatorData.blockScreen);

            if (OwnerBlockIndicatorData.showTip)
            {
                tipTextContainer.SetActive(true);
                tipText.text = LocalizationManager.GetLocalize(LocalizeTable.TUTORIAL, OwnerBlockIndicatorData.tipDescriptionId);
            }
            else tipTextContainer.SetActive(false);
        }

        private void StopTutorial()
            => TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);

        #endregion Class Methods
    }
}