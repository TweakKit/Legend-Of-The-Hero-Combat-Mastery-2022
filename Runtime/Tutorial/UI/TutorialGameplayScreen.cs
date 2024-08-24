using System.Threading;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using Runtime.UI;
using Runtime.Extensions;
using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Runtime.Localization;
using Runtime.Definition;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;
using Runtime.Utilities;
using TMPro;

namespace Runtime.Tutorial.UI
{
    public class TutorialGameplayScreen : Screen<EmptyScreenData>
    {
        #region Members

        private const int START_NORMAL_WAVE_INDEX = 3;
        private static readonly float s_displayBloodEffectSpeed = 1.0f;
        private static readonly float s_maxBloodEffectAlphaValue = 0.6f;

        [SerializeField]
        private CanvasGroup _tutorialPanelCanvasGroup;
        [SerializeField]
        private CanvasGroup _blackPanelCanvasGroup;
        [SerializeField]
        private CanvasGroup _joystickObjectCanvasGroup;
        [SerializeField]
        private CanvasGroup _normalAttackObjectCanvasGroup;
        [SerializeField]
        private CanvasGroup _specialAttackObjectCanvasGroup;
        [SerializeField]
        private PlayAttackButtonElement _playAttackButtonElement;
        [SerializeField]
        private GameObject _waveInfoContainer;
        [SerializeField]
        private WarningWaveText _warningWaveText;
        [SerializeField]
        private TextMeshProUGUI _waveText;
        [SerializeField]
        private TextMeshProUGUI _waveCountTime;
        [SerializeField]
        private PlaySpecialAttackButtonElementWrapper _playSpecialAttackButtonElementWrapper;

        [Header("=== BLOOD EFFECT ===")]
        [SerializeField]
        private CanvasGroup _bloodEffectCanvasGroup;

        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;
        private Registry<HeroGotHitMessage> _heroGotHitRegistry;
        private Registry<WaveTimeUpdatedMessage> _waveTimeUpdatedRegistry;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region API Methods

        private void Update()
        {
            _playAttackButtonElement.UpdateStatus();
            _playSpecialAttackButtonElementWrapper.UpdateStatus();
        }

        #endregion API Methods

        #region Class Methods

        public override async UniTask Initialize(EmptyScreenData screenData)
        {
            await base.Initialize(screenData);
            _bloodEffectCanvasGroup.alpha = 0.0f;
            _warningWaveText.gameObject.SetActive(false);
            _waveInfoContainer.SetActive(false);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
            _heroGotHitRegistry = Messenger.Subscriber().Subscribe<HeroGotHitMessage>(OnHeroGotHit);
            _waveTimeUpdatedRegistry = Messenger.Subscriber().Subscribe<WaveTimeUpdatedMessage>(OnWaveTimeUpdated);
        }

        public override UniTask Cleanup()
        {
            _heroSpawnedRegistry.Dispose();
            _gameStateChangedRegistry.Dispose();
            _heroGotHitRegistry.Dispose();
            _waveTimeUpdatedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
            return base.Cleanup();
        }

        public void ShowTutorialPanel()
           => _tutorialPanelCanvasGroup.SetActive(true);

        public void HideTutorialPanel()
           => _tutorialPanelCanvasGroup.SetActive(false);

        public void ShowBlackPanel()
           => _blackPanelCanvasGroup.SetActive(true);

        public void HideBlackPanel()
           => _blackPanelCanvasGroup.SetActive(false);

        public void ShowJoystickObject()
            => _joystickObjectCanvasGroup.SetActive(true);

        public void HideJoystickObject()
            => _joystickObjectCanvasGroup.SetActive(false);

        public void EnableJoystickObject()
            => _joystickObjectCanvasGroup.SetActiveWithoutAlpha(true);

        public void DisableJoystickObject()
            => _joystickObjectCanvasGroup.SetActiveWithoutAlpha(false);

        public void ShowNormalAttackObject()
            => _normalAttackObjectCanvasGroup.SetActive(true);

        public void HideNormalAttackObject()
            => _normalAttackObjectCanvasGroup.SetActive(false);

        public void EnableNormalAttackObject()
            => _normalAttackObjectCanvasGroup.SetActiveWithoutAlpha(true);

        public void DisableNormalAttackObject()
            => _normalAttackObjectCanvasGroup.SetActiveWithoutAlpha(false);

        public void ShowSpecialAttackObject()
            => _specialAttackObjectCanvasGroup.SetActive(true);

        public void HideSpecialAttackObject()
            => _specialAttackObjectCanvasGroup.SetActive(false);

        public void EnableSpecialAttackObject()
            => _specialAttackObjectCanvasGroup.SetActiveWithoutAlpha(true);

        public void DisableSpecialAttackObject()
            => _specialAttackObjectCanvasGroup.SetActiveWithoutAlpha(false);

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
            => InitPlayActionButtonElements(heroSpawnedMessage.HeroModel.WeaponModel);

        private void OnGameStateChanged(GameStateChangedMessage gameStateChangedMessage)
        {
            if (gameStateChangedMessage.GameStateType == GameStateEventType.StopGameplayByBackKey)
                OpenQuitModalAsync().Forget();
        }

        private void OnHeroGotHit(HeroGotHitMessage heroGotHitMessage)
        {
            if (heroGotHitMessage.DamageReceived > 0)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                ShowBloodEffectAsync(_cancellationTokenSource.Token).Forget();
            }
        }

        private void OnWaveTimeUpdated(WaveTimeUpdatedMessage waveTimeUpdatedMessage)
        {
            var currentGameplayTimeInSecond = waveTimeUpdatedMessage.CurrentGameplayTime;
            _waveCountTime.text = TimeUtility.FormatTimeDuration(currentGameplayTimeInSecond, TimeFormatType.Mmss);

            if (waveTimeUpdatedMessage.IsNewWave)
            {
                if (waveTimeUpdatedMessage.CurrentWaveIndex >= START_NORMAL_WAVE_INDEX)
                {
                    var waveFormat = LocalizationManager.GetLocalize(LocalizeTable.CAMPAIGN, LocalizeKeys.WAVE_WITH_INDEX);
                    _waveInfoContainer.SetActive(true);
                    _waveText.text = string.Format(waveFormat, $"{waveTimeUpdatedMessage.CurrentWaveIndex - START_NORMAL_WAVE_INDEX + 1}" +
                                                               $"/{waveTimeUpdatedMessage.MaxWaveIndex - START_NORMAL_WAVE_INDEX + 1}");
                    _warningWaveText.Init(waveTimeUpdatedMessage.CurrentWaveIndex - START_NORMAL_WAVE_INDEX,
                                          waveTimeUpdatedMessage.CurrentWaveIndex == waveTimeUpdatedMessage.MaxWaveIndex);
                }
            }
        }

        private async UniTask ShowBloodEffectAsync(CancellationToken cancellationToken)
        {
            float interpolationTime = 0.0f;
            while (interpolationTime < 1.0f)
            {
                interpolationTime += Time.unscaledDeltaTime * s_displayBloodEffectSpeed;
                var parapolaInterpolationValue = 4.0f * (-interpolationTime * interpolationTime + interpolationTime);
                _bloodEffectCanvasGroup.alpha = parapolaInterpolationValue * s_maxBloodEffectAlphaValue;
                await UniTask.Yield(cancellationToken);
            }
            _bloodEffectCanvasGroup.alpha = 0.0f;
        }

        private async UniTask OpenQuitModalAsync()
        {
            var content = LocalizationManager.GetLocalize(LocalizeTable.POPUP, LocalizeKeys.QUIT_GAME_DESCRIPTION);
            var quitModalData = new QuitModalData(content: content);
            var quitGameModalOption = new WindowOptions(ModalIds.QUIT_GAME, false);
            await ScreenNavigator.Instance.LoadTutorialModal(quitGameModalOption, quitModalData);
        }

        private void InitPlayActionButtonElements(WeaponModel weaponModel)
        {
            _playAttackButtonElement.Init(weaponModel);
            _playSpecialAttackButtonElementWrapper.Init(weaponModel);
        }

        #endregion Class Methods
    }
}