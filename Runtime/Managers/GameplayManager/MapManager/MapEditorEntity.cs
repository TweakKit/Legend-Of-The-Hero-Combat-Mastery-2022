using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay
{
    public class MapEditorEntity : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private EntityType _entityType = EntityType.Object;
        [SerializeField]
        private uint _level;
        [SerializeField]
        private uint _entityId;
        [SerializeField]
        private uint _waveActive = 0;
        [SerializeField]
        private float _delaySpawnInWave = 0;
        private uint _entityLevelBonusCount;

        #endregion Members

        #region Properties

        public uint Level => _level + _entityLevelBonusCount;
        public EntityType EntityType => _entityType;
        public uint EntityId => _entityId;
        public uint WaveActive => _waveActive;
        public float DelaySpawnInWave => _delaySpawnInWave;
        public bool ShouldDisable => _waveActive != 0 || _delaySpawnInWave > 0;

        #endregion Properties

        #region Class Methods

        public void SetEntityLevelBonusCount(uint levelBonusCount)
            => _entityLevelBonusCount = levelBonusCount;

        #endregion Class Methods
    }
}