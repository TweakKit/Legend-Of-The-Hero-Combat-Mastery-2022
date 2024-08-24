using System.Threading;
using Runtime.Definition;
using Runtime.Message;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Tutorial
{
    public class UnlockGachaTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _gachaButton;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.UnlockGacha;

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
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.GachaButton)
                _gachaButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForGachaButtonShowedUpAsync(cancellationToken);
            await StartTutorialOnGachaButtonAsync(cancellationToken);
        }

        private async UniTask WaitForGachaButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _gachaButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnGachaButtonAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenGachaButton, _gachaButton);
            await UniTask.CompletedTask;
        }

        #endregion Class Methods
    }
}