using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Core.Singleton;
using Runtime.Definition;
using Runtime.Gameplay.EntitySystem;
using Runtime.Server.CallbackData;
using Cysharp.Threading.Tasks;

namespace Runtime.Manager.Data
{
    public abstract class DataDispatcher : MonoSingleton<DataDispatcher>
    {
        #region Properties

        public abstract int PreviousHeroLevel { get; }
        public abstract int HeroLevel { get; }
        public abstract bool StartedByWatchedAds { get; set; } 
        public abstract long HeroExp { get; }
        public abstract int StageId { get; }
        public abstract string BattleId { get; }
        public abstract long HeroNextLevelRequiredExp { get; }
        public abstract SkillTreeSystemData[] UnlockedSkillTreeSystems { get; }
        public abstract StageInfoConfigItem StageInfoData { get; }
        public abstract StageInfoConfigItem NextStageInfoData { get; }
        public abstract ResourceData[] StageReviveCostResourceData { get; }
        public abstract Dictionary<EquipmentType, EquipmentEquippedData> SelectedEquipments { get; }
        public abstract long IncreasedEnemyDeafeatedNumber { get; }

        #endregion Properties

        #region Class Methods

        public abstract void UpdateStageEnd(StageEndCallbackData data, bool isWin);
        public abstract UniTask<HeroStatsInfo> GetHeroStatsInfo(CharacterLevelStats heroLevelStats);
        public abstract UniTask InitEquipmentSystems(HeroModel heroModel);

        #endregion Class Methods
    }
}