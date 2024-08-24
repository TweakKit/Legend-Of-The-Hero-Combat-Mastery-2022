using UnityEngine;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Tutorial
{
    /// <summary>
    /// This component is attached to those objects whose are parts of tutorials.<br/>
    /// When it appears in the scene, a notification will be published telling the world that this tutorial target is now on the scene,
    /// and whatever the corresponding tutorial will pick up and handle it own business there.
    /// </summary>
    public class TutorialTargetObject : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private TutorialTargetType _tutorialTargetType = TutorialTargetType.None;
        [SerializeField]
        private TutorialTargetBoundType _tutorialTargetBoundType = TutorialTargetBoundType.RectTransform;

        #endregion Members

        #region API Methods

        private void Start()
        {
            var tutorialRuntimeTarget = new TutorialRuntimeTarget(gameObject, _tutorialTargetBoundType);
            var tutorialObjectAppearedMessage = new TutorialTargetObjectAppearedMessage(_tutorialTargetType, tutorialRuntimeTarget);
            Messenger.Publisher().Publish(tutorialObjectAppearedMessage);
        }

        #endregion API Methods
    }

    /// <summary>
    /// This class holds a reference to the game object that plays a role in a tutorial, along with its bound type.<br/>
    /// It's the source for creating a runtime TutorialBlockTargetData in a sequence.<br/>
    /// </summary>
    public class TutorialRuntimeTarget
    {
        #region Members

        public GameObject gameObject;
        public TutorialTargetBoundType targetBoundType;

        #endregion Members

        #region Struct Methods

        public TutorialRuntimeTarget(GameObject gameObject, TutorialTargetBoundType targetBoundType)
        {
            this.gameObject = gameObject;
            this.targetBoundType = targetBoundType;
        }

        #endregion Struct Methods
    }

    public enum TutorialTargetType
    {
        None = -1,
        NextStageButton = 1,
        EquipButton = 3,
        GachaButton = 4,
        HeroEquipButton = 5,
        BattleButton = 6,
        JuiceFactoryStructureObject = 7,
        JuiceFactoryModal = 8,
        JuiceJugsContainer = 9,
        BackMenuButton = 10,
        OrderStructureObject = 11,
        JuiceFactoryOutput = 12,
        SendOrderReturnButton = 13,
        OrderStructureModal = 14,
        PlayStageButton = 15,
        BackButton = 16,
        SkipTimeButton = 17,
        UseSkipFiveMinuteButton = 18,
        InventoryEquipmentItemsContainer = 19,
        SkillTreeTab = 20,
        UpgradeSkillTreeButton = 21,
        SkillTreeItemsContainer = 22,
        SkillTreeInfoContentPanel = 23,
        JuiceFactoryFourthInputHolder = 24,
        Gacha1PreminumButton = 25,
        StageItemsContainer = 26,
    }
}