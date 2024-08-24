using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Runtime.Message;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Manager.Data;
using Runtime.Gameplay.Manager;
using Runtime.Manager;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Tutorial
{
    public class GameplayGuideTutorialGameplayManager : GameplayManager
    {
        #region Members

        private const uint HERO_WAVE_INDEX = 0;
        private const uint FIRST_ENEMY_WAVE_INDEX = 1;
        private const uint SECOND_ENEMY_WAVE_INDEX = 2;
        private const uint FIRST_ENEMIES_WAVE_INDEX = 3;
        private const uint SECOND_ENEMIES_WAVE_INDEX = 4;

        [SerializeField]
        private float _respawnHeroDelay = 1.0f;
        [SerializeField]
        private float _afterRespawnHeroDelay = 1.0f;

        [Header("EVENTS")]
        [SerializeField]
        private UnityEvent _stageMapCreatedEvent;
        [SerializeField]
        private UnityEvent _heroDiedEvent;
        [SerializeField]
        private UnityEvent _heroRevivedEvent;
        [SerializeField]
        private UnityEvent _firstEnemyKilledEvent;
        [SerializeField]
        private UnityEvent _secondEnemyKilledEvent;
        [SerializeField]
        private UnityEvent _firstEnemiesWaveKilledEvent;
        [SerializeField]
        private UnityEvent _secondEnemiesWaveKilledEvent;
        private GameplayGuideTutorialEntitiesManager _gameplayGuideTutorialEntitiesManager;

        #endregion Members

        #region Properties

        protected override uint StartWaveIndex => HERO_WAVE_INDEX;

        #endregion Properties

        #region API Methods

        protected override void Start()
        {
            base.Start();
            _gameplayGuideTutorialEntitiesManager = GameplayGuideTutorialEntitiesManager.Instance as GameplayGuideTutorialEntitiesManager;
        }

        #endregion API Methods

        #region Class Methods

        public void CreateHero()
        {
            CurrentWaveIndex = HERO_WAVE_INDEX;
            StartWave();
        }

        public void CreateFirstEnemy()
        {
            CurrentWaveIndex = FIRST_ENEMY_WAVE_INDEX;
            StartWave();
        }

        public void CreateSecondEnemy()
        {
            CurrentWaveIndex = SECOND_ENEMY_WAVE_INDEX;
            StartWave();
        }

        public void CreateFirstEnemiesWave()
        {
            CurrentWaveIndex = FIRST_ENEMIES_WAVE_INDEX;
            StartWave();
        }

        public void CreateSecondEnemiesWave()
        {
            CurrentWaveIndex = SECOND_ENEMIES_WAVE_INDEX;
            StartWave();
        }

        public void StopGameFlow()
            => GameManager.Instance.StopGameFlow(GameFlowTimeControllerType.TutorialGameplay);

        public void ContinueGameFlow()
            => GameManager.Instance.ContinueGameFlow(GameFlowTimeControllerType.TutorialGameplay);

        public override void StartGameplay()
        {
            base.StartGameplay();
            _stageMapCreatedEvent?.Invoke();
        }

        protected override void FinishWave(bool isClearWave)
        {
            CheckWave();
            if (CurrentWaveIndex == maxWaveIndex)
                HandleWinStage();
        }

        private void CheckWave()
        {
            switch (CurrentWaveIndex)
            {
                case FIRST_ENEMY_WAVE_INDEX:
                    _firstEnemyKilledEvent?.Invoke();
                    break;

                case SECOND_ENEMY_WAVE_INDEX:
                    _secondEnemyKilledEvent?.Invoke();
                    break;

                case FIRST_ENEMIES_WAVE_INDEX:
                    _firstEnemiesWaveKilledEvent?.Invoke();
                    break;

                case SECOND_ENEMIES_WAVE_INDEX:
                    _secondEnemiesWaveKilledEvent?.Invoke();
                    break;
            }
        }

        public override void KillHero()
            => StartCoroutine(HandleHeroKilled());

        private IEnumerator HandleHeroKilled()
        {
            _heroDiedEvent?.Invoke();
            mapLoader.TerminateSpawnStageWave();
            _gameplayGuideTutorialEntitiesManager.ClearEnemies();
            yield return new WaitForSecondsRealtime(_respawnHeroDelay);
            CreateHero();
            yield return new WaitForSecondsRealtime(_afterRespawnHeroDelay);
            _heroRevivedEvent?.Invoke();
        }

        protected override async UniTask WinGame(ResourceData[] mergedRewards, ResourceData[] droppableRewards)
        {
            DataManager.Local.SetSelectStage(Constants.MIN_BATTLE_SCENE_ID);
            var stageId = Constants.MIN_BATTLE_SCENE_ID;
            var previousHeroLevel = DataDispatcher.Instance.PreviousHeroLevel;
            var heroLevel = DataDispatcher.Instance.HeroLevel;
            var currentExp = DataDispatcher.Instance.HeroExp;
            var heroNextLevelRequiredExp = DataDispatcher.Instance.HeroNextLevelRequiredExp;

            await DataManager.Config.LoadStageInfo();
            var stageInfo = DataManager.Config.GetStageData(stageId);
            var nextStageInfo = DataManager.Config.GetNextStageData(stageId);

            var questsDataTuple = new List<Tuple<bool, string>>();
            foreach (var quest in Quests)
                questsDataTuple.Add(new Tuple<bool, string>(quest.HasCompleted, quest.Info));

            var victoryData = new VictoryData
            (
                stageFinishedWaveIndex: maxWaveIndex,
                countTime: CurrentGameplayTime,
                previousHeroLevel: previousHeroLevel,
                heroLevel: heroLevel,
                heroCurrentExp: currentExp,
                heroMaxExp: heroNextLevelRequiredExp,
                questsDataTuple: questsDataTuple,
                receivedRewards: mergedRewards,
                droppableRewards: droppableRewards,
                canReplayStage: false,
                stageInfoConfig: stageInfo,
                nextStageInfoConfig: nextStageInfo
            );

            Messenger.Publisher().Publish(new GameWinMessage(victoryData));
        }

        #endregion Class Methods
    }
}