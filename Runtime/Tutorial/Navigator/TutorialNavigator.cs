using Runtime.Core.Singleton;
using Runtime.Manager.Data;
using Runtime.Definition;
using Runtime.FeatureSystem;

namespace Runtime.Tutorial
{
    public class TutorialNavigator : MonoSingleton<TutorialNavigator>
    {
        #region Members

        private TutorialManager _currentTutorialManager;

        #endregion Members

        #region Properties

        public static TutorialManager CurrentTutorial
            => Instance._currentTutorialManager;

        public bool CanStartGameplayGuideTutorial
        {
            get
            {
                bool hasPlayedGameplayTutorial = DataManager.Local.HasPlayedGameplayTutorial();
                return !hasPlayedGameplayTutorial;
            }
        }

        public bool CanStartReceiveGachaTokenTutorial
        {
            get
            {
                var isGachaTokenUnlocked = FeatureUnlockChecker.IsGachaTokenUnlocked();
                var hasCompletedReceiveGachaTokenTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.ReceiveGachaToken);
                return isGachaTokenUnlocked && !hasCompletedReceiveGachaTokenTutorial;
            }
        }

        public bool CanStartNotifyUnlockJuicyFactoryTutorial
        {
            get
            {
                var isJuiceFactoryStructureUnlocked = FeatureUnlockChecker.IsStructureUnlocked(StructureType.JuiceFactory);
                var hasCompletedNotifyUnlockJuiceFactoryTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.NotifyUnlockJuiceFactory);
                return isJuiceFactoryStructureUnlocked && !hasCompletedNotifyUnlockJuiceFactoryTutorial;
            }
        }

        public bool CanStartUseJuiceFactoryTutorial
        {
            get
            {
                var isJuiceFactoryStructureUnlocked = FeatureUnlockChecker.IsStructureUnlocked(StructureType.JuiceFactory);
                var hasCompletedNotifyUnlockJuiceFactoryTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.NotifyUnlockJuiceFactory);
                var hasCompletedUseJuiceFactoryTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UseJuiceFactoryStructure);
                return isJuiceFactoryStructureUnlocked && hasCompletedNotifyUnlockJuiceFactoryTutorial && !hasCompletedUseJuiceFactoryTutorial;
            }
        }

        public bool CanStartUseOrderStructureTutorial
        {
            get
            {
                var hasCompletedUseJuiceFactoryStructureTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UseJuiceFactoryStructure);
                var hasCompletedUseOrderStructureTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UseOrderStructure);
                return hasCompletedUseJuiceFactoryStructureTutorial && !hasCompletedUseOrderStructureTutorial;
            }
        }

        public bool CanStartUnlockGachaTutorial
        {
            get
            {
                var isUnlockGachaUnlocked = FeatureUnlockChecker.IsUnlockGachaUnlocked();
                var hasCompletedUnlockGachaTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UnlockGacha);
                return isUnlockGachaUnlocked && !hasCompletedUnlockGachaTutorial;
            }
        }

        public bool CanStartGachaEquipmentTutorial
        {
            get
            {
                bool hasCompletedUnlockGachaTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UnlockGacha);
                bool hasCompletedGachaEquipmentTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.GachaEquipment);
                return hasCompletedUnlockGachaTutorial && !hasCompletedGachaEquipmentTutorial;
            }
        }

        public bool CanStartUnlockEquipmentTutorial
        {
            get
            {
                var isHeroEquimentUnlocked = FeatureUnlockChecker.IsHeroEquipUnlocked();
                bool hasCompletedUnlockEquipmentTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UnlockEquipment);
                return isHeroEquimentUnlocked && !hasCompletedUnlockEquipmentTutorial;
            }
        }

        public bool CanStartEquipEquipmentTutorial
        {
            get
            {
                var isEquipEquipmentUnlocked = FeatureUnlockChecker.IsEquipEquipmentUnlocked();
                bool hasCompletedEquipEquimentTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.EquipEquipment);
                return isEquipEquipmentUnlocked && !hasCompletedEquipEquimentTutorial;
            }
        }

        public bool CanStartUnlockSkillTreeTutorial
        {
            get
            {
                bool hasCompletedUseOrderStructureTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UseOrderStructure);
                bool hasCompletedUnlockSkillTreeTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UnlockSkillTree);
                return hasCompletedUseOrderStructureTutorial && !hasCompletedUnlockSkillTreeTutorial;
            }
        }

        public bool CanStartSkillTreeTutorial
        {
            get
            {
                bool hasCompletedUnlockSkillTreeTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.UnlockSkillTree);
                bool hasCompletedSkillTreeTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.SkillTree);
                if (hasCompletedUnlockSkillTreeTutorial && !hasCompletedSkillTreeTutorial)
                {
                    var firstBranchSkillTreeHighestUnlocked = DataManager.Server.FirstBranchSkillTreeHighestUnlocked;
                    var secondBranchSkillTreeHighestUnlocked = DataManager.Server.SecondBranchSkillTreeHighestUnlocked;
                    var skillTreeDataConfig = DataManager.Config.GetSkillTreeDataInfo();
                    var heroLevel = DataManager.Server.HeroLevel;
                    if (firstBranchSkillTreeHighestUnlocked < 0 && secondBranchSkillTreeHighestUnlocked < 0)
                    {
                        if (skillTreeDataConfig.secondBranchItems.Length > 0)
                        {
                            if (skillTreeDataConfig.secondBranchItems[0].unlockLevel <= heroLevel)
                            {
                                var costs = skillTreeDataConfig.secondBranchItems[0].costs;
                                var isEnoughResource = true;
                                foreach (var cost in costs)
                                {
                                    var currentValue = DataManager.Server.GetResource(cost.resourceType, cost.resourceId);
                                    if (currentValue < cost.resourceNumber)
                                    {
                                        isEnoughResource = false;
                                        break;
                                    }
                                }

                                if (isEnoughResource)
                                    return true;
                            }
                        }
                    }
                    else return false;
                }

                return false;
            }
        }

        public bool CanStartHeadToBattleTutorial
        {
            get
            {
                bool hasCompletedEquipEquipmentTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.EquipEquipment);
                bool hasCompletedHeadToBattleTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.HeadToBattle);
                return hasCompletedEquipEquipmentTutorial && !hasCompletedHeadToBattleTutorial;
            }
        }

        public bool CanStartShowHowToGoToBattleTutorial
        {
            get
            {
                bool hasCompletedHeadToBattleTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.HeadToBattle);
                bool hasCompletedShowHowToGoToBattleTutorial = DataManager.Server.HasCompletedTutorial(TutorialType.ShowHowToGoToBattle);
                return hasCompletedHeadToBattleTutorial && !hasCompletedShowHowToGoToBattleTutorial;
            }
        }

        public bool HasCurrentTutorialFinished
        {
            get
            {
                return _currentTutorialManager == null || _currentTutorialManager.HasCompletedAllSequences;
            }
        }

        #endregion Properties

        #region Class Methods

        public static void SetCurrentTutorial(TutorialManager tutorialManager)
        {
            if (Instance._currentTutorialManager != null)
                Instance._currentTutorialManager.ResetTutorial();
            Instance._currentTutorialManager = tutorialManager;
        }

        public void ExecuteGameplayGuideTutorial(GameplayGuideTutorialManager gameplayGuideTutorialManager)
        {
            SetCurrentTutorial(gameplayGuideTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteUnlockGachaTutorial()
        {
            var unlockGachaTutorialManager = FindObjectOfType<UnlockGachaTutorialManager>();
            SetCurrentTutorial(unlockGachaTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteGachaEquipmentTutorial()
        {
            var gachaEquipmentTutorialManager = FindObjectOfType<GachaEquipmentTutorialManager>();
            SetCurrentTutorial(gachaEquipmentTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteUnlockEquipmentTutorial()
        {
            var unlockEquipmentTutorialManager = FindObjectOfType<UnlockEquipmentTutorialManager>();
            SetCurrentTutorial(unlockEquipmentTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteEquipEquipmentTutorial()
        {
            var equipEquipmentTutorialManager = FindObjectOfType<EquipEquipmentTutorialManager>();
            SetCurrentTutorial(equipEquipmentTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteSkillTreeTutorial()
        {
            var skillTreeTutorialManager = FindObjectOfType<SkillTreeTutorialManager>();
            SetCurrentTutorial(skillTreeTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteHeadToBattleTutorial()
        {
            var headToBattleTutorialManager = FindObjectOfType<HeadToBattleTutorialManager>();
            SetCurrentTutorial(headToBattleTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteShowHowToGoToBattleTutorial()
        {
            var showHowToGoToBattleTutorialManager = FindObjectOfType<ShowHowToGoToBattleTutorialManager>();
            SetCurrentTutorial(showHowToGoToBattleTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteUseJuiceFactoryTutorial()
        {
            var useJuiceFactoryTutorialManager = FindObjectOfType<UseJuiceFactoryStructureTutorialManager>();
            SetCurrentTutorial(useJuiceFactoryTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteUseOrderStructureTutorial()
        {
            var useOrderStructureTutorialManager = FindObjectOfType<UseOrderStructureTutorialManager>();
            SetCurrentTutorial(useOrderStructureTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteUnlockSkillTreeTutorial()
        {
            var unlockSkillTreeTutorialManager = FindObjectOfType<UnlockSkillTreeTutorialManager>();
            SetCurrentTutorial(unlockSkillTreeTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteReceiveGachaTokenTutorial()
        {
            var receiveGachaTokenTutorialManager = FindObjectOfType<ReceiveGachaTokenTutorialManager>();
            SetCurrentTutorial(receiveGachaTokenTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        public void ExecuteNotifyUnlockJuiceFactoryTutorial()
        {
            var notifyUnlockJuiceFactoryTutorialManager = FindObjectOfType<NotifyUnlockJuiceFactoryTutorialManager>();
            SetCurrentTutorial(notifyUnlockJuiceFactoryTutorialManager);
            CurrentTutorial.StartTutorialRuntime();
        }

        #endregion Class Methods
    }
}