using System;
using CsvReader;

namespace Runtime.Definition
{
    [Serializable]
    public class SpawnedEntityInfo
    {
        #region Members

        public string entityId;
        public EntityType entityType;
        public uint entityNumber;
        public uint entityLevel;

        [CsvColumnIgnore]
        public uint entityLevelBonusCount;

        #endregion Members

        #region Properties

        public uint RuntimeEntityLevel
        {
            get
            {
                return entityLevel + entityLevelBonusCount;
            }
        }

        #endregion Properties

        #region Struct Methods

        public SpawnedEntityInfo() { }

        public SpawnedEntityInfo(string entityId, EntityType entityType, uint entityNumber, uint entityLevel)
        {
            this.entityId = entityId;
            this.entityType = entityType;
            this.entityNumber = entityNumber;
            this.entityLevel = entityLevel;
            entityLevelBonusCount = 0;
        }

        public void SetEntityLevelBonusCount(uint levelBonusCount)
            => entityLevelBonusCount = levelBonusCount;

        #endregion Struct Methods
    }
}