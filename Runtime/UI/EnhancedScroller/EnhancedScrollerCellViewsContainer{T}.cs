using System;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;

namespace Runtime.UI
{
    public abstract class EnhancedScrollerCellViewsContainer<T> : MonoBehaviour, IEnhancedScrollerDelegate where T : class
    {
        #region Members

        [SerializeField]
        protected EnhancedScroller enhancedScroller;
        [SerializeField]
        protected EnhancedScrollerCellView enhancedScrollerCellViewPrefab;
        [SerializeField]
        protected int numberOfCellsPerRow = 3;
        [SerializeField]
        protected int cellSize = 100;
        protected SmallList<T> data = new();

        #endregion Members

        #region Properties

        protected Action<T> OnClickCellViewAction { get; set; }
        public int NumberOfCells { get { return data.Count; } }

        #endregion Properties

        #region API Methods

        protected virtual void Awake() => enhancedScroller.Delegate = this;

        #endregion API Methods

        #region Class Methods

        public virtual void Init(List<T> displayData, Action<T> onClickCellViewAction)
        {
            data.Clear();
            foreach (var data in displayData)
                this.data.Add(data);

            enhancedScroller.ReloadData();
            OnClickCellViewAction = onClickCellViewAction;
        }

        public virtual void Reload(List<T> updatedDisplayData)
        {
            data.Clear();
            foreach (var data in updatedDisplayData)
                this.data.Add(data);

            enhancedScroller.ReloadData(enhancedScroller.NormalizedScrollPosition);
        }

        public void ScrollToTop() => enhancedScroller.JumpToDataIndex(0);

        public virtual EnhancedScrollerCellView GetCellView(EnhancedScroller enhancedScroller, int dataIndex, int cellIndex)
        {
            EnhancedScrollerCellViewsRow<T> cellView = enhancedScroller.GetCellView(enhancedScrollerCellViewPrefab) as EnhancedScrollerCellViewsRow<T>;
            cellView.Init(ref data, dataIndex * numberOfCellsPerRow, OnClickCellView);
            return cellView;
        }

        public virtual EnhancedScrollerCellView GetCellView(int dataIndex)
        {
            var cellView = enhancedScroller.GetCellViewAtDataIndex(dataIndex);
            return cellView;
        }

        public virtual float GetCellViewSize(EnhancedScroller enhancedScroller, int dataIndex)
            => cellSize;

        public virtual int GetNumberOfCells(EnhancedScroller enhancedScroller)
            => Mathf.CeilToInt((float)data.Count / (float)numberOfCellsPerRow);

        protected virtual void OnClickCellView(T data)
            => OnClickCellViewAction?.Invoke(data);

        #endregion Class Methods
    }
}