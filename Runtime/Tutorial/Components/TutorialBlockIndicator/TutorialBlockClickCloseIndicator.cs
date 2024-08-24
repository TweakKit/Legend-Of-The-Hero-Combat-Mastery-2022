using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Tutorial
{
    public class TutorialBlockClickCloseIndicator : TutorialBlockTargetIndicator<TutorialBlockClickCloseIndicatorData>
    {
        #region Members

        [SerializeField]
        private Button _closeButton;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            _closeButton.onClick.AddListener(CloseIndicator);
        }

        private void CloseIndicator()
            => TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex, OwnerBlockData.switchData.switchType == SwitchType.Automatic);

        #endregion Class Methods
    }
}