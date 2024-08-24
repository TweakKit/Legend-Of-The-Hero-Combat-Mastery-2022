using System.Threading;
using UnityEngine;
using Runtime.UI;
using Runtime.Manager.Data;
using Runtime.Definition;
using Runtime.Message;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    public class UseOrderStructureTutorialManager : TutorialManager
    {
        #region Members

        [Tooltip("Reference to the home screen so that this tutorial can determine when to start the tutorial when in home screen.")]
        [SerializeField]
        private HomeScreen _homeScreen;
        private TutorialRuntimeTarget _juiceFactoryStructureObject;
        private TutorialRuntimeTarget _juiceFactoryOutput;
        private TutorialRuntimeTarget _orderStructureObject;
        private TutorialRuntimeTarget _orderStructureModal;
        private TutorialRuntimeTarget _sendOrderReturnButton;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.UseOrderStructure;

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
            _juiceFactoryOutput = null;
            _tutorialFlowCancellationTokenSource.Cancel();
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceFactoryStructureObject)
                _juiceFactoryStructureObject = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.JuiceFactoryOutput)
                _juiceFactoryOutput = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.OrderStructureObject)
                _orderStructureObject = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.OrderStructureModal)
                _orderStructureModal = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.SendOrderReturnButton)
                _sendOrderReturnButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForNeededFactoryOutputCompletedMakingAsync(cancellationToken);
            await DirectTutorial(cancellationToken);
        }

        private async UniTask DirectTutorial(CancellationToken cancellationToken)
        {
            var isOpeningJuiceFactory = ScreenNavigator.Instance.IsShowingModal(ModalIds.JUICE_FACTORY);
            if (isOpeningJuiceFactory)
                await DirectTutorialFromJuiceFactory(cancellationToken);
            else
                await DirectTutorialFromHomeScreen(cancellationToken);
        }

        private async UniTask DirectTutorialFromJuiceFactory(CancellationToken cancellationToken)
            => await FirstDirectTutorial(cancellationToken);

        private async UniTask DirectTutorialFromHomeScreen(CancellationToken cancellationToken)
        {
            await WaitForHomeScreenShownOnlyAsync(cancellationToken);
            await SecondDirectTutorial(cancellationToken);
        }

        private async UniTask FirstDirectTutorial(CancellationToken cancellationToken)
        {
            await WaitForOrderStructureObjectShowedUpAsync(cancellationToken);
            await SetUpTutorialOrderStructureObjectAsync(cancellationToken);

            await WaitForJuiceFactoryOutputShowedUpAsync(cancellationToken);
            await StartTutorialOnJuiceFactoryOutputAsync(cancellationToken);

            await WaitForOrderStructureModalShowedUpAsync(cancellationToken);
            await StartTutorialOnOrderStructureModalAsync(cancellationToken);

            await WaitForSendOrderReturnButtonShowedUpAsync(cancellationToken);
            await SetUpTutorialOnSendOrderReturnButtonAsync(cancellationToken);
        }

        private async UniTask SecondDirectTutorial(CancellationToken cancellationToken)
        {
            await WaitForJuiceFactoryObjectShowedUpAsync(cancellationToken);
            await StartTutorialOnJuiceFactoryObjectAsync(cancellationToken);

            await WaitForOrderStructureObjectShowedUpAsync(cancellationToken);
            await SetUpTutorialOrderStructureObjectAsync(cancellationToken);

            await WaitForJuiceFactoryOutputShowedUpAsync(cancellationToken);
            await StartTutorialOnJuiceFactoryOutputAsync(cancellationToken);

            await WaitForOrderStructureModalShowedUpAsync(cancellationToken);
            await StartTutorialOnOrderStructureModalAsync(cancellationToken);

            await WaitForSendOrderReturnButtonShowedUpAsync(cancellationToken);
            await SetUpTutorialOnSendOrderReturnButtonAsync(cancellationToken);
        }

        private async UniTask WaitForNeededFactoryOutputCompletedMakingAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var hasProducedItems = DataManager.Server.HasProducedItems(MoneyType.FactoryProductBlueberryJuice, StructureType.JuiceFactory);
                if (!hasProducedItems)
                    await UniTask.Yield(cancellationToken);
                else
                    break;
            }
        }

        private async UniTask WaitForHomeScreenShownOnlyAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                var isOnlyHomeScreenShown = _homeScreen.IsOnlyHomeScreenShown;
                if (!isOnlyHomeScreenShown)
                    await UniTask.Yield(cancellationToken);
                else
                    break;
            }
        }

        private async UniTask WaitForJuiceFactoryObjectShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _juiceFactoryStructureObject != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnJuiceFactoryObjectAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenJuiceFactoryObject, _juiceFactoryStructureObject);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForOrderStructureObjectShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _orderStructureObject != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialOrderStructureObjectAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.LightenOrderStructureObject, _orderStructureObject);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForJuiceFactoryOutputShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _juiceFactoryOutput != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnJuiceFactoryOutputAsync(CancellationToken cancellationToken)
        {
            ScreenNavigator.Instance.PopAllAboveModals(_juiceFactoryOutput.gameObject);
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenJuiceFactoryOutput, _juiceFactoryOutput);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForOrderStructureModalShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _orderStructureModal != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnOrderStructureModalAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.ShowOrderStructureInfo, _orderStructureModal);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForSendOrderReturnButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _sendOrderReturnButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialOnSendOrderReturnButtonAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.LightenSendOrderReturnButton, _sendOrderReturnButton);
            await UniTask.CompletedTask;
        }

        #endregion Class Methods
    }
}