using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockArrowIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public bool animateArrow;
        public AnimationCurve animateCurve;
        public float animateSpeed;
        public float animateMagnitude;
        public bool autoCalculateRotation;
        public Vector3 arrowDirection;
        public Vector3 arrowRotation;
        public Vector2 arrowSize;
        public bool autoAnchor;
        public Vector2 pivotPoint;
        public Vector2 anchor;
        public Vector2 rectOffset;
        public bool trackOutOfScreen;
        [Range(0, 1)]
        public float screenOffset;

        #endregion Members
    }
}