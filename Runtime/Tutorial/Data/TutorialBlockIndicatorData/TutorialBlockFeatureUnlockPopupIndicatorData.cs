using System;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockFeatureUnlockPopupIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public FeatureType featureType;
        public Sprite featureSprite;
        public float displayFeatureTime;
        public float flyDuration;
        public Vector3 flyToOffset;
        public float stopShowingFeatureDelay;

        #endregion Members
    }
}