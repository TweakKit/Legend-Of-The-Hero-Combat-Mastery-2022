using System.Threading;
using Runtime.Definition;
using Runtime.Message;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Tutorial
{
    public class NotifyUnlockJuiceFactoryTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _backMenuButton;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.NotifyUnlockJuiceFactory;

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

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForBackMenuButtonShowedUpAsync(cancellationToken);
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.NotifyUnlockJuiceHouse, _backMenuButton);
        }

        private async UniTask WaitForBackMenuButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _backMenuButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.BackMenuButton)
                _backMenuButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        #endregion Class Methods
    }
}