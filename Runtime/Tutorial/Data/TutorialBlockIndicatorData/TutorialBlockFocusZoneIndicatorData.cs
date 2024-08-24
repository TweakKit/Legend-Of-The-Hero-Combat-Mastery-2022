using System;
using UnityEngine;
using UnityEngine.Events;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockFocusZoneIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public float dimImageAlpha = 0.75f;
        public bool disableRaycastOnDimImage;
        public bool endTutorialByClickInFocusImage;
        public PointerTriggerType endTutorialConditionPointerTrigger;
        public UnityEvent endTutorialEvent;
        public Vector2 rectOffset;
        public Vector2 positionOffset;
        public bool showOutFocusZoneEffect;
        public GameObject outfocusZoneEffectPrefab;
        public float outfocusZoneEffectScaleSpeed;
        public float outfocusZoneEffectScaleValue;

        #endregion Members

        #region Class Methods

        public void AddEndTutorialAction(UnityAction eventAction)
            => endTutorialEvent.AddListener(eventAction);

        #endregion Class Methods
    }
}