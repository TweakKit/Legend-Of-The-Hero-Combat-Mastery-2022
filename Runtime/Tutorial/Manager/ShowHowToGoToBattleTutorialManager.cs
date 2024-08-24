using System.Threading;
using Runtime.UI;
using Runtime.Manager.Data;
using Runtime.Definition;
using Runtime.Message;
using Runtime.ConfigModel;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    public class ShowHowToGoToBattleTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _stageItemsContainer;
        private TutorialRuntimeTarget _playStageButtonGameObject;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.ShowHowToGoToBattle;

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

        public override void ResetTutorial()
        {
            base.ResetTutorial();
            _tutorialFlowCancellationTokenSource.Cancel();
        }

        public override void StartTutorialRuntime()
        {
            base.StartTutorialRuntime();
            _tutorialFlowCancellationTokenSource = new CancellationTokenSource();
            StartTutorialAsync(_tutorialFlowCancellationTokenSource.Token).Forget();
        }

        private void OnTutorialTargetObjectAppeared(TutorialTargetObjectAppearedMessage tutorialTargetObjectAppearedMessage)
        {
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.StageItemsContainer)
                _stageItemsContainer = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.PlayStageButton)
                _playStageButtonGameObject = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForStageItemsContainerShowedUpAsync(cancellationToken);
            await StartTutorialOnStageItemAsync(cancellationToken);
            await WaitForPlayStageButtonShowedUpAsync(cancellationToken);
            await StartTutorialOnPlayStageButtonAsync(cancellationToken);
        }

        private async UniTask WaitForStageItemsContainerShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _stageItemsContainer != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnStageItemAsync(CancellationToken cancellationToken)
        {
            var itemsContainer = _stageItemsContainer.gameObject.GetComponent<StageItemsContainer>();
            await UniTask.WaitUntil(() => itemsContainer.NumberOfCells > 0, cancellationToken: cancellationToken);
            var fullStageId = await DataManager.Server.GetHighestUnlockStage(Constants.FIRST_WORLD_ID, StageModeType.Normal);
            var stageExtract = fullStageId.ExtractStageId();
            var stageItemIndex = stageExtract.stageId - 1;
            var stageItemsRow = itemsContainer.GetCellView(stageItemIndex);
            var firstStageItemGameObject = stageItemsRow == null ? itemsContainer.GetCellView(0).transform.GetChild(0).gameObject : stageItemsRow.transform.GetChild(0).gameObject;
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenStageItem,
                                        new TutorialRuntimeTarget(firstStageItemGameObject, TutorialTargetBoundType.RectTransform));
        }

        private async UniTask WaitForPlayStageButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _playStageButtonGameObject != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnPlayStageButtonAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenPlayStageButton, _playStageButtonGameObject);
            await UniTask.CompletedTask;
        }

        #endregion Class Methods
    }
}