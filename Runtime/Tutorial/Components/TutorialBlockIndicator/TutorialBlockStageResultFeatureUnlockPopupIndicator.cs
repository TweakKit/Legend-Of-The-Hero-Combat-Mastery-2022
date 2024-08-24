using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Localization;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using Runtime.Core.Mics;

namespace Runtime.Tutorial
{
    public class TutorialBlockStageResultFeatureUnlockPopupIndicator : TutorialBlockTargetIndicator<TutorialBlockStageResultFeatureUnlockPopupIndicatorData>
    {
        #region Members

        [SerializeField]
        private Button _closeIndicatorButton;
        [SerializeField]
        private Button _closePopupButton;
        [SerializeField]
        private Button _homeButton;
        [SerializeField]
        private Image _featureImage;
        [SerializeField]
        private TextMeshProUGUI _titleText;
        [SerializeField]
        private TextMeshProUGUI _unlockedFeatureDescriptionText;
        [SerializeField]
        private GameObject _titleContainer;
        [SerializeField]
        private GameObject _iconContainer;
        [SerializeField]
        private GameObject _descriptionContainer;
        [SerializeField]
        private GameObject _lock;
        [SerializeField]
        private GameObject _handImage;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            _lock.SetActive(true);
            _titleContainer.SetActive(false);
            _descriptionContainer.SetActive(false);
            _featureImage.sprite = OwnerBlockIndicatorData.featureSprite;
            _handImage.gameObject.SetActive(false);
            _closeIndicatorButton.gameObject.SetActive(false);
            _closePopupButton.gameObject.SetActive(true);
            _closePopupButton.onClick.AddListener(FinishShowingFeature);
            LoadDataAsync().Forget();
            StopAllCoroutines();
        }

        public override void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            base.InitStuff(tutorialBlockTargetsData);
            if (tutorialBlockTargetsData.Count > 0)
            {
                var target = tutorialBlockTargetsData[0].runtimeTarget;
                _homeButton.onClick.AddListener(() => {
                    FinishShowingFeature();
                    target.GetComponentInChildren<IClickable>().Click();
                });
            }
        }

        private async UniTask LoadDataAsync()
        {
            _titleText.text = await LocalizationManager.GetLocalizeAsync(LocalizeTable.BASE_MAP, LocalizeKeys.GetFeatureName(OwnerBlockIndicatorData.featureType));
            _unlockedFeatureDescriptionText.text = await LocalizationManager.GetLocalizeAsync(LocalizeTable.BASE_MAP, LocalizeKeys.GetUnlockedFeatureDescription(OwnerBlockIndicatorData.featureType));
        }

        private IEnumerator RunDisplay()
        {
            yield return new WaitForSecondsRealtime(OwnerBlockIndicatorData.showHomeButtonDelay);
            _handImage.gameObject.SetActive(true);
            _closeIndicatorButton.gameObject.SetActive(true);
            _closeIndicatorButton.onClick.AddListener(FinishShowingFeature);
        }

        private void FinishShowingFeature()
            => TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);

        #endregion Class Method

        #region Unity Event Callback Methods

        public void FinishedLockShaking()
        {
            _lock.SetActive(false);
            _titleContainer.SetActive(true);
            _descriptionContainer.SetActive(true);
            StartCoroutine(RunDisplay());
        }

        #endregion Unity Event Callback Methods
    }
}