using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Gameplay.EntitySystem;
using Runtime.Server.CallbackData;
using Cysharp.Threading.Tasks;

namespace Runtime.Manager.Data
{
    public class FakeDataDispatcher : DataDispatcher
    {
        #region Members

        [SerializeField]
        private int _heroLevel = 1;
        [SerializeField]
        private WeaponType _weaponType;
        [SerializeField]
        private RarityType _weaponRarity = RarityType.Ultimate;
        [SerializeField]
        private int _weaponLevel = 30;
        [SerializeField]
        private int _weaponStar = 3;
        [SerializeField]
        private EquipmentEquippedData[] _equipments;
        [SerializeField]
        private int _stageId = 101001;
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
        public override int PreviousHeroLevel => _heroLevel;
        public override int HeroLevel => _heroLevel;
        public override long HeroExp => 0;
        public override int StageId => _stageId;
        public override string BattleId => "";
        public override long HeroNextLevelRequiredExp => long.MaxValue;
        public override StageInfoConfigItem StageInfoData => _stageInfoData;
        public override StageInfoConfigItem NextStageInfoData => default;
        public override ResourceData[] StageReviveCostResourceData => null;

        public override Dictionary<EquipmentType, EquipmentEquippedData> SelectedEquipments
        {
            get
            {
                Dictionary<EquipmentType, EquipmentEquippedData> selectedEquipments = new();
                selectedEquipments.Add(EquipmentType.Weapon, new EquipmentEquippedData(EquipmentGrade.A, EquipmentType.Weapon, _weaponRarity, (int)_weaponType, _weaponLevel, _weaponStar));

                foreach (var equipment in _equipments)
                    selectedEquipments.Add(equipment.EquipmentType, equipment);

                return selectedEquipments;
            }
        }

        public override SkillTreeSystemData[] UnlockedSkillTreeSystems => null;

        #endregion Properties

        #region Class Methods

        public override async UniTask<HeroStatsInfo> GetHeroStatsInfo(CharacterLevelStats heroLevelStats)
        {
            var heroStats = new HeroStatsInfo(heroLevelStats);
            await heroStats.UpdateDetectRange(SelectedEquipments.FirstOrDefault(x => x.Key == EquipmentType.Weapon).Value);
            return heroStats;
        }

        public override void UpdateStageEnd(StageEndCallbackData data, bool isWin) { }
        public override UniTask InitEquipmentSystems(HeroModel heroModel) => UniTask.CompletedTask;

        #endregion Class Methods
    }
}