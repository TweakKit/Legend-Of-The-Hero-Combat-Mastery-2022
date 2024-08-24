using System;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockStageResultFeatureUnlockPopupIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public FeatureType featureType;
        public Sprite featureSprite;
        public float showHomeButtonDelay;

        #endregion Members
    }
}