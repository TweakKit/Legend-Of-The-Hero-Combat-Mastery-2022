using System;
using UnityEngine;
using UnityEngine.UI;
using UnityScreenNavigator.Runtime.Core.Shared.Views;
using Runtime.UI;
using Runtime.Definition;
using Runtime.Manager.Data;
using Cysharp.Threading.Tasks;
using TMPro;
using Runtime.Localization;

namespace Runtime.Gameplay.BaseBuilder
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public class BuildStructureHUD : MonoBehaviour
    {
        #region Members

        [Header("=== GENERAL INFO ===")]
        [SerializeField]
        private GameObject _containerPanel;
        [SerializeField]
        private TextMeshProUGUI _nameText;

        [Header("=== MERGE EQUIPMENT INFO ===")]
        [SerializeField]
        private GameObject _buildInProgressElementsContainer;
        [SerializeField]
        private Image _buildTimeBarSlider;
        [SerializeField]
        private TextMeshProUGUI _buildTimeText;

        [Header("=== BUILD COMPLETE ELEMENTS ===")]
        [SerializeField]
        private Button _confirmBuildDoneStatusButton;
        [SerializeField]
        private Button _confirmBuildCompleteButton;

        [Header("=== EDIT BUTTONS CONTAINER ===")]
        [SerializeField]
        private GameObject _editButtonsContainer;
        [SerializeField]
        private Button _infomationButton;
        [SerializeField]
        private Button _skipBuildTimeButton;

        #endregion Members

        #region Properties

        private BuildStructureModel OwnerBuildStructureModel { get; set; }

        #endregion Properties

        #region API Methods

        private void Awake()
        {
            Canvas worldCanvas = gameObject.GetComponent<Canvas>();
            worldCanvas.renderMode = RenderMode.WorldSpace;
            worldCanvas.worldCamera = Camera.main;
        }

        #endregion API Methods

        #region Class Methods

        public void Init(BuildStructureModel buildStructureModel, Action confirmCompleteAfterBuildTimeRanOutAction)
        {
            OwnerBuildStructureModel = buildStructureModel;
            _nameText.text = LocalizationManager.GetLocalize(LocalizeTable.BASE_MAP, LocalizeKeys.GetStructureName(OwnerBuildStructureModel.id));
            _confirmBuildDoneStatusButton.onClick.AddListener(() => confirmCompleteAfterBuildTimeRanOutAction?.Invoke());
            _confirmBuildCompleteButton.onClick.AddListener(() => confirmCompleteAfterBuildTimeRanOutAction?.Invoke());
            _infomationButton.onClick.AddListener(OnClickInformationButton);
            _skipBuildTimeButton.onClick.AddListener(OnClickSkipBuildTimeButton);

            UpdateBuildStatusUI(false);
            _buildInProgressElementsContainer.gameObject.SetActive(false);
            _containerPanel.SetActive(false);
        }

        public void PrepareBuild(TimeSpan totalBuildTimeTimespan)
        {
            _buildTimeBarSlider.fillAmount = 1.0f;
            _buildTimeText.text = totalBuildTimeTimespan.ToString(Constants.TIME_STRING_FORMAT);
            _containerPanel.SetActive(true);
            _buildInProgressElementsContainer.SetActive(true);
            Unselect();
            UpdateBuildStatusUI(false);
        }

        public void CompleteBuild()
        {
            _buildInProgressElementsContainer.SetActive(false);
            _editButtonsContainer.SetActive(false);
            _containerPanel.SetActive(true);
            UpdateBuildStatusUI(true);
        }

        public void UpdateBuild(TimeSpan leftBuildTimespan, TimeSpan totalBuildTimeTimespan)
        {
            _buildTimeBarSlider.fillAmount = (float)(leftBuildTimespan.TotalSeconds / totalBuildTimeTimespan.TotalSeconds);
            _buildTimeText.text = leftBuildTimespan.ToString(Constants.TIME_STRING_FORMAT);
            if (leftBuildTimespan.TotalSeconds <= 0)
            {
                _buildInProgressElementsContainer.SetActive(false);
                UpdateBuildStatusUI(true);
                Unselect();
            }
        }

        public void Hide()
        {
            Unselect();
            UpdateBuildStatusUI(false);
            _containerPanel.SetActive(false);
        }

        public void Show(bool hasFinishedBuildTime = false)
        {
            if (OwnerBuildStructureModel.currentState != StructureState.Built)
            {
                _containerPanel.SetActive(true);
                if (hasFinishedBuildTime)
                {
                    UpdateBuildStatusUI(true);
                    _buildInProgressElementsContainer.SetActive(false);
                }
                else
                {
                    _buildInProgressElementsContainer.SetActive(true);
                }
            }
        }

        public void SelectToUpgrade()
        {
            _containerPanel.SetActive(true);
            _editButtonsContainer.SetActive(true);
            _skipBuildTimeButton.gameObject.SetActive(false);
        }

        public void SelectToSkipBuildTime()
        {
            _containerPanel.SetActive(true);
            _editButtonsContainer.SetActive(true);
            _skipBuildTimeButton.gameObject.SetActive(true);
        }

        public void Unselect()
        {
            _editButtonsContainer.SetActive(false);
            if (OwnerBuildStructureModel.currentState == StructureState.Built)
                _containerPanel.SetActive(false);
        }

        private void UpdateBuildStatusUI(bool hasFinishedBuildTime)
        {
            _confirmBuildDoneStatusButton.gameObject.SetActive(hasFinishedBuildTime);
            _confirmBuildCompleteButton.gameObject.SetActive(hasFinishedBuildTime);
        }

        private void OnClickUpgradeButton()
        {
            var modalData = new StructureUpgradeModalData(OwnerBuildStructureModel);
            var windowOptions = new WindowOptions(ModalIds.STRUCTURE_UPGRADE, true);
            ScreenNavigator.Instance.LoadModal(windowOptions, modalData).Forget();
        }

        private void OnClickSkipBuildTimeButton()
        {
            var builtConstruction = DataManager.Server.GetConstruction(OwnerBuildStructureModel.uid);
            var buildTimeRequired = OwnerBuildStructureModel.id.GetBuildTime(OwnerBuildStructureModel.currentLevel + 1);
            var modalData = new SpeedUpConstructionModalData(builtConstruction.StructureUId,
                                                             OwnerBuildStructureModel.id,
                                                             OwnerBuildStructureModel.currentLevel,
                                                             buildTimeRequired,
                                                             builtConstruction.ConstructionType,
                                                             builtConstruction.StartTimeTotalSecondsFromOriginTime,
                                                             MoneyType.Gem);
            var windowOptions = new WindowOptions(ModalIds.SPEED_UP_CONSTRUCTION, true);
            ScreenNavigator.Instance.LoadModal(windowOptions, modalData).Forget();
        }

        private void OnClickInformationButton()
        {
            var modalData = new StructureInfoModalData(OwnerBuildStructureModel.id,
                                                       OwnerBuildStructureModel.id.GetStructureName(),
                                                       OwnerBuildStructureModel.id.GetStructureDescription(),
                                                       OwnerBuildStructureModel.currentLevel);
            var windowOptions = new WindowOptions(ModalIds.STRUCTURE_INFO, true);
            ScreenNavigator.Instance.LoadModal(windowOptions, modalData).Forget();
        }

        #endregion Class Methods
    }
}