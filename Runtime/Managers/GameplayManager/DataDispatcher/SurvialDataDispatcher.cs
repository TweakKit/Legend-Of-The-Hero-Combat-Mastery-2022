using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Gameplay.EntitySystem;
using Runtime.Gameplay.Manager;
using Runtime.Server.CallbackData;
using Cysharp.Threading.Tasks;

namespace Runtime.Manager.Data
{
    public class SurvialDataDispatcher : DataDispatcher
    {
        #region Members

        [SerializeField]
        private StageInfoConfigItem _stageInfoData;

        #endregion Members

        #region Properties

        public override bool StartedByWatchedAds
        {
            get => false;
            set { }
        }

        public override long IncreasedEnemyDeafeatedNumber => 0;
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

        public override StageInfoConfigItem StageInfoData => _stageInfoData;
        public override StageInfoConfigItem NextStageInfoData => default;
        public override ResourceData[] StageReviveCostResourceData => null;

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
            var survivalStageEndCallbackData = data as SurvivalStageEndCallbackData;
            DataManager.Server.ResetSurvivalProgress(StageId);
            DataManager.Server.AddResourcesData(survivalStageEndCallbackData.StageRewardsResourcesData, ResourceEarnSourceType.StageEnd);
            DataManager.Server.SetTotalEnemyKilled(survivalStageEndCallbackData.TotalEnemyKilled);
        }

        public override async UniTask InitEquipmentSystems(HeroModel heroModel)
            => await MechanicSystemManager.Instance.InitAsync(heroModel, SelectedEquipments.Values.ToArray());

        #endregion Class Methods
    }
}