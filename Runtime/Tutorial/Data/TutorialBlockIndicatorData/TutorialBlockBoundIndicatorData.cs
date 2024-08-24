using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockBoundIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public Vector3 positionOffset;
        public float magnitudeOffset;
        public Vector3 rotation;
        public Vector3 scale;
        public Vector3 anchor;
        public bool autoCalculateRotation;

        #endregion Members
    }
}