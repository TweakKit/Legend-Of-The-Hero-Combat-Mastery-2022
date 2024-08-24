using UnityEngine;

namespace Runtime.Gameplay.Manager
{
    public abstract class ReturnPool : MonoBehaviour
    {
        #region Properties

        protected abstract float ReturnDelayTime { get; }

        #endregion Properties

        #region API Methods

        protected virtual void OnEnable() => Invoke("InvokeReturnPool", ReturnDelayTime);

        #endregion API Methods

        #region Class Methods

        protected virtual void InvokeReturnPool()
        {
            gameObject.transform.SetParent(null);
            PoolManager.Instance.Remove(gameObject);
        }

        #endregion Class Methods
    }
}