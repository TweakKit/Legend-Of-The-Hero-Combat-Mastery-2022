using System.Linq;
using Runtime.Message;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public class SurvivalEntitiesManager : EntitiesManager
    {
        #region Members

        private uint _currentSavedSpawnedWaveIndex;

        #endregion Members

        #region Properties

        public uint CurrentSavedSpawnedWaveIndex => _currentSavedSpawnedWaveIndex;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _currentSavedSpawnedWaveIndex = 0;
        }

        #endregion API Methods

        #region Class Methods

        public override async UniTask<HandleCharacterDiedResultType> HandleCharacterDied(CharacterDiedMessage characterDiedMessage)
        {
            var handleCharacterDiedResultType = await base.HandleCharacterDied(characterDiedMessage);
            if (handleCharacterDiedResultType == HandleCharacterDiedResultType.ClearWave)
            {
                var characterSpawnedWaveIndex = characterDiedMessage.CharacterModel.SpawnedWaveIndex;
                if (_currentSavedSpawnedWaveIndex < characterSpawnedWaveIndex)
                    _currentSavedSpawnedWaveIndex = characterSpawnedWaveIndex;
            }
            else if (handleCharacterDiedResultType == HandleCharacterDiedResultType.EnemyDiedNoSpawn)
            {
                var characterSpawnedWaveIndex = characterDiedMessage.CharacterModel.SpawnedWaveIndex;
                var sameEnemySpawnedWaveIndexesCount = EnemyModels.Count(x => x.SpawnedWaveIndex == characterSpawnedWaveIndex)
                                                     + currentActionContainEnemySpawnedWaveIndexes.Count(x => x == characterSpawnedWaveIndex);
                if (sameEnemySpawnedWaveIndexesCount == 0)
                {
                    if (_currentSavedSpawnedWaveIndex < characterSpawnedWaveIndex)
                        _currentSavedSpawnedWaveIndex = characterSpawnedWaveIndex;
                }
            }
            return handleCharacterDiedResultType;
        }

        #endregion Class Methods
    }
}