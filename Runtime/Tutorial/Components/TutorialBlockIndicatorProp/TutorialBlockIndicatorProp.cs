using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Tutorial
{
    [ExecuteInEditMode]
    public class TutorialBlockIndicatorProp : MonoBehaviour
    {
        #region Members

        public Image propImage;
        [HideInInspector]
        public Rect propImageRect;
        [HideInInspector]
        public bool isActiveSelf;
        [HideInInspector]
        public Sprite originalSprite;
        [HideInInspector]
        public Color originalColor;
        [HideInInspector]
        public Vector2 originSize;
        [HideInInspector]
        public float animationProgressTimer;
        [HideInInspector]
        public TutorialBlockTargetData tutorialBlockTargetData;

        #endregion Members

        #region API Methods

        private void Awake() => InitAtAwake();
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }

        #endregion API Methods

        #region Class Methods

        public virtual void InitAtAwake()
        {
            if (propImage == null)
            {
                propImage = GetComponentInChildren<Image>();
                if (propImage == null)
                    propImage = gameObject.AddComponent<Image>();
                originalSprite = propImage.sprite;
                originalColor = propImage.color;
                originSize = propImage.rectTransform.sizeDelta;
            }
        }

        public virtual void Init()
            => animationProgressTimer = 0;

        public virtual void RunUpdate() { }

        public virtual void Destroy()
        {
            if (propImage != null)
            {
                propImage.sprite = originalSprite;
                propImage.overrideSprite = null;
                propImage.color = originalColor;
                propImage.rectTransform.sizeDelta = originSize;
            }
            tutorialBlockTargetData = null;
        }

        #endregion Class Methods
    }
}