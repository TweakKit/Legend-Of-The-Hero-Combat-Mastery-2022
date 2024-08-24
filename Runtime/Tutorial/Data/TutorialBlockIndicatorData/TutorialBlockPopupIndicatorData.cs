using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockPopupIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public Vector2 popupSize;
        public bool dontSetTransform;
        public bool autoAnchor;
        public Vector2 anchor;
        public Vector2 rectOffset;
        public float scaleAnimationSmoothValue;
        public bool trackOutOfScreen;
        [Range(0, 1)]
        public float screenOffset;
        public bool scaleInOut;
        public bool useTemporaryInfo;
        public string titleId;
        public string messageId;
        public bool canCloseAsTouchOutSidePopup;
        [ShowIf(nameof(canCloseAsTouchOutSidePopup), true)]
        [Min(0.0f)]
        public float enableCloseAsTouchOutsizePopUpDelay;

        #endregion Members
    }
}