using System.Threading;
using UnityEngine;
using Runtime.UI;
using Runtime.Definition;
using Runtime.Message;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    public class SkillTreeTutorialManager : TutorialManager
    {
        #region Members

        private TutorialRuntimeTarget _skillTreeTab;
        private TutorialRuntimeTarget _skillTreeItemsContainer;
        private TutorialRuntimeTarget _upgradeSkillTreeButton;
        private TutorialRuntimeTarget _skillTreeInfoContentPanel;
        private CancellationTokenSource _tutorialFlowCancellationTokenSource;
        private Registry<TutorialTargetObjectAppearedMessage> _tutorialTargetObjectAppearedRegistry;

        #endregion Members

        #region Properties

        public override TutorialType TutorialType => TutorialType.SkillTree;

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
            if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.SkillTreeTab)
                _skillTreeTab = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.SkillTreeItemsContainer)
                _skillTreeItemsContainer = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.SkillTreeInfoContentPanel)
                _skillTreeInfoContentPanel = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
            else if (tutorialTargetObjectAppearedMessage.TutorialTargetType == TutorialTargetType.UpgradeSkillTreeButton)
                _upgradeSkillTreeButton = tutorialTargetObjectAppearedMessage.TutorialRuntimeTarget;
        }

        private async UniTask StartTutorialAsync(CancellationToken cancellationToken)
        {
            await WaitForSkillTreeTabShowedUpAsync(cancellationToken);
            await StartTutorialOnSkillTreeTabAsync(cancellationToken);
            await WaitForSkillTreeItemsContainerShowedUpAsync(cancellationToken);
            await StartTutorialOnSkillTreeSecondBrandItemButtonAsync(cancellationToken);
            await WaitForSkillTreeInfoContentPanelAndUpgradeSkillTreeButtonShowedUpAsync(cancellationToken);
            await SetUpTutorialOnUpgradeSkillTreeButtonAsync(cancellationToken);
        }

        private async UniTask WaitForSkillTreeTabShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _skillTreeTab != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnSkillTreeTabAsync(CancellationToken cancellationToken)
        {
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenSkillTreeTab, _skillTreeTab);
            await UniTask.CompletedTask;
        }

        private async UniTask WaitForSkillTreeItemsContainerShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _skillTreeItemsContainer != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask StartTutorialOnSkillTreeSecondBrandItemButtonAsync(CancellationToken cancellationToken)
        {
            var skillTreeItemsContainer = _skillTreeItemsContainer.gameObject.GetComponent<SkillTreeItemsContainer>();
            await UniTask.WaitUntil(() => skillTreeItemsContainer.NumberOfCells > 1, cancellationToken: cancellationToken);
            var skillTreeItemsRow = skillTreeItemsContainer.GetCellView(1);
            var skillTreeSecondBrandItemButton = GetTutorialSkillTreeSecondBrandItemButton(skillTreeItemsRow.transform);
            SetUpAndPlayRuntimeTutorial(TutorialRuntimeStepType.LightenSkillTreeSecondBrandItemButton, 
                                                    new TutorialRuntimeTarget(skillTreeSecondBrandItemButton, TutorialTargetBoundType.RectTransform));
        }

        private async UniTask WaitForSkillTreeInfoContentPanelAndUpgradeSkillTreeButtonShowedUpAsync(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => _skillTreeInfoContentPanel != null && _upgradeSkillTreeButton != null, cancellationToken: cancellationToken);
            await UniTask.CompletedTask;
        }

        private async UniTask SetUpTutorialOnUpgradeSkillTreeButtonAsync(CancellationToken cancellationToken)
        {
            SetUpRuntimeTutorial(TutorialRuntimeStepType.LightenUpgradeSkillTreeButton, _upgradeSkillTreeButton, _skillTreeInfoContentPanel);
            await UniTask.CompletedTask;
        }

        private GameObject GetTutorialSkillTreeSecondBrandItemButton(Transform skillTreeItemsRowTransform)
        {
            var secondBrandItemButton = skillTreeItemsRowTransform.GetChild(0).GetChild(3).GetChild(0).GetChild(0).gameObject;
            return secondBrandItemButton;
        }

        #endregion Class Methods
    }
}