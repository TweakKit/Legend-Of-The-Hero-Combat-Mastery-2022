using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.FeatureSystem;
using Runtime.Gameplay.Quest;
using Runtime.Localization;
using Runtime.Manager;
using Runtime.Manager.Data;
using UnityScreenNavigator.Runtime.Core.Shared.Views;

namespace Runtime.UI
{
    public partial class ScreenNavigator
    {
        public async UniTask Navigate(NavigationTargetType navigationTargetType, int targetId)
        {
            if (navigationTargetType == NavigationTargetType.Battle)
            {
                var options = new WindowOptions(ScreenIds.STAGE_SELECTION_SCREEN);
                await LoadScreen(options, new StageSelectionScreenData(false));
            }
            else if (navigationTargetType == NavigationTargetType.Stage)
            {
                var isUnlockedTargetStage = await DataManager.Server.CheckStageUnlocked(targetId);
                if (isUnlockedTargetStage)
                {
                    DataManager.Local.SetSelectStage(targetId);
                }

                var selectedStageId = DataManager.Local.SelectedFullStageId;
                var stageInfo = await DataManager.Config.LoadStageInfo();
                var stageData = stageInfo.items.FirstOrDefault(x => x.stageId == selectedStageId);
                var options = new WindowOptions(ScreenIds.STAGE_SELECTION_SCREEN);
                await LoadScreen(options, new StageSelectionScreenData(false));

                if (isUnlockedTargetStage)
                {
                    var stageExtractId = selectedStageId.ExtractStageId();
                    var userStageStarInfo = DataManager.Server.GetUserStageInfo(selectedStageId);
                    var stageQuestsDataTuple = new List<Tuple<bool, string>>();
                    var questDataFactory = new QuestDataFactory();

                    foreach (var starQuest in stageData.starQuests)
                    {
                        var hasCompleted = false;
                        var questDataConfigItem = await questDataFactory.GetQuestDataConfigItem(starQuest.questIdentity.questType, starQuest.questIdentity.questDataId);
                        var questData = new QuestData(questDataConfigItem);
                        var questModel = QuestModelFactory.GetQuestModel(starQuest.questIdentity.questType, questData);
                        if (userStageStarInfo != null)
                        {
                            var result = userStageStarInfo.StarStatesDictionary.TryGetValue(starQuest.star, out hasCompleted);
                            if (!result)
                                hasCompleted = false;
                        }
                        stageQuestsDataTuple.Add(new Tuple<bool, string>(hasCompleted, questModel.GetLocalizedInfo()));
                    }

                    var stageCurrentStar = userStageStarInfo == null ? 0 : (uint)userStageStarInfo.TotalStar;
                    var modalData = new StageInfoModalData
                                (
                                    star: stageCurrentStar,
                                    data: stageData,
                                    stageQuestsDataTuple: stageQuestsDataTuple
                                );

                    var modalOptions = new WindowOptions(ModalIds.STAGE_INFO_MODAL);
                    await LoadModal(modalOptions, modalData);
                }
            }
            else if (navigationTargetType == NavigationTargetType.Order)
            {
                var buildStructureType = StructureType.Order;
                var structureUnlockedData = FeatureUnlockChecker.GetStructureUnlockedData(buildStructureType);
                if (structureUnlockedData.isUnlocked)
                {
                    var options = new WindowOptions(ScreenIds.ORDER_SCREEN);
                    await LoadScreen(options, new EmptyScreenData());
                }
                else
                {
                    ToastController.Instance.Show(structureUnlockedData.unlockedRequimentDescription);
                }
            }
            else if (navigationTargetType == NavigationTargetType.JuiceFactory)
            {
                var buildStructureType = StructureType.JuiceFactory;
                var structureUnlockedData = FeatureUnlockChecker.GetStructureUnlockedData(buildStructureType);
                if (structureUnlockedData.isUnlocked)
                {
                    await PopToRootScreen(false);
                    var modalOptions = new WindowOptions(ModalIds.JUICE_FACTORY);
                    await LoadModal(modalOptions, new EmptyModalData());
                }
                else
                {
                    ToastController.Instance.Show(structureUnlockedData.unlockedRequimentDescription);
                }
            }
            else if (navigationTargetType == NavigationTargetType.ShopResource)
            {
                var screenOptions = new WindowOptions(ScreenIds.SHOP_SCREEN);
                await LoadScreen(screenOptions, new ShopScreenData(ShopScreen.RESOURCES_SHOP_SHEET_INDEX));
            }
            else if (navigationTargetType == NavigationTargetType.ResourceBundle)
            {
                var screenOptions = new WindowOptions(ScreenIds.SHOP_SCREEN);
                await LoadScreen(screenOptions, new ShopScreenData(ShopScreen.RESOURCES_BUNDLE_SHEET_INDEX));
            }
            else if (navigationTargetType == NavigationTargetType.CustomizeBundle)
            {
                if (DataManager.Server.HasCompletedTutorial(TutorialType.UseJuiceFactoryStructure))
                {
                    var screenOptions = new WindowOptions(ScreenIds.SHOP_SCREEN);
                    await LoadScreen(screenOptions, new ShopScreenData(ShopScreen.CUSTOMIZE_BUNDLE_SHEET_INDEX));
                }
                else ToastController.Instance.Show(LocalizationManager.GetLocalize(LocalizeTable.POPUP, LocalizeKeys.COMING_SOON));
            }
            else if (navigationTargetType == NavigationTargetType.HeroButton)
            {
                var equipEquipmentUnlockedData = FeatureUnlockChecker.GetHeroEquipUnlockedData();
                if (equipEquipmentUnlockedData.isUnlocked)
                {
                    var screenOptions = new WindowOptions(ScreenIds.INVENTORY_SCREEN);
                    await LoadScreen(screenOptions, new InventoryScreenData(InventoryScreen.EQUIPMENT_INVENTORY_SHEET_INDEX));
                }
                else ToastController.Instance.Show(equipEquipmentUnlockedData.unlockedRequimentDescription);
            }
            else if (navigationTargetType == NavigationTargetType.SkillTree)
            {
                var skillTreeUnlockedData = FeatureUnlockChecker.GetSkillTreeUnlockedData();
                if (skillTreeUnlockedData.isUnlocked)
                {
                    var screenOptions = new WindowOptions(ScreenIds.INVENTORY_SCREEN);
                    await LoadScreen(screenOptions, new InventoryScreenData(InventoryScreen.SKILL_TREE_INVENTORY_SHEET_INDEX));
                }
                else ToastController.Instance.Show(skillTreeUnlockedData.unlockedRequimentDescription);
            }
            else if (navigationTargetType == NavigationTargetType.Gacha)
            {
                var gachaEquipmentUnlockedData = FeatureUnlockChecker.GetUnlockGachaUnlockedData();
                if (gachaEquipmentUnlockedData.isUnlocked)
                {
                    var windowOptions = new WindowOptions(ScreenIds.GACHA_SCREEN, true);
                    LoadScreen(windowOptions, new EmptyScreenData()).Forget();
                }
                else ToastController.Instance.Show(gachaEquipmentUnlockedData.unlockedRequimentDescription);
            }
            else if (navigationTargetType == NavigationTargetType.DailyFortune)
            {
                await PopToRootScreen(false);
                var modalOptions = new WindowOptions(ModalIds.DAILY_FORTUNE);
                await LoadModal(modalOptions, new EmptyModalData());
            }
        }
    }
}