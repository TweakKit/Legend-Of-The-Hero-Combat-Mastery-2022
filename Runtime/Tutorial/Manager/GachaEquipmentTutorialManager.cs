using System.Threading;
using Runtime.Definition;
using Runtime.Message;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Tutorial
{
    public class GachaEquipmentTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _gacha1PremiumButton;
        private TutorialRuntimeTarget _backButton;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.GachaEquipment;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _tutorialTargetObjectAppearedRegistry = Messenger.Subscriber().Subscribe<TutorialTargetObjectAppearedMessage>(OnTutorialTargetObjectAppeared);
        }

        private void OnDestroy()
        {
            if (_tutorialFlowCancellationTokenSource != null)
                _tutorialFlowCancellationTokenSource.Cancel();
            _tutorialTargetObjectAppearedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public override void StartTutorialRuntime()
        {
            base.StartTutorialRuntime();
            _tutorialFlowCancellationTokenSource = new CancellationTokenSource();
            StartTutorialAsync(_tutorialFlowCancellationTokenSource.Token).Forget();
        }

        public override void ResetTutorial()
        {
            base.ResetTutorial();
            _tutorialFlowCancellationTokenSource.Cancel();
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.BackButton)
            {
                _backButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            }
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.Gacha1PreminumButton)
            {
                _gacha1PremiumButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            }
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForBackButtonShowedUpAsync(cancellationToken);
            await SetUpTutorialOnBackButtonAsync(cancellationToken);
            await WaitForGacha1PremiumButtonShowedUpAsync(cancellationToken);
            await StartTutorialOnGacha1PremiumButtonAsync(cancellationToken);
        }

        private async UniTask WaitForBackButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _backButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialOnBackButtonAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.LightenGachaBackButton, _backButton);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForGacha1PremiumButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _gacha1PremiumButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnGacha1PremiumButtonAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenGacha1PremiumButton, _gacha1PremiumButton);
            await UniTask.CompletedTask;
        }

        #endregion Class Methods
    }
}