using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Gameplay.Tools.Easing;
using Runtime.Localization;
using Runtime.FeatureSystem;
using Coffee.UIEffects;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using TMPro;

namespace Runtime.Tutorial
{
    public class TutorialBlockFeatureUnlockPopupIndicator : TutorialBlockTargetIndicator<TutorialBlockFeatureUnlockPopupIndicatorData>
    {
        #region Members

        [SerializeField]
        private Image _featureImage;
        [SerializeField]
        private TextMeshProUGUI _titleText;
        [SerializeField]
        private TextMeshProUGUI _unlockedFeatureDescription;
        [SerializeField]
        private UIEffect _disableEffect;
        [SerializeField]
        private UIParticle _particleEffect;
        [SerializeField]
        private UIParticle _showButtonParticleEffect;
        [SerializeField]
        private GameObject _titleContainer;
        [SerializeField]
        private GameObject _iconContainer;
        [SerializeField]
        private GameObject _descriptionContainer;
        [SerializeField]
        private GameObject _lock;
        [SerializeField]
        private RectTransform _flyObject;
        private Transform _flyToTransform;
        private bool _targetIsUI;
        private UnlockFeature _targetUnlockFeature;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            _lock.SetActive(true);
            _titleContainer.SetActive(false);
            _descriptionContainer.SetActive(false);
            _particleEffect.Stop();
            _showButtonParticleEffect.Stop();
            _disableEffect.enabled = true;
            _featureImage.sprite = OwnerBlockIndicatorData.featureSprite;
            _targetUnlockFeature?.UpdateStatus(false);
            LoadDataAsync().Forget();
            StopAllCoroutines();
        }

        public override void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            base.InitStuff(tutorialBlockTargetsData);
            if (tutorialBlockTargetsData.Count > 0)
            {
                _flyToTransform = tutorialBlockTargetsData[0].runtimeTarget.transform;
                _targetIsUI = tutorialBlockTargetsData[0].targetBoundType == TutorialTargetBoundType.RectTransform;
                _targetUnlockFeature = _flyToTransform.GetComponent<UnlockFeature>();
            }
        }

        private async UniTask LoadDataAsync()
        {
            _titleText.text = await LocalizationManager.GetLocalizeAsync(LocalizeTable.BASE_MAP, LocalizeKeys.GetFeatureName(OwnerBlockIndicatorData.featureType));
            _unlockedFeatureDescription.text = await LocalizationManager.GetLocalizeAsync(LocalizeTable.BASE_MAP, LocalizeKeys.GetUnlockedFeatureDescription(OwnerBlockIndicatorData.featureType));
        }

        private IEnumerator StartDisplayEffects()
        {
            yield return new WaitForSecondsRealtime(OwnerBlockIndicatorData.displayFeatureTime);

            _titleContainer.SetActive(false);
            _descriptionContainer.SetActive(false);

            var startLocalScale = _flyObject.transform.localScale;
            var endLocalScale = Vector3.one * 0.5f;
            var startMovePosition = _flyObject.transform.position;
            var endMovePosition = _flyToTransform.position;
            if (!_targetIsUI)
                endMovePosition = Camera.main.WorldToScreenPoint(_flyToTransform.position);
            endMovePosition += OwnerBlockIndicatorData.flyToOffset;

            float currentFlyDuration = 0.0f;
            while (currentFlyDuration <= OwnerBlockIndicatorData.flyDuration)
            {
                currentFlyDuration += Time.unscaledDeltaTime;
                var interpolatedValue = Easing.EaseInBack(0.0f, 1.0f, Mathf.Clamp01(currentFlyDuration / OwnerBlockIndicatorData.flyDuration));
                _flyObject.transform.localScale = Vector3.Lerp(startLocalScale, endLocalScale, interpolatedValue);
                _flyObject.transform.position = Vector3.Lerp(startMovePosition, endMovePosition, interpolatedValue);
                yield return null;
            }

            _iconContainer.SetActive(false);
            _showButtonParticleEffect.transform.position = _flyObject.position;
            _showButtonParticleEffect.Play();

            yield return new WaitForSecondsRealtime(OwnerBlockIndicatorData.stopShowingFeatureDelay);

            FinishShowingFeature();
        }

        private void FinishShowingFeature()
        {
            _targetUnlockFeature?.UpdateStatus(true);
            TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);
        }

        #endregion Class Method


        #region Unity Event Callback Methods

        public void FinishedLockShaking()
        {
            _disableEffect.enabled = false;
            _lock.SetActive(false);
            _particleEffect.Play();
            _titleContainer.SetActive(true);
            _descriptionContainer.SetActive(true);
            StartCoroutine(StartDisplayEffects());
        }

        #endregion Unity Event Callback Methods
    }
}