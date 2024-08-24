using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Tutorial
{
    public class TutorialBlockIndicator : MonoBehaviour
    {
        #region Members

        protected List<TutorialBlockIndicatorProp> props;
        protected Canvas canvas;
        protected CanvasGroup canvasGroup;
        protected float smoothVelocity;

        #endregion Members

        #region Properties

        public TutorialBlockData OwnerBlockData { get; set; }
        protected TutorialBlockIndicatorData OwnerBlockIndicatorData { get; set; }
        public CanvasGroup CanvasGroup { get { return canvasGroup; } }
        public Canvas Canvas { get { return canvas; } }

        #endregion Properties

        #region API Methods

        private void Update()
            => RunUpdate();

        public virtual void Start() { }
        public virtual void OnEnable() { }

        public virtual void OnDisable()
            => Destroy();

        #endregion API Methods

        #region Class Methods

        public virtual void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            props = new List<TutorialBlockIndicatorProp>();
            canvas = gameObject.GetComponentInParent<Canvas>();
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            OwnerBlockIndicatorData = tutorialBlockIndicatorData;
            OwnerBlockData = tutorialBlockData;
            transform.SetAsFirstSibling();
        }

        protected virtual void PostInit() { }

        /// <summary>
        /// When initialized, this indicator's transform is placed at the topmost position in its parent transform.
        /// Since there are indicators that are required to be placed in the topmost position afterwards, then
        /// this function is here for a re-rearrange order, those override it can set their transform to be the topmost child.
        /// </summary>
        public virtual void RearrangeOrder() { }

        public virtual void Destroy()
        {
            OwnerBlockIndicatorData = null;
            OwnerBlockData = null;
        }

        public virtual void RunUpdate()
            => props.ForEach(x => x.RunUpdate());

        public virtual void Stop()
        {
            SetVisibility(false);
            EndIndicator();
        }

        public void SetPropStatus(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool enable)
        {
            if (tutorialBlockIndicatorProp != null && tutorialBlockIndicatorProp.gameObject.activeSelf != enable)
                tutorialBlockIndicatorProp.gameObject.SetActive(enable);
        }

        protected void ClearProps()
        {
            for (int i = props.Count - 1; i >= 0; i--)
                Destroy(props[i].gameObject);
            props.Clear();
        }

        protected void SetVisibility(bool isVisible)
        {
            CanvasGroup.alpha = isVisible ? 1 : 0;
            CanvasGroup.interactable = isVisible;
        }

        protected void EndIndicator()
        {
            ClearProps();
            CanvasGroup.interactable = false;
        }

        #endregion Class Methods
    }
}