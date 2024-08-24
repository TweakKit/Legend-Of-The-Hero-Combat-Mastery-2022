using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Runtime.Core.Singleton;
using Cysharp.Threading.Tasks;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;
using System.Threading;

namespace Runtime.SceneLoading
{
    [Serializable]
    public struct SceneLoaderData
    {
        #region Members

        public List<SceneLoadingInfo> scenesLoadingInfo;

        #endregion Members
    }

    public class SceneManager : PersistentMonoSingleton<SceneManager>
    {
        #region Members

        private static bool s_isLoadingScene;
        [SerializeField]
        private SceneLoaderData _sceneLoaderData;
        [SerializeField]
        private float _sceneLoadingProgressSmoothSpeed = 3.0f;
        [SerializeField]
        private float _mainLoadScale = 1.0f;
        [SerializeField]
        private float _progressBarRoundValue = 0;
        [SerializeField]
        private SceneLoaderScreen _sceneLoaderScreen;
        private bool _hasFinishedLoading;
        private float _sceneLoadingProgressSmoothValue;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        private float DeltaTime => Time.unscaledDeltaTime;

        /// <summary>
        /// The scene loading info of the scene to be opened.
        /// </summary>
        public SceneLoadingInfo SceneLoadingInfo { get; private set; }

        /// <summary>
        /// The action that will be executed when there is a trigger that will change scene.
        /// </summary>
        private static Action TriggerChangeSceneAction { get; set; }

        /// <summary>
        /// The action that will be executed before the scene loading process takes place.
        /// </summary>
        private static Action BeforeChangeSceneAction { get; set; }

        /// <summary>
        /// The task that needs to be completed after the old scene has been unloaded, the new scene has been loaded,
        /// but before the new scene is about to appear (before the fade-in takes place).
        /// </summary>
        private static Func<CancellationToken, UniTask> CompleteTaskBeforeNewSceneAppearedTask { get; set; }

        /// <summary>
        /// The action that will be executed after the new scene has been loaded, and is about to appear (fade in).
        /// </summary>
        private static Action BeforeNewSceneAppearedAction { get; set; }


        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            s_isLoadingScene = false;
            _hasFinishedLoading = false;
            _sceneLoadingProgressSmoothValue = 0.0f;
        }

        private void OnEnable()
            => UnitySceneManager.activeSceneChanged += OnActiveSceneChanged;

        private void OnDisable()
            => UnitySceneManager.activeSceneChanged -= OnActiveSceneChanged;

        #endregion API Methods

        #region Class Methods

        public static async UniTask LoadSceneAsync(string sceneName)
        {
            if (s_isLoadingScene)
                return;
            s_isLoadingScene = true;
            await Instance.StartInternalLoadSceneAsync(sceneName);
            s_isLoadingScene = false;
        }

        public static void RegisterTriggerChangeScene(Action triggerChangeSceneAction)
            => TriggerChangeSceneAction += triggerChangeSceneAction;

        public static void RegisterBeforeChangeScene(Action beforeChangeSceneAction)
            => BeforeChangeSceneAction += beforeChangeSceneAction;

        public static void RegisterBeforeNewSceneAppeared(Action beforeNewSceneAppearedAction)
            => BeforeNewSceneAppearedAction += beforeNewSceneAppearedAction;

        public static void RegisterCompleteTaskBeforeNewSceneAppeared(Func<CancellationToken, UniTask> completeTaskBeforeNewSceneAppearedTask)
            => CompleteTaskBeforeNewSceneAppearedTask += completeTaskBeforeNewSceneAppearedTask;

        private async UniTask StartInternalLoadSceneAsync(string sceneName)
        {
            if (TriggerChangeSceneAction != null)
            {
                TriggerChangeSceneAction.Invoke();
                TriggerChangeSceneAction = null;
            }
            var sceneOpenedLoadingInfo = GetSceneLoadingInfo(sceneName);
            SceneLoadingInfo = sceneOpenedLoadingInfo;
            _sceneLoaderScreen.UpdateSceneLoadingInfo(SceneLoadingInfo);
            await StartSceneLoadingAsyncOperation();
        }

        private IEnumerator StartSceneLoadingAsyncOperation()
        {
            yield return _sceneLoaderScreen.PrepareLoadingScene();

            if (BeforeChangeSceneAction != null)
            {
                BeforeChangeSceneAction.Invoke();
                BeforeChangeSceneAction = null;
            }

            yield return null;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var sceneLoadingAsyncOperation = Addressables.LoadSceneAsync(SceneLoadingInfo.LoadedSceneName);
            var currentLoadingTime = 0.0f;
            while (!sceneLoadingAsyncOperation.IsDone)
            {
                var status = sceneLoadingAsyncOperation.GetDownloadStatus();
                float completeProgress = Mathf.Clamp01(status.Percent);
                _sceneLoadingProgressSmoothValue = Mathf.Lerp(_sceneLoadingProgressSmoothValue, completeProgress, DeltaTime * _sceneLoadingProgressSmoothSpeed);
                float progressBarValue = _progressBarRoundValue > 0 ? (Mathf.Round(_sceneLoadingProgressSmoothValue / _progressBarRoundValue) * _progressBarRoundValue)
                                                                    : _sceneLoadingProgressSmoothValue;
                progressBarValue = progressBarValue * _mainLoadScale;
                _sceneLoaderScreen.UpdateLoadProgress(progressBarValue);
                currentLoadingTime += DeltaTime;
                if (completeProgress >= 1)
                {
                    if (currentLoadingTime > SceneLoadingInfo.sceneLoadingMinTime)
                    {
                        if (!_hasFinishedLoading)
                        {
                            _sceneLoaderScreen.UpdateLoadProgress(1);
                            _hasFinishedLoading = true;
                        }
                    }
                    else yield return null;
                }

                yield return null;
            }
            _sceneLoaderScreen.UpdateLoadProgress(1);
        }

        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
            => ResetWhenSceneChanged();

        private void ResetWhenSceneChanged()
        {
            _hasFinishedLoading = false;
            _sceneLoadingProgressSmoothValue = 0.0f;
            WaitToCompleteTaskBeforeSceneAppear().Forget();
        }

        private async UniTask WaitToCompleteTaskBeforeSceneAppear()
        {
            if (CompleteTaskBeforeNewSceneAppearedTask != null)
            {
                await CompleteTaskBeforeNewSceneAppearedTask.Invoke(_cancellationTokenSource.Token);
                CompleteTaskBeforeNewSceneAppearedTask = null;
            }
            else await UniTask.CompletedTask;

            await UniTask.Yield();

            if (BeforeNewSceneAppearedAction != null)
            {
                BeforeNewSceneAppearedAction.Invoke();
                BeforeNewSceneAppearedAction = null;
            }

            await UniTask.Yield();

            _sceneLoaderScreen.ResetWhenSceneChanged();
        }

        private SceneLoadingInfo GetSceneLoadingInfo(string sceneName)
        {
            foreach (SceneLoadingInfo sceneLoadingInfo in _sceneLoaderData.scenesLoadingInfo)
            {
                if (sceneName.StartsWith(sceneLoadingInfo.sceneStartNameFormat))
                {
                    sceneLoadingInfo.UpdateLoadedSceneName(sceneName);
                    return sceneLoadingInfo;
                }
            }
            return null;
        }

        #endregion Class Methods
    }
}