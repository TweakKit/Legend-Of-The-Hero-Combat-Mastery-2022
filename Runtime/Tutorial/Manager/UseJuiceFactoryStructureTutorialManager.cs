using System.Threading;
using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Message;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;
using ScreenNavigatorModal = UnityScreenNavigator.Runtime.Core.Modals.Modal;

namespace Runtime.Tutorial
{
    public class UseJuiceFactoryStructureTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _juiceFactoryObject;
        private TutorialRuntimeTarget _juiceFactoryModal;
        private TutorialRuntimeTarget _juiceJugsContainer;
        private TutorialRuntimeTarget _fourthJuiceInputHolder;
        private TutorialRuntimeTarget _skipTimeButton;
        private TutorialRuntimeTarget _useSkipFiveMinuteButton;
        private bool _hasFollowedUpAndStartedOrderStructureTutorial;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private CancellationTokenSource _checkJuiceOutputCompletedMakingCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.UseJuiceFactoryStructure;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _hasFollowedUpAndStartedOrderStructureTutorial = false;
            _tutorialTargetObjectAppearedRegistry = Messenger.Subscriber().Subscribe<TutorialTargetObjectAppearedMessage>(OnTutorialTargetObjectAppeared);
        }

        private void OnDestroy()
        {
            if (_tutorialFlowCancellationTokenSource != null)
                _tutorialFlowCancellationTokenSource.Cancel();
            if (_checkJuiceOutputCompletedMakingCancellationTokenSource != null)
                _checkJuiceOutputCompletedMakingCancellationTokenSource.Cancel();
            _tutorialTargetObjectAppearedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public override void StartTutorialRuntime()
        {
            base.StartTutorialRuntime();
            _tutorialFlowCancellationTokenSource = new CancellationTokenSource();
            StartTutorialAsync(_tutorialFlowCancellationTokenSource.Token).Forget();
            _checkJuiceOutputCompletedMakingCancellationTokenSource = new CancellationTokenSource();
            StartCheckingJuiceOutputCompletedMakingAsync(_checkJuiceOutputCompletedMakingCancellationTokenSource.Token).Forget();
        }

        public override void ResetTutorial()
        {
            base.ResetTutorial();
            _tutorialFlowCancellationTokenSource.Cancel();
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceFactoryStructureObject)
                _juiceFactoryObject = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceFactoryModal)
                _juiceFactoryModal = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceJugsContainer)
                _juiceJugsContainer = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceFactoryFourthInputHolder)
                _fourthJuiceInputHolder = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.SkipTimeButton)
                _skipTimeButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.UseSkipFiveMinuteButton)
                _useSkipFiveMinuteButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartCheckingJuiceOutputCompletedMakingAsync(CancellationToken cancellationToken)
        {
            while (!_hasFollowedUpAndStartedOrderStructureTutorial)
            {
                var hasProducedItems = DataManager.Server.HasProducedItems(MoneyType.FactoryProductBlueberryJuice, StructureType.JuiceFactory);
                if (!hasProducedItems)
                    await UniTask.Yield(cancellationToken);
                else
                    break;
            }

            if (!_hasFollowedUpAndStartedOrderStructureTutorial)
            {
                StopTutorial();
                MarkTutorialCompleted();
                FollowUpAndStartTutorialRuntime();
            }
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForJuiceFactoryObjectShowedUpAsync(cancellationToken);
            await StartTutorialOnJuiceFactoryObjectAsync(cancellationToken);
            await WaitForJuiceFactoryModalShowedUpAsync(cancellationToken);
            await StartTutorialOnJuiceFactoryModalAsync(cancellationToken);
            await WaitForFourthJuiceInputHolderShowedUpAsync(cancellationToken);
            await SetUpTutorialOnFourthJuiceInputHolderAsync(cancellationToken);
            await WaitForJuiceJugShowedUpAsync(cancellationToken);
            await SetUpTutorialJuiceJugAsync(cancellationToken);
            await WaitForSkipTimeButtonShowedUpAsync(cancellationToken);
            await SetUpTutorialSkipTimeButtonAsync(cancellationToken);
            await WaitForUseSkipFiveMinuteTimeButtonShowedUpAsync(cancellationToken);
            await StartTutorialOnUseSkipFiveMinuteTimeButtonAsync(cancellationToken);
        }

        private async UniTask WaitForJuiceFactoryObjectShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _juiceFactoryObject != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnJuiceFactoryObjectAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenJuiceFactoryObject, _juiceFactoryObject);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForJuiceFactoryModalShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _juiceFactoryModal != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnJuiceFactoryModalAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.ShowJuiceFactoryInfo, _juiceFactoryModal);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForJuiceJugShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _juiceJugsContainer != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialJuiceJugAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.ShowHowToMakeJuiceJug, _juiceJugsContainer);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForFourthJuiceInputHolderShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _fourthJuiceInputHolder != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialOnFourthJuiceInputHolderAsync(CancellationToken cancellationToken)
        {
            _fourthJuiceInputHolder.gameObject.SetActive(false);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForSkipTimeButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _skipTimeButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialSkipTimeButtonAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.LightenSkipTimeButton, _skipTimeButton);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForUseSkipFiveMinuteTimeButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _useSkipFiveMinuteButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnUseSkipFiveMinuteTimeButtonAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenUseSkipFiveMinuteButton, _useSkipFiveMinuteButton);
            await UniTask.CompletedTask;
        }

        #endregion Class Methods

        #region Unity Event Callback Methods

        public void LockCloseJuiceFactoryModal()
        {
            var juiceFactoryModal = _juiceFactoryModal.gameObject.GetComponent<ScreenNavigatorModal>();
            if (juiceFactoryModal != null)
                juiceFactoryModal.CanCloseAsClickOnBackdrop = false;
        }

        public void UnlockCloseJuiceFactoryModal()
        {
            var juiceFactoryModal = _juiceFactoryModal.gameObject.GetComponent<ScreenNavigatorModal>();
            if (juiceFactoryModal != null)
                juiceFactoryModal.CanCloseAsClickOnBackdrop = true;
        }

        public void FollowUpAndStartTutorialRuntime()
        {
            if(!_hasFollowedUpAndStartedOrderStructureTutorial)
            {
                _hasFollowedUpAndStartedOrderStructureTutorial = true;
                TutorialNavigator.Instance.ExecuteUseOrderStructureTutorial();
            }
        }

        #endregion Unity Event Callback Methods
    }
}