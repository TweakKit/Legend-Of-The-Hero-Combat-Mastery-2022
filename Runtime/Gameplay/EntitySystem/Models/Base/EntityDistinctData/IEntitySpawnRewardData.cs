using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public struct EntitySpawnRewardData
    {
        #region Members

        public ResourceType spawnRewardType;
        public int spawnRewardId;
        public long spawnRewardNumber;

        #endregion Members

        #region Struct Methods

        public EntitySpawnRewardData(ResourceType spawnRewardType, int spawnRewardId, long spawnRewardNumber)
        {
            this.spawnRewardType = spawnRewardType;
            this.spawnRewardId = spawnRewardId;
            this.spawnRewardNumber = spawnRewardNumber;
        }

        #endregion Struct Methods
    }
}