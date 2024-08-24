using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Utilities;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockTargetData
    {
        #region Members

        public GameObject target;
        public TutorialTargetBoundType targetBoundType;
        [HideInInspector]
        public GameObject runtimeTarget;
        [HideInInspector]
        public Canvas ownerCanvas;
        [HideInInspector]
        public Rect screenRect;
        private Dictionary<Transform, Bounds> _unscaledBoundsDictionary = new Dictionary<Transform, Bounds>();
        private Bounds _targetBound = new Bounds();
        private List<RectTransform> _allRectTransforms;
        private bool _outOfScreen;

        #endregion Members

        #region Properties

        public Dictionary<Transform, Bounds> UnscaledBoundsDictionary
        {
            get
            {
                if (_unscaledBoundsDictionary == null)
                    _unscaledBoundsDictionary = new Dictionary<Transform, Bounds>();
                return _unscaledBoundsDictionary;
            }
            set
            {
                _unscaledBoundsDictionary = value;
            }
        }

        public bool OutOfScreen
        {
            get { return _outOfScreen; }
        }

        #endregion Properties

        #region Class Methods

        public TutorialBlockTargetData(GameObject target, TutorialTargetBoundType targetBoundType)
        {
            this.target = target;
            this.targetBoundType = targetBoundType;
            runtimeTarget = target;
        }

        /// <summary>
        /// Update the target screen rect.
        /// </summary>
        public void UpdateTargetScreenRect()
            => screenRect = CalculateRect();

        /// <summary>
        /// Update canvas reference.
        /// </summary>
        public void UpdateCanvas()
        {
            if (runtimeTarget != null && targetBoundType == TutorialTargetBoundType.RectTransform)
            {
                ownerCanvas = runtimeTarget.GetComponentInParent<Canvas>();
                _targetBound = TransformUtility.CalculateBounds(runtimeTarget.transform);
                if (_allRectTransforms == null)
                    _allRectTransforms = new List<RectTransform>();
                else
                    _allRectTransforms.Clear();
                _allRectTransforms.AddRange(runtimeTarget.GetComponentsInChildren<RectTransform>().ToList());
            }
        }

        /// <summary>
        /// Update individual target bound.
        /// </summary>
        public void UpdateTargetBound()
        {
            if (runtimeTarget != null && targetBoundType == TutorialTargetBoundType.Transform && UnscaledBoundsDictionary.ContainsKey(runtimeTarget.transform))
                _targetBound = UnscaledBoundsDictionary[runtimeTarget.transform];
        }

        /// <summary>
        /// Caculate rect depends on which type is used.
        /// </summary>
        private Rect CalculateRect()
        {
            var rect = new Rect();
            if (TutorialNavigator.CurrentTutorial != null)
            {
                switch (targetBoundType)
                {
                    case TutorialTargetBoundType.RectTransform:
                        if (ownerCanvas != null)
                        {
                            if (ownerCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                                rect = TransformUtility.RectTransformToScreenSpace(_allRectTransforms, null);
                            else if (ownerCanvas.renderMode == RenderMode.ScreenSpaceCamera)
                                rect = TransformUtility.RectTransformToScreenSpace(_allRectTransforms, ownerCanvas.worldCamera);
                            else if (ownerCanvas.renderMode == RenderMode.WorldSpace)
                                rect = TransformUtility.BoundsToScreenRect(ownerCanvas.worldCamera, _targetBound);
                        }
                        break;

                    case TutorialTargetBoundType.Transform:
                        if (runtimeTarget != null)
                            rect = TransformUtility.CalculateCombineScreenRect(runtimeTarget.transform, _targetBound, TutorialNavigator.CurrentTutorial.Camera, out _outOfScreen);
                        break;

                    default:
                        rect = TransformUtility.CalculateCombineScreenRect(UnscaledBoundsDictionary, TutorialNavigator.CurrentTutorial.Camera, out _outOfScreen);
                        break;
                }
            }

            return rect;
        }

        #endregion Class Methods
    }

    public enum TutorialTargetBoundType
    {
        None,
        All,
        RectTransform,
        Transform
    }
}