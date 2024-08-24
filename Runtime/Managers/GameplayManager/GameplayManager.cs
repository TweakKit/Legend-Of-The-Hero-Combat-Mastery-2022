using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Server;
using Runtime.Message;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Manager.Data;
using Runtime.Server.CallbackData;
using Runtime.Server.Models;
using Runtime.Core.Singleton;
using Runtime.Gameplay.Quest;
using Runtime.Manager;
using Runtime.SceneLoading;
using Runtime.Gameplay.EntitySystem;
using Runtime.Gameplay.CollisionDetection;
using Runtime.Tracking;
using Runtime.Extensions;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public class GameplayManager : MonoSingleton<GameplayManager>
    {
        #region Members

        protected bool hasRevivedHero;
        protected StageEndType stageEndType;
        protected bool hasFinishedSpawnWave;
        protected uint maxWaveIndex;
        protected bool slowDownGameMotionOnStageEnd;
        protected ResourceData[] droppableRewards;
        protected WaveTimer waveTimer;
        protected MapLoader mapLoader;
        protected QuestManager questManager;
        protected bool isStageEnded;
        protected bool hasSuccessfullySentResultToServer;
        protected bool revivedInCurrentWave;
        protected Registry<GameStateChangedMessage> gameStateChangedAtSendResultRegistry;

        #endregion Members

        #region Properties

        protected virtual uint StartWaveIndex => 0;
        public uint CurrentWaveIndex { get; protected set; }
        public ResourceData[] DroppableRewards => droppableRewards;
        public List<IQuest> Quests => questManager != null ? questManager.Quests : null;
        public int AchievedQuestsCount => questManager != null ? questManager.GetAchievedQuestsCount() : -1;
        public long CurrentGameplayTime => waveTimer.CurrentGameplayTime;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            isStageEnded = false;
            hasRevivedHero = false;
            slowDownGameMotionOnStageEnd = true;
            waveTimer = new WaveTimer();
            SceneManager.RegisterBeforeChangeScene(OnBeforeChangeScene);
        }

        protected virtual void Start()
        {
            CurrentWaveIndex = StartWaveIndex;
            mapLoader = MapManager.Instance.MapLoader;
        }

        protected virtual void OnApplicationQuit()
            => LeaveApplication();

        #endregion API Methods

        #region Class Methods

        public virtual void StartGameplay()
        {
            revivedInCurrentWave = false;
            SetUpQuests();
            waveTimer.SetUp();
            maxWaveIndex = GameplayDataManager.Instance.StageLoadConfig.waveConfigs.Max(x => x.waveIndex);
            Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.StartStage));
            GameManager.Instance.SetGameStatusType(GameStatusType.StartStage);
            StartWave();
        }

        public virtual void GetDroppableRewards(EntitySpawnRewardData[] rewardsData)
        {
            var droppableResourcesData = new List<ResourceData>();
            foreach (var reward in rewardsData)
                droppableResourcesData.Add(new ResourceData(reward.spawnRewardType, reward.spawnRewardId, reward.spawnRewardNumber));

            if (droppableRewards == null)
                droppableRewards = droppableResourcesData.ToArray();
            else
                droppableRewards = ResourceDataExtenstions.MergeRewards(droppableRewards, droppableResourcesData.ToArray());
        }

        public virtual void OnBeforeChangeScene()
        {
            waveTimer.Dispose();
            MapManager.Instance.Dispose();
            MechanicSystemManager.Instance.Dispose();

            var disposableEntities = FindObjectsOfType<Disposable>(true);
            foreach (var disposableEntity in disposableEntities)
                disposableEntity.Dispose();

            CollisionSystem.Instance.Dispose();
        }

        protected virtual void SetUpQuests()
        {
            var stageInfoData = DataDispatcher.Instance.StageInfoData;
            var questIdentities = stageInfoData.GetQuestIdentities();
            if (questIdentities != null && questIdentities.Length > 0)
            {
                questManager = gameObject.GetOrAddComponent<QuestManager>();
                questManager.LoadQuests(stageInfoData.GetQuestIdentities());
            }
        }

        protected virtual void StartWave()
        {
            hasFinishedSpawnWave = false;
            mapLoader.SpawnWave(GameplayDataManager.Instance.StageLoadConfig, CurrentWaveIndex, CurrentWaveIndex, 0, OnFinishSpawnWave);
            var waveConfig = GameplayDataManager.Instance.StageLoadConfig.waveConfigs.FirstOrDefault(x => x.waveIndex == CurrentWaveIndex);
            waveTimer.Start(waveConfig, onFinish: () => FinishWave(false));
            Messenger.Publisher().Publish(new WaveTimeUpdatedMessage(true, CurrentGameplayTime, (int)CurrentWaveIndex, (int)maxWaveIndex));
        }

        protected virtual void OnFinishSpawnWave()
            => hasFinishedSpawnWave = true;

        public virtual void ClearWave()
        {
            if (hasFinishedSpawnWave)
                FinishWave(true);
        }

        protected virtual void FinishWave(bool isClearWave)
        {
#if TRACKING
            TrackingWaveCompleted(true);
#endif
            if (CurrentWaveIndex == maxWaveIndex)
            {
                if (isClearWave)
                {
                    HandleWinStage();
                }
                else
                {
                    var hasNoEnemiesLeft = EntitiesManager.Instance.HaveNoEnemiesLeft;
                    if (hasNoEnemiesLeft)
                        HandleWinStage();
                    else
                        HandleLoseStage();
                }
            }
            else
            {
                CurrentWaveIndex += 1;
                StartWave();
            }
        }

        public virtual void KillHero()
            => RunHeroKilledEventActionAsync().Forget();

        protected virtual async UniTask RunHeroKilledEventActionAsync()
        {
            if (!hasRevivedHero && !isStageEnded)
            {
                GameManager.Instance.SlowDownGameFlow(GameFlowTimeControllerType.StageRevive);
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.STAGE_RESULT_SLOW_DOWN_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy(), ignoreTimeScale: true);
                GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.StageRevive);

                var reviveConfirmDisplayMessage = new ReviveConfirmDisplayMessage(OnCanceledRevive, OnRevivedSuccessfully);
                Messenger.Publisher().Publish(reviveConfirmDisplayMessage);
            }
            else
            {
                HandleLoseStage();
            }
        }

        protected virtual void TrackingWaveCompleted(bool isComplete)
        {
            var waveNumber = CurrentWaveIndex;
            var battleId = DataDispatcher.Instance.BattleId;
            var healthRemain = EntitiesManager.Instance.HeroModel.CurrentHp;
            var healthRatio = EntitiesManager.Instance.HeroModel.CurrentHp / EntitiesManager.Instance.HeroModel.MaxHp;
            var playTime = CurrentGameplayTime;
            var defeatedEnemy = EntitiesManager.Instance.DefeatedEnemiesCount;
            var waveResult = isComplete ? 1 : 0;
            FirebaseManager.Instance.TrackWaveCompleted((int)waveNumber, battleId, (int)healthRemain, healthRatio, playTime, defeatedEnemy, waveResult, revivedInCurrentWave ? 1 : 0);
            revivedInCurrentWave = false;
        }

        protected virtual void OnCanceledRevive()
        {
            slowDownGameMotionOnStageEnd = false;
            HandleLoseStage();
        }

        protected virtual void OnRevivedSuccessfully()
        {
            revivedInCurrentWave = true;
            hasRevivedHero = true;
            slowDownGameMotionOnStageEnd = true;
            var heroModel = EntitiesManager.Instance.HeroModel;
            EntitiesManager.Instance.CreateEntityAsync(heroModel.SpawnedWaveIndex, heroModel.EntityId,
                                                       heroModel.Level, EntityType.Hero, heroModel.Position).Forget();
        }

        protected virtual void HandleWinStage()
        {
            if (isStageEnded)
                return;

            isStageEnded = true;
            waveTimer.Dispose();
            stageEndType = StageEndType.Win;
            Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.WinStage));
            SendBattleResult();
        }

        protected virtual void HandleLoseStage()
        {
            if (isStageEnded)
                return;

#if TRACKING
            TrackingWaveCompleted(false);
#endif

            isStageEnded = true;
            waveTimer.Dispose();
            stageEndType = StageEndType.Lose;
            Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.LoseStage));
            SendBattleResult();
        }

        protected virtual void SendBattleResult()
        {
            hasSuccessfullySentResultToServer = false;
            RegisterGameplayStateChangedAtSendResult();

            var stageId = DataDispatcher.Instance.StageId;
            var battleId = DataDispatcher.Instance.BattleId;

            var stageInfo = DataDispatcher.Instance.StageInfoData;
            var starDictionary = new Dictionary<int, bool>()
            {
                {1, true },
                {2, true },
                {3, true },
            };

            for (int i = 0; i < stageInfo.starQuests.Length; i++)
            {
                var questIdentity = stageInfo.starQuests[i].questIdentity;
                var quest = Quests.FirstOrDefault(x => x.QuestType == questIdentity.questType && x.QuestId == questIdentity.questDataId);
                if (!quest.HasCompleted)
                    starDictionary[stageInfo.starQuests[i].star] = false;
            }

            var droppableResources = droppableRewards == null ? new List<Resource>() : droppableRewards.ToResourceData().ToList();
            var requestData = new CampaignStageEndRequestData(stageId, stageEndType == StageEndType.Win, battleId, CurrentWaveIndex, EntitiesManager.Instance.DefeatedEnemiesCount, starDictionary, droppableResources);
            NetworkServer.SendCampaignStageEndRequest(requestData, OnCampaignStageEndCallback);
        }

        protected virtual void OnCampaignStageEndCallback(CampaignStageEndCallbackData data)
        {
            hasSuccessfullySentResultToServer = true;
            UnregisterGameplayStateChangedAtSendResult();
            if (data.ResultCode == LogicCode.SUCCESS)
            {
                GameManager.Instance.SetGameStatusType(stageEndType == StageEndType.Win ? GameStatusType.WonStage : GameStatusType.LostStage);
                DataDispatcher.Instance.UpdateStageEnd(data, stageEndType == StageEndType.Win);
                PresentEndGameAsync(data).Forget();
            }
            else ToastController.Instance.Show("Something went wrong when sending the result to the server - Logic code: " + data.ResultCode);
        }

        protected virtual async UniTask PresentEndGameAsync(CampaignStageEndCallbackData data)
        {
            if (slowDownGameMotionOnStageEnd)
            {
                GameManager.Instance.SlowDownGameFlow(GameFlowTimeControllerType.StageResult);
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.STAGE_RESULT_SLOW_DOWN_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy(), ignoreTimeScale: true);
                GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.StageResult);
            }

            var mergedRewards = GetRewards(data.StageRewardsResourcesData, data.FirstClearRewardsResourcesData);
            if (stageEndType == StageEndType.Win)
                await WinGame(mergedRewards, data.DroppableRewardsResourcesData);
            else
                await LoseGame(mergedRewards, data.DroppableRewardsResourcesData);
        }

        protected virtual UniTask WinGame(ResourceData[] mergedRewards, ResourceData[] droppableRewards)
        {
            var questsDataTuple = new List<Tuple<bool, string>>();
            foreach (var quest in Quests)
                questsDataTuple.Add(new Tuple<bool, string>(quest.HasCompleted, quest.Info));

            var previousHeroLevel = DataDispatcher.Instance.PreviousHeroLevel;
            var heroLevel = DataDispatcher.Instance.HeroLevel;
            var currentExp = DataDispatcher.Instance.HeroExp;
            var heroNextLevelRequiredExp = DataDispatcher.Instance.HeroNextLevelRequiredExp;

            var victoryData = new VictoryData
            (
                countTime: CurrentGameplayTime,
                stageFinishedWaveIndex: maxWaveIndex,
                previousHeroLevel: previousHeroLevel,
                heroLevel: heroLevel,
                heroCurrentExp: currentExp,
                heroMaxExp: heroNextLevelRequiredExp,
                questsDataTuple: questsDataTuple,
                receivedRewards: mergedRewards,
                droppableRewards: droppableRewards,
                stageInfoConfig: DataDispatcher.Instance.StageInfoData,
                nextStageInfoConfig: DataDispatcher.Instance.NextStageInfoData
            );
            Messenger.Publisher().Publish(new GameWinMessage(victoryData));
            return UniTask.CompletedTask;
        }

        protected virtual UniTask LoseGame(ResourceData[] mergedRewards, ResourceData[] droppableRewards)
        {
            var stageInfo = DataDispatcher.Instance.StageInfoData;
            UserActionManager.Instance.TrackIAPFailStageAction(stageInfo.stageId.ToString());

            var loseData = new LoseData
            (
                countTime: CurrentGameplayTime,
                receivedRewards: mergedRewards,
                droppableRewards: droppableRewards,
                stageInfo: stageInfo
            );
            Messenger.Publisher().Publish(new GameLoseMessage(GameModeType.Campaign, loseData));
            return UniTask.CompletedTask;
        }

        protected virtual ResourceData[] GetRewards(ResourceData[] rewards, ResourceData[] firstClearReceivedRewards)
        {
            var resources = new List<ResourceData>();

            if (firstClearReceivedRewards != null)
                resources.AddRange(firstClearReceivedRewards.ParseFirstClears());

            if (rewards != null)
                resources.AddRange(rewards);

            return resources.ToArray();
        }

        protected virtual void RegisterGameplayStateChangedAtSendResult()
        {
            gameStateChangedAtSendResultRegistry.Dispose();
            gameStateChangedAtSendResultRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChangedAtSendResult);
        }

        protected virtual void UnregisterGameplayStateChangedAtSendResult()
            => gameStateChangedAtSendResultRegistry.Dispose();

        protected virtual void OnGameStateChangedAtSendResult(GameStateChangedMessage message)
        {
            if (message.GameStateType == GameStateEventType.Reconnected)
            {
                if (!hasSuccessfullySentResultToServer)
                    SendBattleResult();
            }
        }

        protected virtual void LeaveApplication()
            => GameManager.Instance.SetGameStatusType(GameStatusType.QuitStage);

        #endregion Class Methods

        #region Owner Classes

        public class WaveTimer : IDisposable
        {
            #region Members

            private WaveStageLoadConfigItem _data;
            private CancellationTokenSource _cancellationTokenSource;
            private Action _onFinish;
            private long _currentGameplayTime;

            #endregion Members

            #region Properties

            public long CurrentGameplayTime => _currentGameplayTime;

            #endregion Properties

            #region Class Methods

            public void SetUp()
                => _currentGameplayTime = 0;

            public void Start(WaveStageLoadConfigItem data, Action onFinish)
            {
                _data = data;
                _onFinish = onFinish;
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                CountTimeAsync(data.IsInfiniteDuration, _data.duration).Forget();
            }

            public void Dispose()
                => _cancellationTokenSource?.Cancel();

            private async UniTask CountTimeAsync(bool isInfiniteDuration, int duration)
            {
                if (isInfiniteDuration)
                {
                    int count = 0;
                    while (true)
                    {
                        count++;
                        _currentGameplayTime++;
                        Messenger.Publisher().Publish(new WaveTimeUpdatedMessage(false, _currentGameplayTime));
                        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false, cancellationToken: _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    for (int i = 0; i < duration; i++)
                    {
                        _currentGameplayTime++;
                        Messenger.Publisher().Publish(new WaveTimeUpdatedMessage(false, _currentGameplayTime));
                        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: false, cancellationToken: _cancellationTokenSource.Token);
                    }
                }

                _onFinish?.Invoke();
            }

            #endregion Class Methods
        }

        #endregion Owner Classes
    }
}