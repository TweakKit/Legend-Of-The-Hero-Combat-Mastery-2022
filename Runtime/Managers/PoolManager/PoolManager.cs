using System.Threading;
using UnityEngine;
using Unity.Pooling;
using UnityEngine.AddressableAssets;
using Collections.Pooled.Generic;
using Collections.Pooled.Generic.Internals.Unsafe;
using Runtime.Core.Singleton;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public class PoolManager : MonoSingleton<PoolManager>
    {
        #region Members

        private Dictionary<string, GameObjectPool> _pools = new Dictionary<string, GameObjectPool>();

        #endregion Members

        #region API Methods

        private void OnDestroy()
        {
            _pools.GetUnsafe(out var entries, out var count);
            for (int i = 0; i < count; i++)
            {
                ref var entry = ref entries[i];
                if (entry.Next >= -1)
                    entry.Value.Dispose();
            }
            _pools.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        public async UniTask<GameObject> Get(string prefabId, bool isActive = true)
        {
            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                var pooledObject = await Addressables.LoadAssetAsync<GameObject>(prefabId);
                var gameObjectPrefab = new GameObjectPrefab { Source = pooledObject };
                pool = new GameObjectPool(gameObjectPrefab);
                _pools[prefabId] = pool;
            }

            var entity = await pool.Rent();
            entity.name = prefabId;
            entity.transform.SetParent(null);
            entity.SetActive(isActive);
            return entity;
        }

        public async UniTask<GameObject> Get(string prefabId, CancellationToken cancellationToken, bool isActive = true)
        {
            if (!_pools.TryGetValue(prefabId, out var pool))
            {
                var pooledObject = await Addressables.LoadAssetAsync<GameObject>(prefabId).WithCancellation(cancellationToken);
                var gameObjectPrefab = new GameObjectPrefab { Source = pooledObject };
                pool = new GameObjectPool(gameObjectPrefab);
                _pools[prefabId] = pool;
            }

            var entity = await pool.Rent(cancellationToken);
            entity.name = prefabId;
            entity.transform.SetParent(null);
            entity.SetActive(isActive);
            return entity;
        }

        public void Remove(GameObject gameObject)
        {
            gameObject.transform.SetParent(transform);
            var pooledObjectId = gameObject.name;
            if (_pools.TryGetValue(pooledObjectId, out var pool))
                pool.Return(gameObject);
            else
                gameObject.SetActive(false);
        }

        #endregion Class Methods
    }
}