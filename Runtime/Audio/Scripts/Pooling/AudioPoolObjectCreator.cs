using UnityEngine;

namespace Runtime.Audio
{
    public class AudioPoolObjectCreator : MonoBehaviour
    {
        #region Members

        [SerializeField]
        protected GameObject objectPrefab;
        [SerializeField]
        protected int size;

        #endregion Members

        #region Class Methods

        public virtual void Init(AudioPoolManager poolManager)
            => poolManager.CreatePool(objectPrefab, transform, size);

        #endregion Class Methods
    }
}