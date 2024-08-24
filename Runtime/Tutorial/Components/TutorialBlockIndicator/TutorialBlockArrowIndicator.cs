using UnityEngine;
using Runtime.Utilities;

namespace Runtime.Tutorial
{
    public class TutorialBlockArrowIndicator : TutorialBlockTargetIndicator<TutorialBlockArrowIndicatorData>
    {
        #region Members

        private Vector3 _targetLocation;
        private Quaternion _targetRotation;
        private Vector2 _interceptPoint;

        #endregion Members

        #region Class Methods

        public override void CreateStuff(TutorialBlockTargetData tutorialBlockTargetData)
        {
            if (tutorialBlockTargetData.runtimeTarget == null)
                return;
            base.CreateStuff(tutorialBlockTargetData);
        }

        public override void SetUpProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            if (tutorialBlockIndicatorProp.tutorialBlockTargetData.runtimeTarget == null)
                return;

            base.SetUpProp(tutorialBlockIndicatorProp, isMain);
            PositionArrow(tutorialBlockIndicatorProp, true);
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            PositionArrow(tutorialBlockIndicatorProp);
        }

        public override void RunPropAction(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            base.RunPropAction(tutorialBlockIndicatorProp);
            AnimateArrow(tutorialBlockIndicatorProp);
        }

        private void PositionArrow(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool init = false)
        {
            var rectTransform = tutorialBlockIndicatorProp.transform as RectTransform;
            if (rectTransform != null)
            {
                if (init)
                    rectTransform.sizeDelta = OwnerBlockIndicatorData.arrowSize;

                // If using tracking when target is out of screen, the arrow will circle around the screen, otherwise, it would just fade out.
                if (OwnerBlockIndicatorData.trackOutOfScreen && tutorialBlockIndicatorProp.tutorialBlockTargetData.OutOfScreen)
                {
                    if (tutorialBlockIndicatorProp.tutorialBlockTargetData.runtimeTarget != null)
                    {
                        SetPropStatus(tutorialBlockIndicatorProp, true);
                        var center = new Vector3(TutorialNavigator.CurrentTutorial.Camera.pixelWidth/2, TutorialNavigator.CurrentTutorial.Camera.pixelHeight / 2);
                        MathUtility.CalculateOutOfScreenPosition(tutorialBlockIndicatorProp.tutorialBlockTargetData.runtimeTarget, TutorialNavigator.CurrentTutorial.Camera, Canvas, ref _targetLocation, OwnerBlockIndicatorData.screenOffset);
                        rectTransform.localPosition = init ? _targetLocation : Vector3.Slerp(rectTransform.localPosition, _targetLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                        _targetRotation = Quaternion.FromToRotation(OwnerBlockIndicatorData.arrowDirection, (rectTransform.position - center).normalized);
                        rectTransform.localRotation = init ? _targetRotation : Quaternion.Slerp(rectTransform.localRotation, _targetRotation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                    }
                    else SetPropStatus(tutorialBlockIndicatorProp, false);
                }
                else
                {
                    CalculateRect(tutorialBlockIndicatorProp);
                    // Fade out prop when screen rect size is zero.
                    if (tutorialBlockIndicatorProp.propImageRect.size.magnitude != 0)
                    {
                        SetPropStatus(tutorialBlockIndicatorProp, true);
                        if (OwnerBlockIndicatorData.autoAnchor)
                        {
                            var tempRect = tutorialBlockIndicatorProp.propImageRect;
                            tempRect.size += OwnerBlockIndicatorData.rectOffset;
                            tempRect.center -= OwnerBlockIndicatorData.rectOffset / 2;
                            _interceptPoint = MathUtility.CalculateEdgeIntercept(OwnerBlockIndicatorData.pivotPoint, tempRect);
                            _targetLocation = MathUtility.CalculateOffsetPoint(tempRect.center, _interceptPoint, rectTransform.sizeDelta);
                        }
                        else
                        {
                            var tempRect = tutorialBlockIndicatorProp.propImageRect;
                            tempRect.size += OwnerBlockIndicatorData.rectOffset;
                            tempRect.center -= OwnerBlockIndicatorData.rectOffset / 2;
                            _targetLocation = new Vector2(tempRect.center.x + (OwnerBlockIndicatorData.anchor.x - 0.5f) * (tempRect.width + rectTransform.rect.width),
                                                          tempRect.center.y + (0.5f - OwnerBlockIndicatorData.anchor.y) * (tempRect.height + rectTransform.rect.height));
                        }

                        rectTransform.localPosition = init ? _targetLocation : Vector3.Slerp(rectTransform.localPosition, _targetLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                        if (OwnerBlockIndicatorData.autoCalculateRotation)
                            _targetRotation = Quaternion.FromToRotation(OwnerBlockIndicatorData.arrowDirection, (new Vector3(tutorialBlockIndicatorProp.propImageRect.center.x, tutorialBlockIndicatorProp.propImageRect.center.y) - rectTransform.localPosition).normalized);
                        else
                            _targetRotation = Quaternion.Euler(OwnerBlockIndicatorData.arrowRotation);
                        rectTransform.localRotation = init ? _targetRotation : Quaternion.Slerp(rectTransform.localRotation, _targetRotation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                    }
                    else SetPropStatus(tutorialBlockIndicatorProp, false);
                }
            }
        }

        private void AnimateArrow(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            if (OwnerBlockIndicatorData.animateArrow)
            {
                tutorialBlockIndicatorProp.animationProgressTimer += Time.unscaledDeltaTime * OwnerBlockIndicatorData.animateSpeed;
                tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = OwnerBlockIndicatorData.animateCurve.Evaluate(tutorialBlockIndicatorProp.animationProgressTimer) * OwnerBlockIndicatorData.animateMagnitude * OwnerBlockIndicatorData.arrowDirection;
            }
            else
            {
                if (tutorialBlockIndicatorProp.propImage.rectTransform.localPosition != Vector3.zero)
                    tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = Vector3.zero;
            }
        }

        #endregion Class Methods
    }
}