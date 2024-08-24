using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Utilities;

namespace Runtime.Tutorial
{
    /// <summary>
    /// Base indicator for indicators working with target.
    /// </summary>
    public class TutorialBlockTargetIndicator<T> : TutorialBlockIndicator where T : TutorialBlockTargetIndicatorData
    {
        #region Members

        [HideInInspector]
        public bool shouldFocus;

        #endregion Members

        #region Properties

        public new T OwnerBlockIndicatorData => base.OwnerBlockIndicatorData as T;

        #endregion Properties

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            shouldFocus = false;
            InitStuff(tutorialBlockData.focusTargetsData.ToList());
        }

        public override void RunUpdate()
        {
            base.RunUpdate();
            if (shouldFocus)
            {
                for (int i = 0; i < props.Count; i++)
                {
                    if (props[i].tutorialBlockTargetData.runtimeTarget != null)
                        FollowObject(props[i], i == 0);

                    RunPropAction(props[i]);
                }
            }
        }

        protected override void PostInit()
        {
            base.PostInit();
            shouldFocus = true;
            for (int i = 0; i < props.Count; i++)
            {
                if (props[i].tutorialBlockTargetData.runtimeTarget != null)
                {
                    SetUpProp(props[i], i == 0);
                    props[i].gameObject.SetActive(true);
                }
            }
            SetVisibility(true);
        }

        public override void Stop()
        {
            shouldFocus = false;
            base.Stop();
        }

        public virtual void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            ClearProps();
            for (int i = 0; i < tutorialBlockTargetsData.Count; i++)
                CreateStuff(tutorialBlockTargetsData[i]);
            PostInit();
        }

        public virtual void CreateStuff(TutorialBlockTargetData tutorialBlockTargetData)
        {
            if (OwnerBlockIndicatorData.maskPrefab != null)
            {
                var tutorialBlockIndicatorPropGameObject = Instantiate(OwnerBlockIndicatorData.maskPrefab);
                if (tutorialBlockIndicatorPropGameObject != null)
                {
                    var tutorialBlockIndicatorProp = tutorialBlockIndicatorPropGameObject.GetComponent<TutorialBlockIndicatorProp>();
                    if (tutorialBlockIndicatorProp != null)
                    {
                        if (!props.Contains(tutorialBlockIndicatorProp))
                            props.Add(tutorialBlockIndicatorProp);

                        tutorialBlockIndicatorProp.tutorialBlockTargetData = tutorialBlockTargetData;
                        tutorialBlockIndicatorPropGameObject.transform.SetParent(transform);
                        tutorialBlockIndicatorPropGameObject.transform.localPosition = Vector3.zero;
                        tutorialBlockIndicatorPropGameObject.transform.localRotation = Quaternion.identity;
                        tutorialBlockIndicatorPropGameObject.transform.localScale = Vector3.one;
                        tutorialBlockIndicatorPropGameObject.gameObject.SetActive(false);

                        if (tutorialBlockTargetData.runtimeTarget != null)
                            tutorialBlockTargetData.UpdateTargetScreenRect();
                    }
                    else Destroy(tutorialBlockIndicatorPropGameObject);
                }
            }
        }

        public virtual void SetUpProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
            => tutorialBlockIndicatorProp.Init();

        public virtual void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain) { }
        public virtual void RunPropAction(TutorialBlockIndicatorProp tutorialBlockIndicatorProp) { }

        public void CalculateRect(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            switch (tutorialBlockIndicatorProp.tutorialBlockTargetData.targetBoundType)
            {
                case TutorialTargetBoundType.None:
                    tutorialBlockIndicatorProp.propImageRect = new Rect();
                    break;

                case TutorialTargetBoundType.RectTransform:
                    if (tutorialBlockIndicatorProp.tutorialBlockTargetData.ownerCanvas != null)
                        ConvertRectToCanvasRect(tutorialBlockIndicatorProp.tutorialBlockTargetData.screenRect, ref tutorialBlockIndicatorProp.propImageRect);
                    break;

                default:
                    ConvertRectToCanvasRect(tutorialBlockIndicatorProp.tutorialBlockTargetData.screenRect, ref tutorialBlockIndicatorProp.propImageRect);
                    break;
            }
        }

        /// <summary>
        /// Convert screen rect to the local rect.
        /// </summary>
        private void ConvertRectToCanvasRect(Rect objectRect, ref Rect rect)
        {
            if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                TransformUtility.ScreenRectToLocalRectInRectangle((RectTransform)Canvas.transform, objectRect, null, out rect);
            else
                TransformUtility.ScreenRectToLocalRectInRectangle((RectTransform)Canvas.transform, objectRect, Canvas.worldCamera, out rect);
        }

        #endregion Class Methods
    }
}