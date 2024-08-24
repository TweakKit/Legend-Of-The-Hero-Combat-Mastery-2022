using System.Collections.Generic;
using UnityEngine;
using Runtime.Core.Singleton;

namespace Runtime.Audio
{
    public class AudioPoolManager : MonoSingleton<AudioPoolManager>
    {
        #region Properties

        private Dictionary<string, AudioPoolObjectHolder> AudioPoolObjectsDictionary { get; set; }

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            AudioPoolObjectsDictionary = new Dictionary<string, AudioPoolObjectHolder>();
            AudioPoolObjectCreator[] audioPoolObjectCreators = gameObject.GetComponentsInChildren<AudioPoolObjectCreator>();
            foreach (var audioPoolObjectCreator in audioPoolObjectCreators)
                audioPoolObjectCreator.Init(this);
        }

        #endregion API Methods

        #region Class Methods

        public void CreatePool(GameObject objectPrefab, Transform parentTransform, int poolSize)
        {
            if (AudioPoolObjectsDictionary.ContainsKey(objectPrefab.name))
            {
#if UNITY_EDITOR
                Debug.LogError("A pool of the same object type has been created!");
#endif
                return;
            }

            AudioPoolObjectHolder audioPoolObjectHolder = new AudioPoolObjectHolder(objectPrefab, parentTransform, poolSize);
            AudioPoolObjectsDictionary.Add(objectPrefab.name, audioPoolObjectHolder);
        }

        public GameObject GetObject(string objectPrefabName, bool isActive = true)
        {
#if UNITY_EDITOR
            if (!AudioPoolObjectsDictionary.ContainsKey(objectPrefabName))
            {
                Debug.LogError("There is no pool of this object type!");
                return null;
            }
#endif
            AudioPoolObjectHolder audioPoolObjectHolder = AudioPoolObjectsDictionary[objectPrefabName];
            return audioPoolObjectHolder.GetObject(isActive);
        }

        public void ReturnObject(GameObject gameObject)
        {
            string objectName = gameObject.name;
#if UNITY_EDITOR
            if (!AudioPoolObjectsDictionary.ContainsKey(objectName))
            {
                Debug.LogError("There is no pool of this object type!");
                return;
            }
#endif
            AudioPoolObjectHolder audioPoolObjectHolder = AudioPoolObjectsDictionary[objectName];
            audioPoolObjectHolder.ReturnObject(gameObject);
        }

        #endregion Class Methods
    }
}