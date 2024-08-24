using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Message;
using Runtime.ConfigModel;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;
using Runtime.Definition;
using Runtime.Manager.Data;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public sealed class MapLoader : MonoBehaviour, IDisposable
    {
        #region Members

        [ReadOnly]
        [SerializeField]
        private MapSpawnPoint[] _spawnPoints;
        private MapEditorEntity[] _mapEditorEntities;

        private EntityModel _heroModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private CancellationTokenSource _spawnWaveCancellationTokenSource;
        private bool _finishedSpawnInConfig = false;
        private bool _finishedSpawnInStageData = false;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _mapEditorEntities = FindObjectsOfType<MapEditorEntity>(true);
        }

        private void Start()
        {
            _spawnWaveCancellationTokenSource = new CancellationTokenSource();
            if (_mapEditorEntities != null)
            {
                foreach (var mapEditorEntity in _mapEditorEntities)
                {
                    if (mapEditorEntity.ShouldDisable)
                        mapEditorEntity.gameObject.SetActive(false);
                }
            }
        }

        #endregion API Methods

        #region Class Methods

        public async UniTask SpawnHero(uint configWaveIndex, uint spawnedWaveIndex)
        {
            var entityId = Constants.HERO_ID.ToString();
            var entityType = EntityType.Hero;
            var entityLevel = (uint)DataManager.Server.HeroLevel;
            var spawnedEntityInfo = new SpawnedEntityInfo(entityId, entityType, 1, entityLevel);
            var entityStageLoadConfigItem = new EntityStageLoadConfigItem(spawnedEntityInfo, configWaveIndex, 0.0f, false, 0.0f, 0);
            await SpawnEntityAsync(entityStageLoadConfigItem, spawnedWaveIndex);
        }

        public void SpawnWave(StageLoadConfigItem stageLoadConfigItem, uint configWaveIndex, uint spawnedWaveIndex,
                              uint entityLevelBonusCount, Action finishSpawnCallback)
        {
            _finishedSpawnInStageData = true;
            _finishedSpawnInConfig = true;
            var stageMapEditorEntities = _mapEditorEntities.Where(x => x.WaveActive == configWaveIndex)
                                                           .OrderBy(x => x.DelaySpawnInWave)
                                                           .ToList();

            if (stageMapEditorEntities.Count > 0)
            {
                _finishedSpawnInStageData = false;
                stageMapEditorEntities.ForEach(x => x.SetEntityLevelBonusCount(entityLevelBonusCount));
                SpawnStageWaveByMapEditorDataAsync(stageMapEditorEntities, finishSpawnCallback).Forget();
            }

            if (stageLoadConfigItem.entites != null && stageLoadConfigItem.entites.Length > 0)
            {
                var spawnEntityConfigItems = stageLoadConfigItem.entites.Where(x => x.waveIndex == configWaveIndex).OrderBy(x => x.delaySpawnTime).ToList();
                spawnEntityConfigItems.ForEach(x => x.SetEntityLevelBonusCount(entityLevelBonusCount));
                if (spawnEntityConfigItems.Count > 0)
                {
                    _finishedSpawnInConfig = false;
                    SpawnStageWaveByConfigAsync(spawnEntityConfigItems, spawnedWaveIndex, finishSpawnCallback).Forget();
                }
            }
        }

        public void Dispose()
        {
            _spawnWaveCancellationTokenSource?.Cancel();
            _heroSpawnedRegistry.Dispose();
        }

        public void TerminateSpawnStageWave()
            => _spawnWaveCancellationTokenSource?.Cancel();

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
            => _heroModel = heroSpawnedMessage.HeroModel;

        private async UniTask SpawnStageWaveByConfigAsync(List<EntityStageLoadConfigItem> spawnEntityConfigItems, uint spawnedWaveIndex, Action finishSpawnCallback)
        {
            float countTimeDelay = 0;
            foreach (var spawnEntityConfigItem in spawnEntityConfigItems)
            {
                var delayTime = spawnEntityConfigItem.delaySpawnTime - countTimeDelay;
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: _spawnWaveCancellationTokenSource.Token);
                countTimeDelay += delayTime;
                await SpawnEntityAsync(spawnEntityConfigItem, spawnedWaveIndex);
                await UniTask.Yield(_spawnWaveCancellationTokenSource.Token);
            }
            _finishedSpawnInConfig = true;
            if (_finishedSpawnInStageData && _finishedSpawnInConfig)
                finishSpawnCallback?.Invoke();
        }

        private async UniTask SpawnStageWaveByMapEditorDataAsync(List<MapEditorEntity> stageMapEditorEntities, Action finishSpawnCallback)
        {
            float countTimeDelay = 0;
            foreach (var stageMapEditorEntity in stageMapEditorEntities)
            {
                var delayTime = stageMapEditorEntity.DelaySpawnInWave - countTimeDelay;
                await UniTask.Delay(TimeSpan.FromSeconds(delayTime), cancellationToken: _spawnWaveCancellationTokenSource.Token);
                countTimeDelay += delayTime;
                await SpawnStageWaveByMapEditorDataItemAsync(stageMapEditorEntity);
                await UniTask.Yield(_spawnWaveCancellationTokenSource.Token);
            }

            _finishedSpawnInStageData = true;
            if (_finishedSpawnInStageData && _finishedSpawnInConfig)
                finishSpawnCallback?.Invoke();
        }

        private async UniTask SpawnStageWaveByMapEditorDataItemAsync(MapEditorEntity stageMapEditorEntity)
        {
            if (stageMapEditorEntity.EntityType.IsEnemy())
                SpawnStageWaveByStageDataItemWithWarningAsync(stageMapEditorEntity).Forget();
            await EntitiesManager.Instance.LoadDataNotCreateEntityAsync(stageMapEditorEntity.WaveActive, stageMapEditorEntity.gameObject, stageMapEditorEntity.EntityType,
                                                                        stageMapEditorEntity.EntityId, stageMapEditorEntity.Level, _spawnWaveCancellationTokenSource.Token);
        }

        private async UniTaskVoid SpawnStageWaveByStageDataItemWithWarningAsync(MapEditorEntity stageMapEditorEntity)
        {
            var warningVFX = await EntitiesManager.Instance.CreateSpawnWarningVFXAsync(stageMapEditorEntity.EntityType, Constants.DISPLAY_SPAWN_WARNING_TIME, stageMapEditorEntity.transform.position, _spawnWaveCancellationTokenSource.Token);
            await EntitiesManager.Instance.LoadDataNotCreateEntityAsync(stageMapEditorEntity.WaveActive, stageMapEditorEntity.gameObject, stageMapEditorEntity.EntityType,
                                                                        stageMapEditorEntity.EntityId, stageMapEditorEntity.Level, _spawnWaveCancellationTokenSource.Token);
            EntitiesManager.Instance.RemoveSpawnVFX(warningVFX);
        }

        private async UniTask SpawnEntityAsync(EntityStageLoadConfigItem entityConfig, uint spawnedWaveIndex)
        {
            if (entityConfig.followHero && _heroModel != null)
            {
                var spawnedCenterPosition = _heroModel.Position;
                var spawnedCenterOffsetDistance = entityConfig.distanceFromHero;
                var spawnedEntityInfo = entityConfig.entityConfigItem;
                await EntitiesManager.Instance.CreateEntitiesAsync(spawnedWaveIndex, spawnedCenterPosition, spawnedCenterOffsetDistance, true,
                                                                   _spawnWaveCancellationTokenSource.Token, spawnedEntityInfo);
            }
            else
            {
                var spawnPointIndex = entityConfig.spawnPointIndex;
                if (entityConfig.spawnPointIndex >= _spawnPoints.Count())
                    spawnPointIndex = 0;

                var spawnPosition = _spawnPoints[spawnPointIndex].Position;
                await EntitiesManager.Instance.CreateEntitiesAsync(spawnedWaveIndex, spawnPosition, true,
                                                                   _spawnWaveCancellationTokenSource.Token, entityConfig.entityConfigItem);
            }
        }

        #endregion Class Methods

        #region Editor

        [Button("Load Stage Data")]
        private void LoadStageData()
        {
            _spawnPoints = gameObject.GetComponentsInChildren<MapSpawnPoint>(true);
            _mapEditorEntities = FindObjectsOfType<MapEditorEntity>(true);
        }

        #endregion Editor
    }
}