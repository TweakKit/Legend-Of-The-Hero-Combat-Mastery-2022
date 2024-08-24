using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockPositionIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public Vector3 positionOffset;
        public Vector2 maskSize;
        public bool displayArrow;
        public Sprite arrowSprite;
        public Color arrowColor;
        public Vector3 arrowDirection;
        public Vector2 arrowSize;
        public float rectOffset;
        public bool trackOutOfScreen;
        [Range(0, 1)]
        public float screenOffset;
        public bool displayDistance;
        public Rect fontRect;
        public bool useCamera;
        public float distThreshold;
        public UnitType distUnit;

        #endregion Members
    }
}