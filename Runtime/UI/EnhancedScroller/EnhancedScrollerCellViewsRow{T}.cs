using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace Runtime.UI
{
    public abstract class EnhancedScrollerCellViewsRow<T> : EnhancedScrollerCellView where T : class
    {
        #region Members

        [SerializeField]
        private EnhancedScrollerCellView<T>[] _cellViews;

        #endregion Members

        #region Class Methods

        public virtual void Init(ref SmallList<T> data, int startingIndex, Action<T> onClickSelectCellViewButton)
        {
            for (var i = 0; i < _cellViews.Length; i++)
            {
                if (startingIndex + i < data.Count)
                    _cellViews[i].Init(data[startingIndex + i], onClickSelectCellViewButton);
                else
                    _cellViews[i].Init(null, null);
            }
        }

        #endregion Class Methods
    }
}