using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Modals;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Tutorial
{
    public class TutorialContainer : ModalContainer
    {
        #region Members

        public Button skipCutsceneButton;

        #endregion Members

        #region Properties

        public TutorialType PlayingTutorialType { get; private set; }

        #endregion Properties

        #region API Methods

        protected override void Start()
        {
            base.Start();
            skipCutsceneButton.gameObject.SetActive(false);
            skipCutsceneButton.transform.SetAsLastSibling();
            PlayingTutorialType = TutorialType.None;
        }

        #endregion API Methods

        #region Class Methods

        public void Init(TutorialType tutorialType, bool canSkipTutorialSequence)
        {
            PlayingTutorialType = tutorialType;
            canSkipTutorialSequence = false;
            skipCutsceneButton.gameObject.SetActive(canSkipTutorialSequence);
            if (canSkipTutorialSequence)
            {
                skipCutsceneButton.onClick.RemoveAllListeners();
                skipCutsceneButton.onClick.AddListener(OnClickSkipCutscene);
            }
        }

        public void ResetState()
        {
            skipCutsceneButton.gameObject.SetActive(false);
            PlayingTutorialType = TutorialType.None;
        }

        private void OnClickSkipCutscene()
        {
            skipCutsceneButton.gameObject.SetActive(false);
            TutorialNavigator.CurrentTutorial.SkipSequence();
        }

        #endregion Class Methods
    }
}