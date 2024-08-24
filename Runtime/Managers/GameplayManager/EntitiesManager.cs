using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using Runtime.UI;
using Runtime.Message;
using Runtime.Core.Singleton;
using Runtime.Gameplay.EntitySystem;
using Runtime.Definition;
using Runtime.Manager.Data;
using Runtime.Server.Models;
using Runtime.ConfigModel;
using Runtime.Extensions;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public class EntitiesManager : MonoSingleton<EntitiesManager>
    {
        #region Members

        protected uint entityUId;
        protected int defeatedEnemiesCount;
        protected GameObject heroTomb;
        protected int currentWarningSpawnedEnemyCount;
        protected int currentSpawningEnemyCount;
        protected List<uint> currentActionContainEnemySpawnedWaveIndexes;

        #endregion Members

        #region Properties

        public int DefeatedEnemiesCount => defeatedEnemiesCount;
        public HeroModel HeroModel { get; protected set; }
        public List<CharacterModel> EnemyModels { get; protected set; }
        public bool HaveNoEnemiesLeft => EnemyModels.Count <= 0 &&
                                         currentWarningSpawnedEnemyCount <= 0 &&
                                         currentSpawningEnemyCount <= 0 &&
                                         currentActionContainEnemySpawnedWaveIndexes.Count <= 0;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            defeatedEnemiesCount = 0;
            entityUId = 0;
            currentSpawningEnemyCount = 0;
            currentWarningSpawnedEnemyCount = 0;
            currentActionContainEnemySpawnedWaveIndexes = new List<uint>();
            HeroModel = null;
            EnemyModels = new List<CharacterModel>();
        }

        #endregion API Methods

        #region Class Methods

        public virtual async UniTask CreateEntitiesAsync(uint spawnedWaveIndex, Vector3 spawnPosition, bool displayWarning,
                                                         CancellationToken cancellationToken, SpawnedEntityInfo spawnedEntityInfo)
        {
            for (int i = 0; i < spawnedEntityInfo.entityNumber; i++)
            {
                await CreateEntityWithWarning(spawnedWaveIndex, spawnedEntityInfo, spawnPosition, displayWarning, cancellationToken);
                await UniTask.Yield(cancellationToken);
            }
        }

        public virtual async UniTask CreateEntitiesAsync(uint spawnedWaveIndex, Vector3 spawnedCenterPosition, float spawnedCenterDistancedOffset,
                                                         bool displayWarning, CancellationToken cancellationToken, params SpawnedEntityInfo[] spawnedEntitiesInfo)
        {
            var newEntityModels = new List<EntityModel>();
            var numberOfDirectionsAwayCenterPosition = 16;
            var numberOfSpawnedEntities = spawnedEntitiesInfo.Sum(x => x.entityNumber);
            var validPositions = new List<Vector2>();
            var spreadNodesCount = Mathf.FloorToInt(numberOfSpawnedEntities / numberOfDirectionsAwayCenterPosition);
            var checkedPositions = MapManager.Instance.GetWalkablePositionsAroundPosition(spawnedCenterPosition, spawnedCenterDistancedOffset, spreadNodesCount, numberOfDirectionsAwayCenterPosition);
            var overlapCircleCheckRadius = MapManager.Instance.SlotSize + MapManager.Instance.SlotHalfSize;

            foreach (var checkedPosition in checkedPositions)
            {
                var collider = Physics2D.OverlapCircle(checkedPosition, overlapCircleCheckRadius, Layers.OBSTACLE_LAYER_MASK);
                if (collider == null)
                    validPositions.Add(checkedPosition);
            }

            if (validPositions.Count > 0)
            {
                validPositions.Shuffle();
                var index = 0;

                foreach (var spawnedEntityInfo in spawnedEntitiesInfo)
                {
                    for (int i = 0; i < spawnedEntityInfo.entityNumber; i++)
                    {
                        if (index >= validPositions.Count)
                        {
                            validPositions.Shuffle();
                            index = 0;
                        }

                        var spawnPosition = validPositions[index++];
                        await CreateEntityWithWarning(spawnedWaveIndex, spawnedEntityInfo, spawnPosition, displayWarning, cancellationToken);
                        await UniTask.Yield(cancellationToken);
                    }
                }
            }
            else
            {
                foreach (var spawnedEntityInfo in spawnedEntitiesInfo)
                {
                    for (int i = 0; i < spawnedEntityInfo.entityNumber; i++)
                    {
                        var spawnPosition = spawnedCenterPosition;
                        await CreateEntityWithWarning(spawnedWaveIndex, spawnedEntityInfo, spawnPosition, displayWarning, cancellationToken);
                        await UniTask.Yield(cancellationToken);
                    }
                }
            }
        }

        public virtual async UniTask<GameObject> CreateEntityAsync(uint spawnedWaveIndex, string entityId, uint entityLevel, EntityType entityType,
                                                                   Vector2 spawnPosition, CancellationToken cancellationToken = default)
        {
            currentSpawningEnemyCount++;

            GameObject go = null;
            switch (entityType)
            {
                case EntityType.Hero:
                    go = await CreateHeroAsync(spawnedWaveIndex, uint.Parse(entityId), entityLevel, spawnPosition, cancellationToken);
                    break;
                case EntityType.Zombie:
                    go = await CreateZombieAsync(spawnedWaveIndex, uint.Parse(entityId), entityLevel, spawnPosition, cancellationToken);
                    break;
                case EntityType.Boss:
                    go = await CreateBossAsync(spawnedWaveIndex, uint.Parse(entityId), entityLevel, spawnPosition, cancellationToken);
                    break;
                case EntityType.Object:
                    go = await CreateObjectAsync(spawnedWaveIndex, uint.Parse(entityId), entityLevel, spawnPosition, cancellationToken);
                    break;
                case EntityType.Trap:
                    go = await CreateTrapAsync(spawnedWaveIndex, uint.Parse(entityId), entityLevel, spawnPosition, cancellationToken);
                    break;
                case EntityType.Decoration:
                    go = await CreateDecoration(uint.Parse(entityId), spawnPosition, cancellationToken);
                    break;
                default:
                    break;
            }

            currentSpawningEnemyCount--;
            return go;
        }

        public virtual async UniTask<GameObject> LoadDataNotCreateEntityAsync(uint spawnedWaveIndex, GameObject entityGameObject, EntityType entityType,
                                                                              uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            switch (entityType)
            {
                case EntityType.Hero:
                    return await LoadDataNotCreateHeroAsync(entityGameObject, spawnedWaveIndex, entityId, entityLevel, cancellationToken);

                case EntityType.Zombie:
                    return await LoadDataNotCreateZombieAsync(entityGameObject, spawnedWaveIndex, entityId, entityLevel, cancellationToken);

                case EntityType.Boss:
                    return await LoadDataNotCreateBossAsync(entityGameObject, spawnedWaveIndex, entityId, entityLevel, cancellationToken);

                case EntityType.Object:
                    return await LoadDataNotCreateObjectAsync(entityGameObject, spawnedWaveIndex, entityId, entityLevel, cancellationToken);

                case EntityType.Trap:
                    return await LoadDataNotCreateTrapAsync(entityGameObject, spawnedWaveIndex, entityId, entityLevel, cancellationToken);

                default:
                    return null;
            }
        }

        public virtual async UniTask<EntityModel> CreateTransformedBossAsync(uint spawnedWaveIndex, string bossId, string transformedBossId, uint entityLevel, Vector2 spawnPosition,
                                                                             BossTransformationDataConfig bossTransformationDataConfig, CancellationToken cancellationToken)
        {
            var transformBossData = await GameplayDataManager.Instance.GetTransformedBossDataAsync(uint.Parse(bossId), entityLevel, bossTransformationDataConfig, cancellationToken);
            var transformedBossModel = new TransformedBossModel(spawnedWaveIndex, entityUId++, uint.Parse(transformedBossId), transformBossData.Item1);
            var transformedBossGameObject = await PoolManager.Instance.Get(transformedBossId.ToString(), cancellationToken: cancellationToken);
            transformedBossGameObject.GetComponent<IEntity>().Build(transformedBossModel, spawnPosition);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(transformedBossModel, transformedBossGameObject.transform));
            CreateEntitySpawnVfxAsync(transformedBossModel.EntityType, spawnPosition, cancellationToken).Forget();
            EnemyModels.Add(transformedBossModel);
            return transformedBossModel;
        }

        public virtual async UniTask CreateParasiteAsync(uint spawnedWaveIndex, string parasiteId, Vector2 spawnPosition,
                                                         CharacterModel hostModel, CancellationToken cancellationToken = default)
        {
            var parasiteModel = new ParasiteModel(spawnedWaveIndex, entityUId++, uint.Parse(parasiteId), hostModel);
            var parasiteGameObject = await PoolManager.Instance.Get(parasiteId, cancellationToken: cancellationToken);
            parasiteGameObject.GetComponent<IEntity>().Build(parasiteModel, spawnPosition);
        }

        public virtual async UniTask<GameObject> CreateProjectileAsync(string projectileId, CharacterModel creatorModel, Vector2 spawnPosition, CancellationToken cancellationToken = default)
        {
            var projectileGameObject = await PoolManager.Instance.Get(projectileId, cancellationToken);
            await projectileGameObject.GetComponent<IProjectile>().BuildAsync(creatorModel, spawnPosition);
            return projectileGameObject;
        }

        public virtual async UniTask<GameObject> CreateDamageAreaAsync(string damageAreaId, CharacterModel creatorModel, Vector2 spawnPosition,
                                                                       DamageAreaData damageAreaData, CancellationToken cancellationToken = default)
        {
            var damageAreaGameObject = await PoolManager.Instance.Get(damageAreaId, cancellationToken);
            await damageAreaGameObject.GetComponent<DamageArea>().BuildAsync(creatorModel, spawnPosition, damageAreaData);
            return damageAreaGameObject;
        }

        public virtual async UniTask<HandleCharacterDiedResultType> HandleCharacterDied(CharacterDiedMessage characterDiedMessage)
        {
            if (characterDiedMessage.IsEnemyDied)
            {
                defeatedEnemiesCount++;
                var enemyModel = characterDiedMessage.CharacterModel;

                if (characterDiedMessage.DeathDataIdentity.deathDataType != DeathDataType.None)
                    await ExecuteDeathStrategyAsync(enemyModel, characterDiedMessage.DeathDataIdentity, this.GetCancellationTokenOnDestroy());

                if (characterDiedMessage.DeathDataIdentity.deathDataType != DeathDataType.BossTransform)
                    CreateEntityDestroyVfxAsync(characterDiedMessage.CharacterModel.EntityType, characterDiedMessage.CharacterModel.Position, this.GetCancellationTokenOnDestroy()).Forget();

                EnemyModels.Remove(enemyModel);

                if (HaveNoEnemiesLeft)
                {
                    if (!characterDiedMessage.DeathDataIdentity.deathDataType.IsSpawnedEnemy())
                        return HandleCharacterDiedResultType.ClearWave;
                }
                else
                {
                    if (!characterDiedMessage.DeathDataIdentity.deathDataType.IsSpawnedEnemy())
                        return HandleCharacterDiedResultType.EnemyDiedNoSpawn;
                }
            }
            else if (characterDiedMessage.IsHeroDied)
            {
                CreateHeroTombAsync(characterDiedMessage.CharacterModel.Position, this.GetCancellationTokenOnDestroy()).Forget();
                return HandleCharacterDiedResultType.HeroDied;
            }

            return HandleCharacterDiedResultType.None;
        }

        public virtual async UniTaskVoid CreateDroppableRewardsAsync(EntitySpawnRewardData[] rewardsData, Vector2 droppedPosition)
        {
            var dropItemsNumber = 5;
            var characters = GetEntitiesOfType<Character>();
            if (characters != null && characters.Length > 0)
            {
                var hero = characters.FirstOrDefault(x => x.EntityUId == HeroModel.EntityUId);
                if (hero)
                {
                    var heroTransform = hero.transform;
                    foreach (var rewardData in rewardsData)
                    {
                        var remainValue = rewardData.spawnRewardNumber;
                        long foreachItemValue = rewardData.spawnRewardNumber / dropItemsNumber;
                        var sprite = await GetDropItemSprite(rewardData.spawnRewardType, rewardData.spawnRewardId, this.GetCancellationTokenOnDestroy());
                        for (int i = 0; i < dropItemsNumber; i++)
                        {
                            var dropItem = await PoolManager.Instance.Get(Constants.DROP_ITEM_PREFAB, this.GetCancellationTokenOnDestroy());
                            if (i == dropItemsNumber - 1)
                                dropItem.GetComponent<DropItem>().Init(remainValue, droppedPosition, heroTransform, sprite);
                            else
                                dropItem.GetComponent<DropItem>().Init(foreachItemValue, droppedPosition, heroTransform, sprite);
                            remainValue -= foreachItemValue;
                        }
                    }
                }
            }
        }

        public async UniTask<GameObject> CreateSpawnWarningVFXAsync(EntityType entityType, float displayTime, Vector2 spawnPosition, CancellationToken cancellationToken)
        {
            currentWarningSpawnedEnemyCount++;
            float vfxScaleValue = entityType == EntityType.Boss ? Constants.BOSS_SPAWN_VFX_SCALE_VALUE : Constants.CHARACTER_SPAWN_VFX_SCALE_VALUE;
            var warningVFX = await PoolManager.Instance.Get(Constants.SPAWN_ENEMY_VFX_NAME, cancellationToken: cancellationToken);
            warningVFX.transform.position = spawnPosition;
            warningVFX.transform.localScale = new Vector2(vfxScaleValue, vfxScaleValue);
            await UniTask.Delay(TimeSpan.FromSeconds(displayTime), cancellationToken: cancellationToken);
            return warningVFX;
        }

        public void RemoveSpawnVFX(GameObject spawnVFX)
        {
            currentWarningSpawnedEnemyCount--;
            PoolManager.Instance.Remove(spawnVFX);
        }

        public void AddActionContainEnemySpawnedWaveIndex(uint spawnedWaveIndex)
            => currentActionContainEnemySpawnedWaveIndexes.Add(spawnedWaveIndex);

        public void RemoveActionContainEnemySpawnedWaveIndex(uint spawnedWaveIndex)
            => currentActionContainEnemySpawnedWaveIndexes.Remove(spawnedWaveIndex);

        public T[] GetEntitiesOfType<T>() where T : UnityEngine.Object
        {
            var objects = FindObjectsOfType<T>();
            return objects;
        }

        protected virtual async UniTask<GameObject> CreateHeroAsync(uint spawnedWaveIndex, uint entityId, uint entityLevel, Vector2 spawnPosition, CancellationToken cancellationToken)
        {
            if (heroTomb)
                PoolManager.Instance.Remove(heroTomb);

            var heroData = await GameplayDataManager.Instance.GetHeroDataAsync(entityId, cancellationToken);
            var heroModel = new HeroModel(spawnedWaveIndex, entityUId++, entityId, heroData.Item1, heroData.Item2);

            var heroGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
            heroGameObject.GetComponent<IEntity>().Build(heroModel, spawnPosition);
            await DataDispatcher.Instance.InitEquipmentSystems(heroModel);
            Messenger.Publisher().Publish(new HeroSpawnedMessage(heroModel, heroGameObject.transform));
            CreateEntitySpawnVfxAsync(heroModel.EntityType, spawnPosition, cancellationToken).Forget();
            HeroModel = heroModel;
            return heroGameObject;
        }

        protected virtual async UniTask<GameObject> LoadDataNotCreateHeroAsync(GameObject entityGameObject, uint spawnedWaveIndex, uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            var heroData = await GameplayDataManager.Instance.GetHeroDataAsync(entityId, cancellationToken);
            var heroModel = new HeroModel(spawnedWaveIndex, entityUId++, entityId, heroData.Item1, heroData.Item2);
            await DataDispatcher.Instance.InitEquipmentSystems(heroModel);
            entityGameObject.SetActive(true);
            entityGameObject.GetComponent<IEntity>().Build(heroModel, entityGameObject.transform.position);
            Messenger.Publisher().Publish(new HeroSpawnedMessage(heroModel, entityGameObject.transform));
            HeroModel = heroModel;
            return entityGameObject;
        }

        protected virtual async UniTask<GameObject> CreateZombieAsync(uint spawnedWaveIndex, uint entityId, uint entityLevel, Vector2 spawnPosition, CancellationToken cancellationToken)
        {
            var zombieData = await GameplayDataManager.Instance.GetZombieDataAsync(entityId, entityLevel, cancellationToken);
            var zombieModel = new ZombieModel(spawnedWaveIndex, entityUId++, entityId, zombieData.Item1);
            var zombieGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
            zombieGameObject.GetComponent<IEntity>().Build(zombieModel, spawnPosition);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(zombieModel, zombieGameObject.transform));
            CreateEntitySpawnVfxAsync(zombieModel.EntityType, spawnPosition, cancellationToken).Forget();
            EnemyModels.Add(zombieModel);
            return zombieGameObject;
        }

        protected virtual async UniTask<GameObject> LoadDataNotCreateZombieAsync(GameObject entityGameObject, uint spawnedWaveIndex, uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            var zombieData = await GameplayDataManager.Instance.GetZombieDataAsync(entityId, entityLevel, cancellationToken);
            var zombieModel = new ZombieModel(spawnedWaveIndex, entityUId++, entityId, zombieData.Item1);
            entityGameObject.SetActive(true);
            entityGameObject.GetComponent<IEntity>().Build(zombieModel, entityGameObject.transform.position);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(zombieModel, entityGameObject.transform));
            EnemyModels.Add(zombieModel);
            return entityGameObject;
        }

        protected virtual async UniTask<GameObject> CreateBossAsync(uint spawnedWaveIndex, uint entityId, uint entityLevel, Vector2 spawnPosition, CancellationToken cancellationToken)
        {
            var bossData = await GameplayDataManager.Instance.GetBossDataAsync(entityId, entityLevel, cancellationToken);
            var bossModel = new BossModel(spawnedWaveIndex, entityUId++, entityId, bossData.Item1);
            var bossGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
            bossGameObject.GetComponent<IEntity>().Build(bossModel, spawnPosition);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(bossModel, bossGameObject.transform));
            CreateEntitySpawnVfxAsync(bossModel.EntityType, spawnPosition, cancellationToken).Forget();
            EnemyModels.Add(bossModel);
            return bossGameObject;
        }

        protected virtual async UniTask<GameObject> LoadDataNotCreateBossAsync(GameObject entityGameObject, uint spawnedWaveIndex, uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            var bossData = await GameplayDataManager.Instance.GetBossDataAsync(entityId, entityLevel, cancellationToken);
            var bossModel = new BossModel(spawnedWaveIndex, entityUId++, entityId, bossData.Item1);
            entityGameObject.SetActive(true);
            entityGameObject.GetComponent<IEntity>().Build(bossModel, entityGameObject.transform.position);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(bossModel, entityGameObject.transform));
            EnemyModels.Add(bossModel);
            return entityGameObject;
        }

        protected virtual async UniTask<GameObject> CreateObjectAsync(uint spawnedWaveIndex, uint entityId, uint entityLevel, Vector3 spawnPosition, CancellationToken cancellationToken)
        {
            var objectData = await GameplayDataManager.Instance.GetObjectDataAsync(entityId, entityLevel);
            var objectModel = new ObjectModel(spawnedWaveIndex, entityUId++, entityId, objectData.Item1);
            var objectGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
            objectGameObject.GetComponent<IEntity>().Build(objectModel, spawnPosition);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(objectModel, objectGameObject.transform));
            return objectGameObject;
        }

        protected virtual async UniTask<GameObject> LoadDataNotCreateObjectAsync(GameObject entityGameObject, uint spawnedWaveIndex, uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            var objectData = await GameplayDataManager.Instance.GetObjectDataAsync(entityId, entityLevel);
            var objectModel = new ObjectModel(spawnedWaveIndex, entityUId++, entityId, objectData.Item1);
            entityGameObject.SetActive(true);
            entityGameObject.GetComponent<IEntity>().Build(objectModel, entityGameObject.transform.position);
            Messenger.Publisher().Publish(new EntitySpawnedMessage(objectModel, entityGameObject.transform));
            return entityGameObject;
        }

        protected virtual async UniTask<GameObject> CreateTrapAsync(uint spawnedWaveIndex, uint entityId, uint entityLevel, Vector3 spawnPosition, CancellationToken cancellationToken)
        {
            var isTrapInterval = entityId.IsTrapInterval();
            if (isTrapInterval)
            {
                var trapData = await GameplayDataManager.Instance.GetTrapIntervalDataAsync(entityId, entityLevel);
                var trapModel = new TrapWithIntervalModel(spawnedWaveIndex, entityUId++, entityId.ToString(), trapData);
                var trapGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
                trapGameObject.GetComponent<IEntity>().Build(trapModel, spawnPosition);
                Messenger.Publisher().Publish(new EntitySpawnedMessage(trapModel, trapGameObject.transform));
                return trapGameObject;
            }
            else
            {
                var trapData = await GameplayDataManager.Instance.GetTrapDataAsync(entityId, entityLevel);
                var trapModel = new TrapModel(spawnedWaveIndex, entityUId++, entityId, trapData.Item1, trapData.Item2);
                var trapGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
                trapGameObject.GetComponent<IEntity>().Build(trapModel, spawnPosition);
                Messenger.Publisher().Publish(new EntitySpawnedMessage(trapModel, trapGameObject.transform));
                return trapGameObject;
            }           
        }

        protected virtual async UniTask<GameObject> LoadDataNotCreateTrapAsync(GameObject entityGameObject, uint spawnedWaveIndex, uint entityId, uint entityLevel, CancellationToken cancellationToken)
        {
            var isTrapInterval = entityId.IsTrapInterval();
            if (isTrapInterval)
            {
                var trapData = await GameplayDataManager.Instance.GetTrapIntervalDataAsync(entityId, entityLevel);
                var trapModel = new TrapWithIntervalModel(spawnedWaveIndex, entityUId++, entityId.ToString(), trapData);
                entityGameObject.SetActive(true);
                entityGameObject.GetComponent<IEntity>().Build(trapModel, entityGameObject.transform.position);
                Messenger.Publisher().Publish(new EntitySpawnedMessage(trapModel, entityGameObject.transform));
            }
            else
            {
                var trapData = await GameplayDataManager.Instance.GetTrapDataAsync(entityId, entityLevel);
                var trapModel = new TrapModel(spawnedWaveIndex, entityUId++, entityId, trapData.Item1, trapData.Item2);
                entityGameObject.SetActive(true);
                entityGameObject.GetComponent<IEntity>().Build(trapModel, entityGameObject.transform.position);
                Messenger.Publisher().Publish(new EntitySpawnedMessage(trapModel, entityGameObject.transform));
            }

            return entityGameObject;
        }

        protected virtual async UniTask<GameObject> CreateDecoration(uint entityId, Vector3 spawnPosition, CancellationToken cancellationToken)
        {
            var decorationGameObject = await PoolManager.Instance.Get(entityId.ToString(), cancellationToken: cancellationToken);
            decorationGameObject.transform.position = spawnPosition;
            return decorationGameObject;
        }

        protected virtual async UniTask CreateHeroTombAsync(Vector2 heroTombPosition, CancellationToken cancellationToken)
        {
            heroTomb = await PoolManager.Instance.Get(Constants.HERO_TOMB_PREFAB, cancellationToken: cancellationToken);
            heroTomb.transform.position = heroTombPosition;
        }

        protected virtual async UniTask CreateEntitySpawnVfxAsync(EntityType entityType, Vector2 vfxPosition, CancellationToken cancellationToken)
        {
            float vfxScaleValue = entityType == EntityType.Boss ? Constants.BOSS_SPAWN_VFX_SCALE_VALUE : Constants.CHARACTER_SPAWN_VFX_SCALE_VALUE;
            var spawnVfx = await PoolManager.Instance.Get(Constants.CHARACTER_SPAWN_VFX_NAME, cancellationToken: cancellationToken);
            spawnVfx.transform.position = vfxPosition;
            spawnVfx.transform.localScale = new Vector2(vfxScaleValue, vfxScaleValue);
        }

        protected virtual async UniTask CreateEntityDestroyVfxAsync(EntityType entityType, Vector2 vfxPosition, CancellationToken cancellationToken)
        {
            if (entityType == EntityType.Zombie)
            {
                var zombieDieVfxName = Constants.ZOMBIE_DIE_VFX_NAME;
                var zombieDieVfx = await PoolManager.Instance.Get(zombieDieVfxName, cancellationToken: cancellationToken);
                zombieDieVfx.transform.position = vfxPosition;
            }
            else if (entityType == EntityType.Boss)
            {
                var bossDieVfxName = Constants.BOSS_DIE_VFX_NAME;
                var bossDieExplosionVfxName = Constants.BOSS_DIE_EXPLOSION_VFX_NAME;
                var bossDieVfx = await PoolManager.Instance.Get(bossDieVfxName, cancellationToken: cancellationToken);
                bossDieVfx.transform.position = vfxPosition;
                await UniTask.Delay(TimeSpan.FromSeconds(Constants.SPAWN_BOSS_DIE_EXPLOSION_DELAY_TIME), ignoreTimeScale: true, cancellationToken: cancellationToken);
                PoolManager.Instance.Remove(bossDieVfx);
                var bossDieExplosionVfx = await PoolManager.Instance.Get(bossDieExplosionVfxName);
                bossDieExplosionVfx.transform.position = vfxPosition;
            }
        }

        protected virtual async UniTask<Sprite> GetDropItemSprite(ResourceType resourceType, int resourceId, CancellationToken cancellationToken)
        {
            Sprite sprite = null;
            if (resourceType == ResourceType.Money)
            {
                sprite = await SpriteAssetsLoader.LoadAsset($"{SpriteAtlasKeys.REWARD_DISPLAY}[{resourceId}]", cancellationToken);
            }
            else if (resourceType == ResourceType.EquipmentFragment)
            {
                var (rarity, type) = resourceId.ExtractEquipmentFragmentId();
                sprite = await SpriteAssetsLoader.LoadAsset($"{SpriteAtlasKeys.INVENTORY_DISPLAY}[{Constants.GetEquipmentTypeId(type)}]", cancellationToken);
            }
            else if (resourceType == ResourceType.Equipment)
            {
                var (rarity, type) = resourceId.ExtractEquipmentFragmentId();
                sprite = await SpriteAssetsLoader.LoadAsset($"{SpriteAtlasKeys.INVENTORY_DISPLAY}[{Constants.GetEquipmentTypeId(type)}]", cancellationToken);
            }
            else if (resourceType == ResourceType.HeroExp)
            {
                sprite = await SpriteAssetsLoader.LoadAsset($"{SpriteAtlasKeys.REWARD_DISPLAY}[exp]", cancellationToken);
            }

            return sprite;
        }

        #region Create Entity With warning

        protected virtual async UniTask CreateEntityWithWarning(uint spawnedWaveIndex, SpawnedEntityInfo spawnedEntityInfo, Vector2 spawnPosition,
                                                                bool displayWarning, CancellationToken cancellationToken)
        {
            if (displayWarning && spawnedEntityInfo.entityType.IsEnemy())
                CreateEntityWithWarning(spawnedWaveIndex,
                                        spawnedEntityInfo,
                                        spawnPosition,
                                        cancellationToken).Forget();
            else
                await CreateEntityAsync(spawnedWaveIndex,
                                        spawnedEntityInfo.entityId,
                                        spawnedEntityInfo.RuntimeEntityLevel,
                                        spawnedEntityInfo.entityType,
                                        spawnPosition,
                                        cancellationToken);
        }

        protected virtual async UniTaskVoid CreateEntityWithWarning(uint spawnedWaveIndex, SpawnedEntityInfo spawnedEntityInfo,
                                                                    Vector2 spawnPosition, CancellationToken cancellationToken)
        {
            var warningVFX = await CreateSpawnWarningVFXAsync(spawnedEntityInfo.entityType, Constants.DISPLAY_SPAWN_WARNING_TIME, spawnPosition, cancellationToken);

            var entityGameObject = await CreateEntityAsync(spawnedWaveIndex,
                                                           spawnedEntityInfo.entityId,
                                                           spawnedEntityInfo.RuntimeEntityLevel,
                                                           spawnedEntityInfo.entityType,
                                                           spawnPosition,
                                                           cancellationToken);
            RemoveSpawnVFX(warningVFX);
            entityGameObject.transform.SetParent(transform);
        }

        protected virtual async UniTask ExecuteDeathStrategyAsync(EntityModel deathEntityModel, DeathDataIdentity deathDataIdentity, CancellationToken cancellationToken)
        {
            var deathDataConfigItem = await GameplayDataManager.Instance.GetDeathDataConfigItem(deathDataIdentity.deathDataType, deathDataIdentity.deathDataId, cancellationToken);
            var deathStrategy = DeathStrategyFactory.GetDeathStrategy(deathDataIdentity.deathDataType);
            await deathStrategy.Execute(deathEntityModel, deathDataConfigItem, cancellationToken);
        }

        #endregion

        #endregion Class Methods
    }

    public enum HandleCharacterDiedResultType
    {
        None,
        ClearWave,
        HeroDied,
        EnemyDiedNoSpawn,
    }
}