using System.Linq;
using System.Collections.Generic;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Gameplay.EntitySystem;
using Runtime.Server.CallbackData;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Manager.Data
{
    public class RealDataDispatcher : DataDispatcher
    {
        #region Members

        private long _increasedEnemyDeafeatedNumber;

        #endregion Members

        #region Properties

        public override bool StartedByWatchedAds
        {
            get => DataManager.Transitioned.StartedStageByWatchAds;
            set => DataManager.Transitioned.SetStartStageByWatchAds(value);
        }

        public override int PreviousHeroLevel => DataManager.Server.PreviousHeroLevel;
        public override int HeroLevel => DataManager.Server.HeroLevel;
        public override long HeroExp => DataManager.Server.HeroExp;
        public override int StageId => DataManager.Local.SelectedFullStageId;
        public override string BattleId => DataManager.Transitioned.BattleId;

        public override long HeroNextLevelRequiredExp
        {
            get
            {
                var heroExpConfig = GameplayDataManager.Instance.HeroExpConfig;
                var (isMaxLevel, nextLevelRequiredExp) = heroExpConfig.GetHeroExpLevelRequired(HeroLevel);
                return nextLevelRequiredExp;
            }
        }

        public override StageInfoConfigItem StageInfoData
        {
            get
            {
                var stageInfoData = GameplayDataManager.Instance.StageInfoConfig;
                return stageInfoData;
            }
        }

        public override StageInfoConfigItem NextStageInfoData
        {
            get
            {
                var nextStageInfoData = GameplayDataManager.Instance.NextStageInfoConfig;
                return nextStageInfoData;
            }
        }

        public override ResourceData[] StageReviveCostResourceData
        {
            get
            {
                var reviveCostResourceData = StageInfoData.reviveCost;
                return reviveCostResourceData;
            }
        }

        public override Dictionary<EquipmentType, EquipmentEquippedData> SelectedEquipments
        {
            get
            {
                var equippedData = DataManager.Server.GetEquippedData();

                Dictionary<EquipmentType, EquipmentEquippedData> selectedEquipments = new();

                foreach (var equipmentData in equippedData)
                {
                    var extractEquipment = equipmentData.FullId.ExtractEquipmentId();
                    var equipmentId = Constants.GetEquipmentId(extractEquipment.equipmentType, extractEquipment.setId);
                    selectedEquipments.Add(extractEquipment.equipmentType, new EquipmentEquippedData(equipmentData.EquipmentGrade, equipmentData.EquipmentType, equipmentData.EquipmentRarity, equipmentId, equipmentData.Level, equipmentData.Star));
                }

                if (!selectedEquipments.ContainsKey(EquipmentType.Weapon))
                    selectedEquipments.Add(EquipmentType.Weapon, new EquipmentEquippedData(EquipmentGrade.A, EquipmentType.Weapon, RarityType.Common, (int)WeaponType.BidetGun, 1, 1));

                return selectedEquipments;
            }
        }

        public override SkillTreeSystemData[] UnlockedSkillTreeSystems
        {
            get
            {
                var secondBranchHighestUnlocked = DataManager.Server.SecondBranchSkillTreeHighestUnlocked;
                var skillTreeDataSystem = GameplayDataManager.Instance.SkillTreeSecondBranchConfigItems.Where(x => x.id <= secondBranchHighestUnlocked && x.skillTreeSystemData.skillTreeSystemType != SkillTreeSystemType.None).Select(x => x.skillTreeSystemData);
                return skillTreeDataSystem.ToArray();
            }
        }

        public override long IncreasedEnemyDeafeatedNumber => _increasedEnemyDeafeatedNumber;

        #endregion Properties

        #region Class Methods

        public override async UniTask<HeroStatsInfo> GetHeroStatsInfo(CharacterLevelStats heroLevelStats)
        {
            var heroStats = new HeroStatsInfo(heroLevelStats);
            await heroStats.UpdateHeroStats(SelectedEquipments.Values.ToArray());
            return heroStats;
        }

        public override void UpdateStageEnd(StageEndCallbackData data, bool isWin)
        {
            var campaignStageEndCallbackData = data as CampaignStageEndCallbackData;
            DataManager.Server.AddResourcesData(campaignStageEndCallbackData.StageRewardsResourcesData, ResourceEarnSourceType.StageEnd);
            DataManager.Server.AddResourcesData(campaignStageEndCallbackData.FirstClearRewardsResourcesData, ResourceEarnSourceType.StageEnd);
            DataManager.Server.AddResourcesData(campaignStageEndCallbackData.DroppableRewardsResourcesData, ResourceEarnSourceType.StageEnd);
            DataManager.Server.UpdateHeroLevel(campaignStageEndCallbackData.HeroLevel);
            DataManager.Server.UpdateHeroExp(campaignStageEndCallbackData.HeroExp);
            DataManager.Server.UpdateHighestLossStage(campaignStageEndCallbackData.HighestLossStage);

            _increasedEnemyDeafeatedNumber = data.TotalEnemyKilled - DataManager.Server.TotalEnemyKilled;
            DataManager.Server.SetTotalEnemyKilled(data.TotalEnemyKilled);
            if (isWin)
                DataManager.Server.UpdateCampaignStarData(StageId, campaignStageEndCallbackData.StarBitMask);
        }

        public override async UniTask InitEquipmentSystems(HeroModel heroModel)
            => await MechanicSystemManager.Instance.InitAsync(heroModel, SelectedEquipments.Values.ToArray());

        #endregion Class Methods
    }
}