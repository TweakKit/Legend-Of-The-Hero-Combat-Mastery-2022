using System.Collections.Generic;
using System.Linq;
using Runtime.Gameplay.EntitySystem;
using Runtime.Gameplay.Manager;

namespace Runtime.Tutorial
{
    public class GameplayGuideTutorialEntitiesManager : EntitiesManager
    {
        #region Class Methods

        public void ClearEnemies()
        {
            var characters = GetEntitiesOfType<Character>();
            for (int i = EnemyModels.Count - 1; i >= 0; i--)
            {
                var enemyModel = EnemyModels[i];
                var enemyCharacter = characters.FirstOrDefault(x => x.EntityUId == enemyModel.EntityUId);
                PoolManager.Instance.Remove(enemyCharacter.gameObject);
            }

            var spawnVfxGameobjects = FindObjectsOfType<SpawnVfxGameObject>();
            foreach (var item in spawnVfxGameobjects)
                PoolManager.Instance.Remove(item.gameObject);

            EnemyModels.Clear();
            currentWarningSpawnedEnemyCount = 0;
            currentSpawningEnemyCount = 0;
            currentActionContainEnemySpawnedWaveIndexes = new List<uint>();
        }

        #endregion Class Methods
    }
}