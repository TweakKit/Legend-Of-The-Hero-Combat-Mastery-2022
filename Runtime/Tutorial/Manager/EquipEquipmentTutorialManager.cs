using System.Threading;
using UnityEngine;
using Runtime.UI;
using Runtime.Definition;
using Runtime.Message;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    public class EquipEquipmentTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _equipButton;
        private TutorialRuntimeTarget _inventoryEquipmentItemsContainer;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.EquipEquipment;

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
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.EquipButton)
                _equipButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.InventoryEquipmentItemsContainer)
                _inventoryEquipmentItemsContainer = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForEquipmentItemsContainerShowedUpAsync(cancellationToken);
            await StartTutorialOnFirstEquipmentItemAsync(cancellationToken);
            await WaitForEquipButtonShowedUpAsync(cancellationToken);
            await StartTutorialOnEquipButtonAsync(cancellationToken);
        }

        private async UniTask WaitForEquipmentItemsContainerShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _inventoryEquipmentItemsContainer != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnFirstEquipmentItemAsync(CancellationToken cancellationToken)
        {
            var itemsContainer = _inventoryEquipmentItemsContainer.gameObject.GetComponent<InventoryEquipmentItemsContainer>();
            await UniTask.WaitUntil(() => itemsContainer.NumberOfCells > 0, cancellationToken: cancellationToken);
            var inventoryEquipmentItemElementsRow = itemsContainer.GetCellView(0);
            var inventoryItemElement = GetTutorialInventoryEquipmentItemElement(inventoryEquipmentItemElementsRow.transform);
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenFirstEquipmentItem,
                                        new TutorialRuntimeTarget(inventoryItemElement, TutorialTargetBoundType.RectTransform));
        }

        private async UniTask WaitForEquipButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _equipButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnEquipButtonAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenEquipButton, _equipButton);
            await UniTask.CompletedTask;
        }

        private GameObject GetTutorialInventoryEquipmentItemElement(Transform inventoryEquipmentItemElementsRowTransform)
        {
            var inventoryItemElement = inventoryEquipmentItemElementsRowTransform.GetChild(0).gameObject;
            return inventoryItemElement;
        }

        #endregion Class Methods
    }
}