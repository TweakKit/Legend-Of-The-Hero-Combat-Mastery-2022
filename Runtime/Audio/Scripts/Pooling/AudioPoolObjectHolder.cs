using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    /// <summary>
    /// A pool object holder holds a queue of pool objects, is reponsible for getting, returning, cleaning objects in the pool.
    /// </summary>
    public class AudioPoolObjectHolder
    {
        #region Members

        private GameObject _objectPrefab;
        private Transform _parentTransform;
        private int _poolSize;
        private Queue<GameObject> _poolObjects;

        #endregion Members

        #region Class Methods

        public AudioPoolObjectHolder(GameObject objectPrefab, Transform parentTransform, int poolSize)
        {
            _objectPrefab = objectPrefab;
            _parentTransform = parentTransform;
            _poolSize = poolSize;
            _poolObjects = new Queue<GameObject>();
            CreatePool();
        }

        public GameObject GetObject(bool isActive)
        {
            if (_poolObjects.Count <= 0)
                CreatePool();

            GameObject returnedPoolObject = _poolObjects.Dequeue();
            returnedPoolObject.SetActive(isActive);
            return returnedPoolObject;
        }

        public void ReturnObject(GameObject gameObject)
        {
            gameObject.SetActive(false);
            _poolObjects.Enqueue(gameObject);
        }

        public void CleanPool()
        {
            while (_poolObjects.Count > 0)
            {
                GameObject poolObject = _poolObjects.Dequeue();
                GameObject.Destroy(poolObject);
            }

            _poolObjects = null;
        }

        private void CreatePool()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                GameObject poolObject = GameObject.Instantiate(_objectPrefab);
                poolObject.name = _objectPrefab.name;
                poolObject.transform.SetParent(_parentTransform);
                poolObject.SetActive(false);
                _poolObjects.Enqueue(poolObject);
            }
        }

        #endregion Class Methods
    }
}