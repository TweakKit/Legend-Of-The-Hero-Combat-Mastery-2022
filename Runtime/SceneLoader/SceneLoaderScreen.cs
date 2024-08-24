using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Gameplay.Tools.Easing;
using Runtime.Localization;
using TMPro;
using UnityRandom = UnityEngine.Random;

namespace Runtime.SceneLoading
{
    public class SceneLoaderScreen : MonoBehaviour
    {
        #region Members

        [Header("=== SETTINGS ===")]
        [SerializeField]
        private float _screenInitialFadeInSpeed = 1.0f;
        [SerializeField]
        private float _startLoadSceneDelay = 1.0f;
        [SerializeField]
        private bool _canChangeBackground;
        [SerializeField]
        private float _backgroundShowTime = 2.0f;
        [SerializeField]
        private float _changeBackgroundDelay = 0.5f;
        [SerializeField]
        private float _backgroundFadeSpeed = 2.0f;
        [SerializeField]
        private float _tipShowTime = 5.0f;
        [SerializeField]
        private float _changeTipDelay = 0.1f;
        [SerializeField]
        private float _tipFadeSpeed = 2.0f;
        [SerializeField]
        [TextArea(2, 2)]
        private string _progressTextFormat = "{0}%";

        [Header("=== UI ELEMENTS ===")]
        [SerializeField]
        private TextMeshProUGUI _progressText;
        [SerializeField]
        private TextMeshProUGUI _tipText;
        [SerializeField]
        private Image _backgroundImage;
        [SerializeField]
        private Image _progressBarSliderImage;
        [SerializeField]
        private CanvasGroup _backgroundCanvasGroup;
        [SerializeField]
        private CanvasGroup _fadeImageCanvas;
        [SerializeField]
        private CanvasGroup _rootCanvasGroup;

        private SceneLoadingInfo _sceneLoadingInfo;
        private List<string> _cacheSceneLoadingTips;
        private List<Sprite> _cacheBackgrounds;
        private bool _isTipFadeIn = false;
        private int _currentTipIndex = 0;
        private int _currentBackgroundIndex = 0;

        #endregion Members

        #region Properties

        private float DeltaTime => Time.unscaledDeltaTime;

        #endregion Properties

        #region API Methods

        private void Awake()
        {
            _progressBarSliderImage.type = Image.Type.Sliced;
            _progressBarSliderImage.transform.localScale = new Vector2(0, 1);
            _fadeImageCanvas.alpha = 1;
            _fadeImageCanvas.transform.SetParent(_rootCanvasGroup.transform.parent, false);
            _fadeImageCanvas.transform.SetAsLastSibling();
            _rootCanvasGroup.gameObject.SetActive(false);
        }

        private void OnDisable() => StopAllCoroutines();

        #endregion API Methods

        #region Class Methods

        public void UpdateLoadProgress(float value)
        {
            string percent = (value * 100).ToString("F0");
            _progressBarSliderImage.transform.localScale = new Vector2(value, 1);
            _progressText.text = string.Format(_progressTextFormat, percent);
        }

        public void ResetWhenSceneChanged()
        {
            StopAllCoroutines();
            var alpha = _tipText.color;
            alpha.a = 0.0f;
            _tipText.color = alpha;
            _isTipFadeIn = true;
            _backgroundCanvasGroup.alpha = 1;
            _fadeImageCanvas.alpha = 1;
            _rootCanvasGroup.gameObject.SetActive(false);
            _fadeImageCanvas.gameObject.SetActive(true);

            if (_sceneLoadingInfo != null)
            {
                StartCoroutine(FadeIn(_sceneLoadingInfo.fadeInDelay,
                                      _sceneLoadingInfo.fadeInSpeed,
                                      () => _fadeImageCanvas.gameObject.SetActive(false)));
            }
            else
            {
                StartCoroutine(FadeIn(0.0f,
                                      _screenInitialFadeInSpeed,
                                      () => _fadeImageCanvas.gameObject.SetActive(false)));
            }
        }

        public IEnumerator PrepareLoadingScene()
        {
            UpdateLoadProgress(0);
            float currentStartLoadSceneDelay = 0.0f;
            while (currentStartLoadSceneDelay < _startLoadSceneDelay)
            {
                currentStartLoadSceneDelay += DeltaTime;
                var value = Easing.EaseInOutCubic(0.0f, 1.0f, currentStartLoadSceneDelay / _startLoadSceneDelay);
                _rootCanvasGroup.alpha = value;
                yield return null;
            }
        }

        public void UpdateSceneLoadingInfo(SceneLoadingInfo sceneLoadingInfo)
        {
            _sceneLoadingInfo = sceneLoadingInfo;
            _cacheBackgrounds = sceneLoadingInfo.backgrounds;
            if (_canChangeBackground)
            {
                if (_cacheBackgrounds.Count > 1)
                    StartCoroutine(StartTransitionBackgrounds());
                else if (_cacheBackgrounds.Count > 0)
                    _backgroundImage.sprite = _cacheBackgrounds[0];
            }

            _cacheSceneLoadingTips = sceneLoadingInfo.sceneLoadingTips;
            if (_cacheSceneLoadingTips.Count > 0)
            {
                _currentTipIndex = UnityRandom.Range(0, _cacheSceneLoadingTips.Count);
                _tipText.text = LocalizationManager.GetLocalize(LocalizeTable.TAP_TO_START, _cacheSceneLoadingTips[_currentTipIndex]);
                StartCoroutine(StartLoopTips());
            }
            else _tipText.text = "";

            _progressText.text = string.Format(_progressTextFormat, 0);
            _rootCanvasGroup.alpha = 0;
            _rootCanvasGroup.gameObject.SetActive(true);
        }

        private IEnumerator StartTransitionBackgrounds()
        {
            while (true)
            {
                _backgroundImage.sprite = _cacheBackgrounds[_currentBackgroundIndex];
                while (_backgroundCanvasGroup.alpha < 1.0f)
                {
                    _backgroundCanvasGroup.alpha += DeltaTime * _backgroundFadeSpeed;
                    yield return new WaitForEndOfFrame();
                }
                yield return new WaitForSeconds(_backgroundShowTime);
                while (_backgroundCanvasGroup.alpha > 0.0f)
                {
                    _backgroundCanvasGroup.alpha -= DeltaTime * _backgroundFadeSpeed;
                    yield return new WaitForEndOfFrame();
                }
                _currentBackgroundIndex = (_currentBackgroundIndex + 1) % _cacheBackgrounds.Count;
                yield return new WaitForSeconds(_changeBackgroundDelay);
            }
        }

        private IEnumerator StartLoopTips()
        {
            Color alpha = _tipText.color;
            if (_isTipFadeIn)
            {
                while (alpha.a < 1.0f)
                {
                    alpha.a += DeltaTime * _tipFadeSpeed;
                    _tipText.color = alpha;
                    yield return null;
                }
                StartCoroutine(StartWaitForNextTip(_tipShowTime));
            }
            else
            {
                while (alpha.a > 0.0f)
                {
                    alpha.a -= DeltaTime * _tipFadeSpeed;
                    _tipText.color = alpha;
                    yield return null;
                }
                StartCoroutine(StartWaitForNextTip(_changeTipDelay));
            }

            if (_isTipFadeIn)
            {
                int previoustipIndex = _currentTipIndex;
                _currentTipIndex = UnityRandom.Range(0, _cacheSceneLoadingTips.Count);
                while (_currentTipIndex == previoustipIndex)
                {
                    _currentTipIndex = UnityRandom.Range(0, _cacheSceneLoadingTips.Count);
                    yield return null;
                }
                _tipText.text = LocalizationManager.GetLocalize(LocalizeTable.TAP_TO_START, _cacheSceneLoadingTips[_currentTipIndex]);
            }
        }

        private IEnumerator StartWaitForNextTip(float delayTime)
        {
            _isTipFadeIn = !_isTipFadeIn;
            yield return new WaitForSeconds(delayTime);
            StartCoroutine(StartLoopTips());
        }

        private IEnumerator FadeIn(float delay, float fadeInSpeed, Action finishAction = null)
        {
            yield return new WaitForSeconds(delay);
            if (fadeInSpeed > 0)
            {
                while (_fadeImageCanvas.alpha > 0.0f)
                {
                    _fadeImageCanvas.alpha -= DeltaTime * fadeInSpeed;
                    yield return null;
                }
            }
            finishAction?.Invoke();
        }

        private IEnumerator FadeOut(float delay, float fadeOutSpeed, Action finishAction = null)
        {
            yield return new WaitForSeconds(delay);
            if (fadeOutSpeed > 0)
            {
                while (_fadeImageCanvas.alpha < 1.0f)
                {
                    _fadeImageCanvas.alpha += DeltaTime * fadeOutSpeed;
                    yield return null;
                }
            }
            finishAction?.Invoke();
        }

        #endregion Class Methods
    }
}