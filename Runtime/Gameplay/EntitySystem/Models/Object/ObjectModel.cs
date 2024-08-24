using System.Linq;
using System.Collections.Generic;
using Runtime.Definition;
using Runtime.ConfigModel;
using Runtime.Utilities;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ObjectModel : EntityModel
    {
        #region Members

        protected uint level;
        protected string destroyVfx;

        #endregion Members

        #region Properties

        public EntitySpawnRewardData[] EntitySpawnRewardsData { get; private set; }
        public SpawnedEntityInfo[] SpawnedEntitiesInfo { get; private set; }
        public override uint Level => level;
        public override EntityType EntityType => EntityType.Object;
        public string DestroyVFX => destroyVfx;

        #endregion Properties

        #region Class Methods

        public ObjectModel(uint spawnedWaveIndex, uint objectUId, uint objectId, ObjectLevelConfigItem objectConfig)
            : base(spawnedWaveIndex, objectUId, objectId.ToString(), Constants.IGNORE_PRIORITY_DETECT)
        {
            level = objectConfig.level;
            destroyVfx = objectConfig.destroyPrefabVfx;
            CalculateDroppableItems(objectConfig);
            InitStats(objectConfig);
            InitEvents(objectConfig);
        }

        protected partial void InitStats(ObjectLevelConfigItem objectConfigItem);
        protected partial void InitEvents(ObjectLevelConfigItem objectConfigItem);

        private void CalculateDroppableItems(ObjectLevelConfigItem objectConfigItem)
        {
            var listSpawnReward = new List<ResourceData>();
            var listSpawnEntity = new List<SpawnedEntityInfo>();

            var distributions = objectConfigItem.rewards.Select(x => x.resourceRate).ToList();
            distributions.AddRange(objectConfigItem.spawnEntities.Select(x => x.entityRate));

            var remain = 1 - distributions.Sum();
            distributions.Add(remain);

            var randomIndex = MathUtility.RandomChoiceFollowingDistribution(distributions);
            if (randomIndex <= objectConfigItem.rewards.Count() - 1)
            {
                listSpawnReward.Add(objectConfigItem.rewards[randomIndex].reward);
                EntitySpawnRewardsData = listSpawnReward.Select(x => new EntitySpawnRewardData(x.resourceType, x.resourceId, x.resourceNumber)).ToArray();
            }
            else
            {
                if (randomIndex == distributions.Count - 1)
                    return;
                var entityRandomIdx = randomIndex - objectConfigItem.rewards.Count();
                listSpawnEntity.Add(objectConfigItem.spawnEntities[entityRandomIdx].entityConfigItem);
                SpawnedEntitiesInfo = listSpawnEntity.ToArray();
            }
        }

        #endregion Class Methods
    }
}