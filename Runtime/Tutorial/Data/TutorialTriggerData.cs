using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialTriggerData
    {
        #region Members

        public TriggerType triggerType;
        [ShowIf(nameof(triggerType), TriggerType.Collider)]
        public TutorialColliderTriggerData colliderTriggerData;
        [ShowIf(nameof(triggerType), TriggerType.UI)]
        public TutorialGraphicTriggerData graphicTriggerData;
        [ShowIf(nameof(triggerType), TriggerType.KeyCode)]
        public KeyCode keyCode;

        #endregion Members
    }

    [Serializable]
    public class TutorialColliderTriggerData
    {
        #region Members

        public Collider collider;
        public bool useLayerFiltering;
        public LayerMask filterLayer;
        public bool useTagFiltering;
        public string filterTag;

        #endregion Members
    }

    [Serializable]
    public class TutorialGraphicTriggerData
    {
        #region Members

        public Graphic graphic;
        public PointerTriggerType pointerTrigger;

        #endregion Members
    }
}