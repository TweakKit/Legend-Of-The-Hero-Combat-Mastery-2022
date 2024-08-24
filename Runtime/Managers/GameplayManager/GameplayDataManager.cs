using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Runtime.Core.Singleton;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Gameplay.EntitySystem;
using Runtime.Manager.Data;
using Runtime.Message;
using Runtime.SceneLoading;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Manager
{
    public class GameplayDataManager : MonoSingleton<GameplayDataManager>
    {
        #region Members

        [SerializeField]
        private bool _loadDataAlongWithLoadScene = true;
        [SerializeField]
        private GameModeType _gameModeType;

        #endregion Members

        #region Properties

        public Tuple<HeroLevelModel, WeaponModel> HeroData { get; private set; }
        public HeroExpConfig HeroExpConfig { get; private set; }
        public EquipmentUnlockedSubStatConfig EquipmentUnlockedSubStatConfig { get; private set; }
        public SkillTreeSecondBranchConfigItem[] SkillTreeSecondBranchConfigItems { get; private set; }
        public SkillDataFactory SkillDataFactory { get; set; }
        public DeathDataFactory DeathDataFactory { get; set; }
        public StageLoadConfigItem StageLoadConfig { get; private set; }
        public StageInfoConfigItem StageInfoConfig { get; private set; }
        public StageInfoConfigItem NextStageInfoConfig { get; private set; }
        public bool IsMaxStage { get; private set; }
        public Dictionary<(QuestType, uint), QuestDataConfigItem> QuestDataConfigItems { get; private set; }
        private Dictionary<uint, BossConfigItem> BossConfigDictionary { get; set; } = new Dictionary<uint, BossConfigItem>();
        private Dictionary<uint, ZombieConfigItem> ZombieConfigDictionary { get; set; } = new Dictionary<uint, ZombieConfigItem>();

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            if (_loadDataAlongWithLoadScene)
            {
                SceneManager.RegisterCompleteTaskBeforeNewSceneAppeared(LoadConfig);
                SceneManager.RegisterBeforeNewSceneAppeared(FinishedLoading);
            }
            else LoadConfig(default).ContinueWith(FinishedLoading).Forget();
        }

        #endregion API Methods

        #region Class Methods

        public async UniTask<Tuple<HeroLevelModel, WeaponModel>> GetHeroDataAsync(uint heroId, CancellationToken cancellationToken)
        {
            if (HeroData == null)
                await LoadHeroDataAsync(cancellationToken);
            return HeroData;
        }

        public async UniTask<Tuple<ZombieLevelModel>> GetZombieDataAsync(uint zombieId, uint level, CancellationToken cancellationToken)
        {
            if (!ZombieConfigDictionary.ContainsKey(zombieId))
            {
                var zombieConfig = await DataManager.Config.LoadZombieConfig(zombieId.ToString(), cancellationToken);
                var configItem = zombieConfig.items.FirstOrDefault(x => x.id == zombieId);
                ZombieConfigDictionary.TryAdd(zombieId, configItem);
            }
            var zombieConfigItem = ZombieConfigDictionary[zombieId];
            level = level < zombieConfigItem.levels.Length ? level : (uint)zombieConfigItem.levels.Length;
            var zombieLevelConfigItem = zombieConfigItem.levels.FirstOrDefault(x => x.level == level);
            var skillIdentity = zombieLevelConfigItem.skillIdentity;
            SkillDataConfigItem skillDataConfigItem = null;
            var skillModels = new List<SkillModel>();

            if (skillIdentity.skillType != SkillType.None)
            {
                skillDataConfigItem = await SkillDataFactory.GetSkillDataConfigItem(skillIdentity.skillType, skillIdentity.skillDataId);
                var skillStatusEffectModelsDictionary = await GetStatusEffectModelsDictionary(skillDataConfigItem.StatusEffectIdentitiesDictionary);
                var skillData = new SkillData(skillDataConfigItem, skillStatusEffectModelsDictionary);
                var skillModel = SkillModelFactory.GetSkillModel(skillIdentity.skillType, skillData);
                skillModels.Add(skillModel);
            }

            var zombieStatsInfo = new ZombieStatsInfo(zombieLevelConfigItem.CharacterLevelStats);
            var zombieLevelModel = new ZombieLevelModel(level,
                                                        zombieLevelConfigItem.detectedPriority,
                                                        zombieStatsInfo,
                                                        skillModels,
                                                        zombieLevelConfigItem.deathDataIdentity);
            return new(zombieLevelModel);
        }

        public async UniTask<Tuple<BossLevelModel>> GetBossDataAsync(uint bossId, uint level, CancellationToken cancellationToken)
        {
            if (!BossConfigDictionary.ContainsKey(bossId))
            {
                var bossConfig = await DataManager.Config.LoadBossConfig(bossId.ToString(), cancellationToken);
                var configItem = bossConfig.items.FirstOrDefault(x => x.id == bossId);
                BossConfigDictionary.TryAdd(bossId, configItem);
            }

            var bossConfigItem = BossConfigDictionary[bossId];
            level = level < bossConfigItem.levels.Length ? level : (uint)bossConfigItem.levels.Length;
            var bossLevelConfigItem = bossConfigItem.levels.FirstOrDefault(x => x.level == level);
            var skillModels = new List<SkillModel>();
            var nextSkillDelays = new List<float>();

            foreach (var skillSequence in bossLevelConfigItem.skillSequences)
            {
                var skillIdentity = skillSequence.skillIdentity;
                var skillDataConfigItem = await SkillDataFactory.GetSkillDataConfigItem(skillIdentity.skillType, skillIdentity.skillDataId);
                var skillStatusEffectModelsDictionary = await GetStatusEffectModelsDictionary(skillDataConfigItem.StatusEffectIdentitiesDictionary);
                var skillData = new SkillData(skillDataConfigItem, skillStatusEffectModelsDictionary);
                var skillModel = SkillModelFactory.GetSkillModel(skillIdentity.skillType, skillData);
                skillModels.Add(skillModel);
                nextSkillDelays.Add(skillSequence.nextSkillDelay);
            }

            var bossStatsInfo = new BossStatsInfo(bossLevelConfigItem.CharacterLevelStats);
            var bossLevelModel = new BossLevelModel(level,
                                                    bossLevelConfigItem.detectedPriority,
                                                    bossStatsInfo,
                                                    skillModels,
                                                    nextSkillDelays,
                                                    bossLevelConfigItem.deathDataIdentity);
            return new(bossLevelModel);
        }

        public async UniTask<Tuple<TransformedBossLevelModel>> GetTransformedBossDataAsync(uint bossId, uint level,
                                                                                           BossTransformationDataConfig bossTransformationDataConfig,
                                                                                           CancellationToken cancellationToken)
        {
            var bossData = await GetBossDataAsync(bossId, level, cancellationToken);
            var entityTransformationProperties = new List<EntityTransformationProperty>();
            var additionalSkillSequence = bossTransformationDataConfig.additionalSkillSequence;
            var skillIdentity = additionalSkillSequence.skillSequence.skillIdentity;
            var nextSkillDelay = additionalSkillSequence.skillSequence.nextSkillDelay;
            var skillDataConfigItem = await SkillDataFactory.GetSkillDataConfigItem(skillIdentity.skillType, skillIdentity.skillDataId);
            var skillStatusEffectModelsDictionary = await GetStatusEffectModelsDictionary(skillDataConfigItem.StatusEffectIdentitiesDictionary);
            var skillData = new SkillData(skillDataConfigItem, skillStatusEffectModelsDictionary);
            var skillModel = SkillModelFactory.GetSkillModel(skillIdentity.skillType, skillData);
            entityTransformationProperties.Add(new AdditionalSkillSequenceEntityTransformationProperty(skillModel,
                                                                                                       nextSkillDelay,
                                                                                                       additionalSkillSequence.useSimultaneouslyWithOthers,
                                                                                                       additionalSkillSequence.simultaneousSkillIndexes));
            var transformedBossLevelModel = new TransformedBossLevelModel(bossData.Item1.Level,
                                                                          bossData.Item1.DetectedPriority,
                                                                          bossData.Item1.CharacterStatsInfo,
                                                                          bossData.Item1.SkillModels,
                                                                          bossData.Item1.NextSkillDelays,
                                                                          entityTransformationProperties);
            return new(transformedBossLevelModel);
        }

        public async UniTask<Tuple<ObjectLevelConfigItem>> GetObjectDataAsync(uint objectId, uint level)
        {
            var objectConfig = await ConfigDataManager.Instance.LoadObjectConfig();
            var objectConfigItem = objectConfig.items.FirstOrDefault(x => x.id == objectId);
            level = level < objectConfigItem.levels.Length ? level : (uint)objectConfigItem.levels.Length;
            var objectConfigLevelItem = objectConfigItem.levels.FirstOrDefault(x => x.level == level);
            return new Tuple<ObjectLevelConfigItem>(objectConfigLevelItem);
        }

        public async UniTask<Tuple<TrapLevelConfigItem, StatusEffectModel[]>> GetTrapDataAsync(uint trapId, uint level)
        {
            var trapConfig = await ConfigDataManager.Instance.LoadTrapConfig();
            var trapConfigItem = trapConfig.items.FirstOrDefault(x => x.id == trapId);
            level = level < trapConfigItem.levels.Length ? level : (uint)trapConfigItem.levels.Length;
            var trapLevelConfigItem = trapConfigItem.levels.FirstOrDefault(x => x.level == level);
            var damageStatusEffectModels = new StatusEffectModel[trapLevelConfigItem.trappedModifierIdentities.Length];
            for (int i = 0; i < trapLevelConfigItem.trappedModifierIdentities.Length; i++)
            {
                var statusEffectDataConfigItem = await DataManager.Config.LoadStatusEffectDataConfigItem(trapLevelConfigItem.trappedModifierIdentities[i].modifierType,
                                                                                                             trapLevelConfigItem.trappedModifierIdentities[i].modifierDataId);
                var statusEffectData = new StatusEffectData(statusEffectDataConfigItem);
                var statusEffectModel = StatusEffectModelFactory.GetStatusEffectModel(trapLevelConfigItem.trappedModifierIdentities[i].modifierType, statusEffectData);
                damageStatusEffectModels[i] = statusEffectModel;
            }
            return new(trapLevelConfigItem, damageStatusEffectModels);
        }

        public async UniTask<TrapWithIntervalLevelConfigItem> GetTrapIntervalDataAsync(uint trapId, uint level)
        {
            var trapConfig = await ConfigDataManager.Instance.LoadTrapIntervalConfig();
            var trapConfigItem = trapConfig.items.FirstOrDefault(x => x.id == trapId);
            level = level < trapConfigItem.levels.Length ? level : (uint)trapConfigItem.levels.Length;
            var trapLevelConfigItem = trapConfigItem.levels.FirstOrDefault(x => x.level == level);
            return trapLevelConfigItem;
        }

        public async UniTask<EquipmentSystemModel> GetEquipmentSystemModel(EquipmentSystemType equipmentSystemType, RarityType rarityType)
        {
            var dataConfig = await GetEquipmentMechanicDataAsync(equipmentSystemType, rarityType);


            var statusEffectIdentitiesDictionary = new Dictionary<string, ModifierIdentity[]>();
            if (dataConfig != null && dataConfig.StatusEffectIdentitiesDictionary != null)
            {
                var statusEffectIdentities = dataConfig.StatusEffectIdentitiesDictionary.ToList();
                foreach (var statusEffectIdentity in statusEffectIdentities)
                    statusEffectIdentitiesDictionary.Add(statusEffectIdentity.Key, statusEffectIdentity.Value);
            }

            var equipmentStatusEffectModelsDictionary = await GetStatusEffectModelsDictionary(statusEffectIdentitiesDictionary);
            var equipmentSystemModel = EquipmentSystemModelFactory.GetEquipmentModel(equipmentSystemType, dataConfig, equipmentStatusEffectModelsDictionary);
            return equipmentSystemModel;
        }

        public async UniTask<SkillTreeSystemModel> GetSkillTreeSystemModel(SkillTreeSystemType skillTreeSystemType, int dataId)
        {
            var dataConfigItem = await GetSkillTreeDataConfigAsync(skillTreeSystemType, dataId);
            var skillTreeSystemModel = SkillTreeSystemModelFactory.GetSkillTreeSystemModel(skillTreeSystemType, dataConfigItem);
            return skillTreeSystemModel;
        }

        public UniTask<DeathDataConfigItem> GetDeathDataConfigItem(DeathDataType deathDataType, int dataId, CancellationToken cancellationToken)
        {
            return DeathDataFactory.GetDeathDataConfigItem(deathDataType, dataId, cancellationToken);
        }

        private async UniTask LoadHeroDataAsync(CancellationToken cancellationToken)
        {
            var heroId = Constants.HERO_ID;
            var heroConfig = await DataManager.Config.LoadHeroInfo(cancellationToken);
            var heroConfigItem = heroConfig.items.FirstOrDefault(x => x.id == heroId);
            var heroLevel = DataDispatcher.Instance.HeroLevel;
            var heroLevelConfigItem = heroConfigItem.levels.FirstOrDefault(x => x.level == heroLevel);
            var equipmentEquip = DataDispatcher.Instance.SelectedEquipments[EquipmentType.Weapon];
            var weaponType = (WeaponType)equipmentEquip.EquipmentId;
            var weaponData = await GetWeaponDataAsync(weaponType, equipmentEquip.RarityType, equipmentEquip.Level);
            var weaponModel = WeaponModelFactory.GetWeaponModel(weaponType, weaponData);
            var heroStatsInfo = await DataDispatcher.Instance.GetHeroStatsInfo(heroLevelConfigItem.CharacterLevelStats);
            var heroLevelModel = new HeroLevelModel((uint)heroLevel,
                                                    heroLevelConfigItem.detectedPriority,
                                                    heroStatsInfo);
            HeroData = new(heroLevelModel, weaponModel);
        }

        private async UniTask<WeaponData> GetWeaponDataAsync(WeaponType weaponType, RarityType weaponEquipmentRarityType, int weaponLevel)
        {
            string weaponDataConfigAssetName = string.Format(AddressableKeys.WEAPON_DATA_CONFIG_ASSET_FORMAT, weaponType);
            var weaponDataConfig = await DataManager.Config.Load<WeaponDataConfig>(weaponDataConfigAssetName);
            var weaponDataConfigItem = weaponDataConfig.GetWeaponDataConfigItem();
            EquipmentMechanicDataConfigItem mechanicDataConfigItem = null;

            var requiredUnlockedSubStat = EquipmentUnlockedSubStatConfig.items.Select(x => x.requiredLevel).ToArray();
            var maxRequiredSatisfiedLevel = -1;
            var indexOfmaxRequiredSatisfiedLevel = 0;

            for (int i = 0; i < requiredUnlockedSubStat.Length; i++)
            {
                var requiredLevel = requiredUnlockedSubStat[i];
                if (requiredLevel <= weaponLevel && requiredLevel >= maxRequiredSatisfiedLevel)
                {
                    maxRequiredSatisfiedLevel = requiredLevel;
                    indexOfmaxRequiredSatisfiedLevel = i;
                }
            }

            var rarityUnlocked = RarityType.Invalid;
            if (indexOfmaxRequiredSatisfiedLevel != -1)
                rarityUnlocked = (RarityType)Mathf.Min(indexOfmaxRequiredSatisfiedLevel + 1, (int)weaponEquipmentRarityType);

            mechanicDataConfigItem = weaponDataConfig.GetEquipmentDataConfigItem(rarityUnlocked);

            var statusEffectIdentitiesDictionary = new Dictionary<string, ModifierIdentity[]>(weaponDataConfigItem.StatusEffectIdentitiesDictionary);
            if (mechanicDataConfigItem != null && mechanicDataConfigItem.StatusEffectIdentitiesDictionary != null)
            {
                var statusEffectIdentities = mechanicDataConfigItem.StatusEffectIdentitiesDictionary.ToList();
                foreach (var statusEffectIdentity in statusEffectIdentities)
                    statusEffectIdentitiesDictionary.Add(statusEffectIdentity.Key, statusEffectIdentity.Value);
            }

            var weaponStatusEffectModelsDictionary = await GetStatusEffectModelsDictionary(statusEffectIdentitiesDictionary);

            return new WeaponData(weaponDataConfigItem, mechanicDataConfigItem, weaponStatusEffectModelsDictionary);
        }

        private async UniTask<Dictionary<string, StatusEffectModel[]>> GetStatusEffectModelsDictionary(Dictionary<string, ModifierIdentity[]> statusEffectIdentitiesDictionary)
        {
            if (statusEffectIdentitiesDictionary != null)
            {
                var statusEffectModelsDictionary = new Dictionary<string, StatusEffectModel[]>();
                foreach (KeyValuePair<string, ModifierIdentity[]> kvp in statusEffectIdentitiesDictionary)
                {
                    var statusEffectIdentities = kvp.Value;
                    if (statusEffectIdentities != null)
                    {
                        var statusEffectModels = new StatusEffectModel[statusEffectIdentities.Length];
                        for (int i = 0; i < statusEffectIdentities.Length; i++)
                        {
                            var statusEffectDataConfigItem = await DataManager.Config.LoadStatusEffectDataConfigItem(statusEffectIdentities[i].modifierType,
                                                                                                                         statusEffectIdentities[i].modifierDataId);
                            var statusEffectData = new StatusEffectData(statusEffectDataConfigItem);
                            var statusEffectModel = StatusEffectModelFactory.GetStatusEffectModel(statusEffectIdentities[i].modifierType, statusEffectData);
                            statusEffectModels[i] = statusEffectModel;
                        }
                        statusEffectModelsDictionary.Add(kvp.Key, statusEffectModels);
                    }
                }
                return statusEffectModelsDictionary;
            }
            else return null;
        }

        private async UniTask<EquipmentMechanicDataConfigItem> GetEquipmentMechanicDataAsync(EquipmentSystemType equipmentSystemType, RarityType rarityType)
        {
            string equipmentMechanicDataConfigAssetName = string.Format(AddressableKeys.EQUIPMENT_MECHANIC_DATA_CONFIG_ASSET_FORMAT, equipmentSystemType);
            var equipmentMechanicDataConfig = await DataManager.Config.Load<EquipmentMechanicDataConfig>(equipmentMechanicDataConfigAssetName);
            var equipmentMechanicDataConfigItem = equipmentMechanicDataConfig.GetEquipmentDataConfigItem(rarityType);
            return equipmentMechanicDataConfigItem;
        }

        private async UniTask<SkillTreeDataConfigItem> GetSkillTreeDataConfigAsync(SkillTreeSystemType skillTreeSystemType, int dataId)
        {
            var assetName = string.Format(AddressableKeys.SKILL_TREE_DATA_CONFIG_ASSET_FORMAT, skillTreeSystemType);
            var dataConfig = await DataManager.Config.Load<SkillTreeSystemDataConfig>(assetName);
            var dataConfigItem = dataConfig.GetSkillTreeDataConfigItem(dataId);
            return dataConfigItem;
        }

        private async UniTask LoadConfig(CancellationToken cancellationToken)
        {
            SkillDataFactory = new SkillDataFactory();
            DeathDataFactory = new DeathDataFactory();
            HeroExpConfig = await DataManager.Config.LoadHeroExpInfo(cancellationToken);
            await LoadStageLoadConfig(cancellationToken);
            await LoadEquipmentUnlockedSubstatCondition(cancellationToken);
            await LoadQuestConfig(cancellationToken);
            await LoadSkillTreeConfig(cancellationToken);
            await LoadHeroDataAsync(cancellationToken);
        }

        public async UniTask<float> GetAttackRangeOfWeapon(WeaponType weaponType)
        {
            string weaponDataConfigAssetName = string.Format(AddressableKeys.WEAPON_DATA_CONFIG_ASSET_FORMAT, weaponType);
            var weaponDataConfig = await Addressables.LoadAssetAsync<WeaponDataConfig>(weaponDataConfigAssetName);
            var weaponDataConfigItem = weaponDataConfig.GetWeaponDataConfigItem();
            var attackRange = weaponDataConfigItem.AttackRange;
            return attackRange;
        }

        private async UniTask LoadEquipmentUnlockedSubstatCondition(CancellationToken cancellationToken)
            => EquipmentUnlockedSubStatConfig = await DataManager.Config.Load<EquipmentUnlockedSubStatConfig>(cancellationToken);

        private async UniTask LoadQuestConfig(CancellationToken cancellationToken)
        {
            var questDataFactory = new QuestDataFactory();
            var questIdentities = DataDispatcher.Instance.StageInfoData.GetQuestIdentities();
            QuestDataConfigItems = new();

            foreach (var questIdentity in questIdentities)
            {
                var questDataConfigItem = await questDataFactory.GetQuestDataConfigItem(questIdentity.questType, questIdentity.questDataId, cancellationToken);
                QuestDataConfigItems.Add((questIdentity.questType, questIdentity.questDataId), questDataConfigItem);
            }
        }

        private async UniTask LoadSkillTreeConfig(CancellationToken cancellationToken)
        {
            var skillTreeDataConfig = await DataManager.Config.Load<SkillTreeDataConfig>(cancellationToken);
            SkillTreeSecondBranchConfigItems = skillTreeDataConfig.secondBranchItems;
        }

        private async UniTask LoadStageLoadConfig(CancellationToken cancellationToken)
        {
            StageLoadConfig stageLoadConfigs = null;
            if (_gameModeType == GameModeType.Survival)
            {
                await DataManager.Config.LoadSurvivalModeInfo();
                stageLoadConfigs = await DataManager.Config.LoadSurvivalStageLoadConfig(cancellationToken);
            }
            else
            {
                stageLoadConfigs = await DataManager.Config.LoadStageLoadConfig(cancellationToken);
            }

            var stageInfoConfigs = await DataManager.Config.Load<StageInfoConfig>(cancellationToken);
            var stageId = DataDispatcher.Instance.StageId;

            StageLoadConfig = stageLoadConfigs.items.FirstOrDefault(x => x.stageId == stageId);
            StageInfoConfig = stageInfoConfigs.items.FirstOrDefault(x => x.stageId == stageId);

            var stageExtract = stageId.ExtractStageId();

            var highestStageInWorld = stageInfoConfigs.GetHighestStageIdInWorld(stageExtract.worldId, stageExtract.mode);

            var indexStageLoad = Array.IndexOf(stageInfoConfigs.items, StageInfoConfig);
            if (indexStageLoad < stageLoadConfigs.items.Length - 1)
            {
                if (stageId == highestStageInWorld && stageExtract.worldId != Constants.MAX_WORLD_ID)
                {
                    var nextStageId = (stageExtract.worldId + 1) * Constants.WORLD_FACTOR + (int)StageModeType.Normal * Constants.STAGE_MODE_FACTOR + 1;
                    NextStageInfoConfig = stageInfoConfigs.items.FirstOrDefault(x => x.stageId == nextStageId);
                    IsMaxStage = false;
                }
                else
                {
                    var nextStageIndex = indexStageLoad + 1;
                    NextStageInfoConfig = stageInfoConfigs.items[nextStageIndex];
                    IsMaxStage = false;
                }
            }
            else
            {
                NextStageInfoConfig = StageInfoConfig;
                IsMaxStage = true;
            }
        }

        private void FinishedLoading()
            => Messenger.Publisher().Publish(new SceneDataLoadedMessage());

        #endregion Class Methods
    }
}