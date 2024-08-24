using System;
using UnityEngine;

namespace Runtime.UI
{
    public abstract class EnhancedScrollerCellView<T> : MonoBehaviour where T : class
    {
        #region Members

        protected T data;

        #endregion Members

        #region API Methods

        private void OnDestroy()
            => Dispose();

        #endregion API Methods

        #region Class Methods

        public virtual void Init(T data, Action<T> onClickSelectCellViewButton)
            => this.data = data;

        protected virtual void Dispose() { }
        protected abstract void SetVisibility(bool isVisible);

        #endregion Class Methods
    }
}