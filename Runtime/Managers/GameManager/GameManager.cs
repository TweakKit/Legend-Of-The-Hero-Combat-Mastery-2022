using System;
using System.Threading;
using UnityEngine;
using Runtime.Core.Singleton;
using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Utilities;
using Runtime.Tracking;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;
using System.Linq;
using Core.Foundation.PubSub;
using Runtime.Gameplay.EntitySystem;
using Runtime.Message;

namespace Runtime.Manager
{
    public class GameManager : PersistentMonoSingleton<GameManager>
    {
        #region Members

        private GameStatusType _gameStatusType;
        private GameFlowTimeControllerType _lastGameFlowTimeControllerType;
        private CancellationTokenSource _staminaRegencancellationTokenSource;

        #endregion Members

        #region Properties

        public GameStatusType GameStatusType => _gameStatusType;
        public bool IsGameOver => _gameStatusType == GameStatusType.WonStage ||
                                  _gameStatusType == GameStatusType.LostStage;
        public bool IsInGameplay => _gameStatusType == GameStatusType.StartStage;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            Time.timeScale = 1;
            _gameStatusType = GameStatusType.None;
            _lastGameFlowTimeControllerType = GameFlowTimeControllerType.None;
            _staminaRegencancellationTokenSource = new();
        }

        #endregion API Methods

        #region Class Methods

        public void StopGameFlow(GameFlowTimeControllerType gameFlowTimeControllerType)
        {
            if (_lastGameFlowTimeControllerType == GameFlowTimeControllerType.None)
            {
                Time.timeScale = 0.0f;
                _lastGameFlowTimeControllerType = gameFlowTimeControllerType;
            }
        }

        public void SlowDownGameFlow(GameFlowTimeControllerType gameFlowTimeControllerType)
        {
            if (_lastGameFlowTimeControllerType == GameFlowTimeControllerType.None)
            {
                Time.timeScale = Constants.STAGE_RESULT_SLOW_DOWN_TIME_SCALE;
                _lastGameFlowTimeControllerType = gameFlowTimeControllerType;
            }
        }

        public void ContinueGameFlow(GameFlowTimeControllerType gameFlowTimeControllerType)
        {
            if (gameFlowTimeControllerType == _lastGameFlowTimeControllerType)
            {
                Time.timeScale = 1.0f;
                _lastGameFlowTimeControllerType = GameFlowTimeControllerType.None;
            }
        }

        public void SetGameStatusType(GameStatusType gameplayStatusType)
        {
            _gameStatusType = gameplayStatusType;

            switch (gameplayStatusType)
            {
                case GameStatusType.StartStage:
                {
#if TRACKING
                    TrackingStartStage().Forget();
#endif
                    break;
                }

                case GameStatusType.WonStage:
                case GameStatusType.LostStage:
                case GameStatusType.QuitStage:
                {
#if TRACKING
                    int stageId = DataDispatcher.Instance.StageId;
                    string battleId = DataDispatcher.Instance.BattleId;
                    string heroId = Constants.HERO_ID.ToString();
                    string petId = "";
                    var equipmentsDictionary = DataDispatcher.Instance.SelectedEquipments;
                    string weaponId = equipmentsDictionary.ContainsKey(EquipmentType.Weapon) ? equipmentsDictionary[EquipmentType.Weapon].EquipmentId.ToString() : "";
                    StageEndType stageEndType = gameplayStatusType == GameStatusType.QuitStage
                                              ? StageEndType.Interrupted
                                              : gameplayStatusType == GameStatusType.WonStage
                                              ? StageEndType.Win
                                              : StageEndType.Lose;
                    long playTime = GameplayManager.Instance.CurrentGameplayTime;
                    int win = stageEndType == StageEndType.Win
                            ? 1
                            : stageEndType == StageEndType.Lose
                            ? 0
                            : 2;
                    int defeatedEnemy = EntitiesManager.Instance.DefeatedEnemiesCount;
                    uint wave = GameplayManager.Instance.CurrentWaveIndex;
                    int goldAchieve = 0;
                    int traps = 0;
                    var heroModel = EntitiesManager.Instance.HeroModel;
                    int hpRemaining = (int)heroModel.CurrentHp;
                    float hpRatio = heroModel.CurrentHp / heroModel.MaxHp;
                    string equipmentsId = "";
                    int star = GameplayManager.Instance.AchievedQuestsCount;
                    foreach (var entry in equipmentsDictionary)
                    {
                        if (entry.Key != EquipmentType.Weapon)
                        {
                            var addedEquipmentId = entry.Value.EquipmentId.ToString();
                            equipmentsId += addedEquipmentId + "/";
                        }
                    }
                    FirebaseManager.Instance.TrackStageEnd(battleId,
                                                           heroId,
                                                           weaponId,
                                                           equipmentsId,
                                                           petId,
                                                           stageId,
                                                           playTime,
                                                           win,
                                                           defeatedEnemy,
                                                           wave,
                                                           goldAchieve,
                                                           traps,
                                                           hpRemaining,
                                                           hpRatio,
                                                           star);
                    if (stageEndType == StageEndType.Win)
                        AppsFlyerManager.Instance.TrackCompleteStage(DataManager.Local.UserId,
                                                                     stageId.ToString());
#endif
                    break;
                }

                case GameStatusType.JustCameHomeAfterFailStage:
                {
                    DataManager.Local.SetBackHomeAfterLostCountTimes();
                    break;
                }
            }
        }

        public void UpdateRegen()
        {
            bool hasStaminaRegen = DataManager.Server.HasRegenData(MoneyType.Stamina);
            if (hasStaminaRegen)
                UpdateStaminaRegen();
        }

        public void UpdateStaminaRegen()
        {
            var staminaLastRegenDateTime = DataManager.Server.GetLastRegenDateTime(MoneyType.Stamina);
            double offsetSyncTime = (GameTime.ServerUtcNow - staminaLastRegenDateTime).TotalSeconds;
            var staminaAddedTimes = (int)(offsetSyncTime / Constants.STAMINA_REGEN_INTERVAL);
            var totalAddedStamina =  staminaAddedTimes * Constants.STAMINA_ADDED_PER_INTERVAL;
            var currentStaminaResourceValue = DataManager.Server.GetMoneyResource(MoneyType.Stamina);
            if (currentStaminaResourceValue < Constants.STAMINA_REGEN_MAX_AMOUNT)
            {
                var newStaminaResourceValue = currentStaminaResourceValue + totalAddedStamina > Constants.STAMINA_REGEN_MAX_AMOUNT
                                            ? Constants.STAMINA_REGEN_MAX_AMOUNT
                                            : currentStaminaResourceValue + totalAddedStamina;
                DataManager.Server.SetMoneyResource(MoneyType.Stamina, newStaminaResourceValue);
            }

            offsetSyncTime -= staminaAddedTimes * Constants.STAMINA_REGEN_INTERVAL;
            StartRegenStamina(offsetSyncTime);
        }

        private void StartRegenStamina(double offsetSyncTime)
        {
            _staminaRegencancellationTokenSource?.Cancel();
            _staminaRegencancellationTokenSource = new CancellationTokenSource();
            RunRegenStaminaAsync(offsetSyncTime).Forget();
        }

        private async UniTask RunRegenStaminaAsync(double offsetSyncTime)
        {
            if (offsetSyncTime > 0)
            {
                for (int i = 0; i < offsetSyncTime; i++)
                {
                    Messenger.Publisher().Publish(new RegenMoneyCountedMessage(MoneyType.Stamina, (long)(offsetSyncTime - i)));
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _staminaRegencancellationTokenSource.Token, ignoreTimeScale: true);
                }
            }

            while (true)
            {
                var currentStaminaResourceValue = DataManager.Server.GetMoneyResource(MoneyType.Stamina);
                if (currentStaminaResourceValue < Constants.STAMINA_REGEN_MAX_AMOUNT)
                {
                    var newStaminaResourceValue = currentStaminaResourceValue + Constants.STAMINA_ADDED_PER_INTERVAL >= Constants.STAMINA_REGEN_MAX_AMOUNT
                                                ? Constants.STAMINA_REGEN_MAX_AMOUNT
                                                : currentStaminaResourceValue + Constants.STAMINA_ADDED_PER_INTERVAL;
                    DataManager.Server.SetMoneyResource(MoneyType.Stamina, newStaminaResourceValue);
                }

                for (int i = 0; i < Constants.STAMINA_REGEN_INTERVAL; i++)
                {
                    Messenger.Publisher().Publish(new RegenMoneyCountedMessage(MoneyType.Stamina, Constants.STAMINA_REGEN_INTERVAL - i));
                    await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _staminaRegencancellationTokenSource.Token, ignoreTimeScale: true);
                }
            }
        }

        private async UniTaskVoid TrackingStartStage()
        {
            int stageId = DataDispatcher.Instance.StageId;
            string battleId = DataDispatcher.Instance.BattleId;
            string heroId = Constants.HERO_ID.ToString();
            string petId = "";
            var equipmentsDictionary = DataDispatcher.Instance.SelectedEquipments;
            string weaponId = equipmentsDictionary.ContainsKey(EquipmentType.Weapon) ? equipmentsDictionary[EquipmentType.Weapon].EquipmentId.ToString() : "";
            string equipmentsId = "";
            foreach (var entry in equipmentsDictionary)
            {
                if (entry.Key != EquipmentType.Weapon)
                {
                    var addedEquipmentId = entry.Value.EquipmentId.ToString();
                    equipmentsId += addedEquipmentId + "/";
                }
            }

            var heroConfig = await DataManager.Config.LoadHeroInfo(default);
            var heroConfigItem = heroConfig.items.FirstOrDefault(x => x.id == Constants.HERO_ID);
            var heroLevel = DataDispatcher.Instance.HeroLevel;
            var heroLevelConfigItem = heroConfigItem.levels.FirstOrDefault(x => x.level == heroLevel);
            var heroStats = await DataDispatcher.Instance.GetHeroStatsInfo(heroLevelConfigItem.CharacterLevelStats);

            FirebaseManager.Instance.TrackStageStart(battleId,
                                                     heroId,
                                                     weaponId,
                                                     equipmentsId,
                                                     petId,
                                                     stageId,
                                                     (int)heroStats.GetStatTotalValue(StatType.Health),
                                                     (int)heroStats.GetStatTotalValue(StatType.AttackDamage));
        }

        #endregion Class Methods
    }
}