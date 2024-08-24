using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using Runtime.Server;
using Runtime.ConfigModel;
using Runtime.Manager.Data;
using Runtime.Server.CallbackData;
using Runtime.Definition;
using Runtime.Manager;
using Runtime.Message;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Manager
{
    public class SurvivalGameplayManager : GameplayManager
    {
        #region Members

        private bool _isBestRecord;
        private uint _loopWaveIndex;
        private uint _loopWaveEntityLevelCount;
        private uint _numberOfWaveLoops;
        private float _currentSaveProgressDelay;
        private float _currentSurvivalProgressTime;
        private CancellationTokenSource _cancellationTokenSource;
        private SurvivalEntitiesManager _survivalEntitiesManager;

        #endregion Members

        #region Properties

        protected override uint StartWaveIndex
        {
            get
            {
                var survivalStageId = DataDispatcher.Instance.StageId;
                var waveIndex = DataManager.Server.GetSurvivalProgressWave(survivalStageId);
                return (uint)waveIndex;
            }
        }

        #endregion Properties

        #region API Methods

        protected override void Start()
        {
            base.Start();
            var survivalStageId = DataDispatcher.Instance.StageId;
            _isBestRecord = false;
            _currentSaveProgressDelay = 0.0f;
            _survivalEntitiesManager = SurvivalEntitiesManager.Instance as SurvivalEntitiesManager;
            _currentSurvivalProgressTime = DataManager.Server.GetSurvivalProgressTime(survivalStageId);
        }

        private void OnDestroy()
            => _cancellationTokenSource.Cancel();

        #endregion API Methods

        #region Class Methods

        public override void StartGameplay()
        {
            var survivalStageId = DataDispatcher.Instance.StageId;
            var survivalModeInfo = DataManager.Config.GetSurvivalModeInfo();
            var stageSurvivalModeConfig = survivalModeInfo.GetSurvivalModeConfigItem(survivalStageId);
            _loopWaveIndex = stageSurvivalModeConfig.loopWaveIndex;
            _loopWaveEntityLevelCount = stageSurvivalModeConfig.loopWaveEntityLevelCount;
            _cancellationTokenSource = new CancellationTokenSource();
            RunStartGameplayAsync(_cancellationTokenSource.Token).Forget();
            SaveSurvivalProgressAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTask RunStartGameplayAsync(CancellationToken cancellationToken)
        {
            maxWaveIndex = GameplayDataManager.Instance.StageLoadConfig.waveConfigs.Max(x => x.waveIndex);
            var configWaveIndex = GetConfigWaveIndex();
            await mapLoader.SpawnHero(configWaveIndex, StartWaveIndex);
            base.StartGameplay();
        }

        public override void KillHero()
            => HandleLoseStage();

        protected override void FinishWave(bool isClearWave)
        {
#if TRACKING
            TrackingWaveCompleted(true);
#endif
            CurrentWaveIndex += 1;
            StartWave();
        }

        protected override void StartWave()
        {
            var configWaveIndex = GetConfigWaveIndex();
            hasFinishedSpawnWave = false;
            mapLoader.SpawnWave(GameplayDataManager.Instance.StageLoadConfig, configWaveIndex, CurrentWaveIndex, _loopWaveEntityLevelCount * _numberOfWaveLoops, OnFinishSpawnWave);
            var waveConfig = GameplayDataManager.Instance.StageLoadConfig.waveConfigs.FirstOrDefault(x => x.waveIndex == configWaveIndex);
            waveTimer.Start(waveConfig, onFinish: () => FinishWave(false));
            Messenger.Publisher().Publish(new WaveTimeUpdatedMessage(true, CurrentGameplayTime, (int)CurrentWaveIndex, -1));
        }

        protected override void SendBattleResult()
        {
            hasSuccessfullySentResultToServer = false;
            RegisterGameplayStateChangedAtSendResult();
            var survivalStageId = DataDispatcher.Instance.StageId;
            var battleId = DataDispatcher.Instance.BattleId;
            var receiveRewardsWaveIndex = (int)CurrentWaveIndex;
            var survivalTime = (long)_currentSurvivalProgressTime;
            var survivalStageEndRequestData = new SurvivalStageEndRequestData(survivalStageId, receiveRewardsWaveIndex, battleId, survivalTime, EntitiesManager.Instance.DefeatedEnemiesCount);

            var record = DataManager.Server.GetSurvivalRecordTime(survivalStageId);
            _isBestRecord =  record == null || record.Wave < receiveRewardsWaveIndex || (record.Wave == receiveRewardsWaveIndex && record.SurvivalTime > survivalTime);

            NetworkServer.SendSurvivalStageEndRequest(survivalStageEndRequestData, OnSurvivalStageEndCallback);
        }

        private void OnSurvivalStageEndCallback(SurvivalStageEndCallbackData callbackData)
        {
            hasSuccessfullySentResultToServer = true;
            UnregisterGameplayStateChangedAtSendResult();
            if (callbackData.ResultCode == LogicCode.SUCCESS)
            {
                GameManager.Instance.SetGameStatusType(stageEndType == StageEndType.Win ? GameStatusType.WonStage : GameStatusType.LostStage);
                DataDispatcher.Instance.UpdateStageEnd(callbackData, stageEndType == StageEndType.Win);
                PresentSurvivalEndAsync(callbackData).Forget();
            }
            else ToastController.Instance.Show("Something went wrong when sending the result to the server - Logic code: " + callbackData.ResultCode);
        }

        private async UniTask SaveSurvivalProgressAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                _currentSurvivalProgressTime += Time.deltaTime;
                _currentSaveProgressDelay += Time.deltaTime;
                if (_currentSaveProgressDelay >= Constants.SURVIVAL_SAVE_PROGRESS_DELAY)
                {
                    _currentSaveProgressDelay = 0.0f;
                    SaveSurvivalProgress();
                }
                await UniTask.Yield(cancellationToken);
            }
        }

        private void SaveSurvivalProgress()
        {
            var survivalStageId = DataDispatcher.Instance.StageId;
            var saveWaveIndex = (int)_survivalEntitiesManager.CurrentSavedSpawnedWaveIndex;
            var challengeId = DataManager.Transitioned.BattleId;
            var requestData = new SurvivalSaveProgressRequestData(survivalStageId, saveWaveIndex, challengeId, (long)_currentSurvivalProgressTime);
            NetworkServer.SaveSurvivalProgressRequest(requestData, OnSurvivalSaveProgressCallback);
        }

        private void OnSurvivalSaveProgressCallback(SurvivalSaveProgressCallbackData callbackData)
        {
            if (callbackData.ResultCode == LogicCode.SUCCESS)
            {
                var survivalStageId = DataDispatcher.Instance.StageId;
                var saveWaveIndex = (int)_survivalEntitiesManager.CurrentSavedSpawnedWaveIndex;
                DataManager.Server.SaveSurvivalProgress(survivalStageId, saveWaveIndex, (long)_currentSurvivalProgressTime);
            }
        }

        private async UniTask PresentSurvivalEndAsync(SurvivalStageEndCallbackData callbackData)
        {
            if (slowDownGameMotionOnStageEnd)
            {
                GameManager.Instance.SlowDownGameFlow(GameFlowTimeControllerType.StageResult);
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.STAGE_RESULT_SLOW_DOWN_DURATION), cancellationToken: this.GetCancellationTokenOnDestroy(), ignoreTimeScale: true);
                GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.StageResult);
            }

            if (stageEndType == StageEndType.Win)
                await WinGame(callbackData.StageRewardsResourcesData, null);
            else
                await LoseGame(callbackData.StageRewardsResourcesData, null);
        }

        protected override UniTask WinGame(ResourceData[] mergedRewards, ResourceData[] droppableRewards)
        {
            return UniTask.CompletedTask;
        }

        protected override UniTask LoseGame(ResourceData[] mergedRewards, ResourceData[] droppableRewards)
        {
            var stageInfo = DataDispatcher.Instance.StageInfoData;
            var loseData = new LoseData
            (
                countTime: (long)_currentSurvivalProgressTime,
                receivedRewards: mergedRewards,
                droppableRewards: droppableRewards,
                stageInfo: stageInfo
            );
            Messenger.Publisher().Publish(new SurvivalEndMessage(_isBestRecord, (int)CurrentWaveIndex, loseData));
            return UniTask.CompletedTask;
        }

        private uint GetConfigWaveIndex()
        {
            _numberOfWaveLoops = 0;
            var configWaveIndex = CurrentWaveIndex;
            while (configWaveIndex > maxWaveIndex)
            {
                _numberOfWaveLoops++;
                configWaveIndex = ((configWaveIndex / (maxWaveIndex + 1) - 1) * (maxWaveIndex + 1)) + (configWaveIndex % (maxWaveIndex + 1)) + _loopWaveIndex;
            }
            return configWaveIndex;
        }

        #endregion Class Methods
    }
}