using System.Collections.Generic;
using UnityEngine;
using Runtime.Utilities;
using Runtime.Core.Mics;
using Runtime.Gameplay.Tools.Easing;

namespace Runtime.Tutorial
{
    public class TutorialBlockShowBehaviorDirectionIndicator : TutorialBlockTargetIndicator<TutorialBlockShowBehaviorDirectionIndicatorData>
    {
        #region Members

        private Vector3 _targetLocation;
        private Quaternion _targetRotation;
        private Vector2 _interceptPoint;
        private IDraggable[] _draggableElements;
        private IDraggable _firstDraggableElement;
        private Vector2 _dragToPosition;
        private Vector3 _propPosition;
        private float _interpolationTime;

        #endregion Members

        #region Class Methods

        public override void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            base.InitStuff(tutorialBlockTargetsData);
            if (tutorialBlockTargetsData.Count > 0)
            {
                var targetTransform = tutorialBlockTargetsData[0].runtimeTarget.transform;
                _draggableElements = targetTransform.GetComponentsInChildren<IDraggable>();
                if (_draggableElements.Length > 0)
                {
                    foreach (var draggableElement in _draggableElements)
                        draggableElement.IsInteractable = false;

                    _firstDraggableElement = _draggableElements[0];
                    _firstDraggableElement.IsInteractable = true;
                    _dragToPosition = _firstDraggableElement.DragToPosition;
                }
            }
        }

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
            PositionProp(tutorialBlockIndicatorProp, true);
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            PositionProp(tutorialBlockIndicatorProp);
        }

        public override void RunPropAction(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            base.RunPropAction(tutorialBlockIndicatorProp);
            AnimateProp(tutorialBlockIndicatorProp);
            CheckTargetHasBeenDraggedSuccessfully();
        }

        private void PositionProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool init = false)
        {
            var rectTransform = tutorialBlockIndicatorProp.transform as RectTransform;
            if (rectTransform != null)
            {
                if (init)
                    rectTransform.sizeDelta = OwnerBlockIndicatorData.arrowSize;

                // If using tracking when target is out of screen, the prop will circle around the screen, otherwise, it would just fade out.
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

        private void AnimateProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            if (_interpolationTime >= 1.0f)
                _interpolationTime = 0.0f;
            _interpolationTime += Time.unscaledDeltaTime * OwnerBlockIndicatorData.animateSpeed;
            _interpolationTime = Mathf.Clamp01(_interpolationTime);
            _propPosition = _firstDraggableElement.OwnerTransform.position + OwnerBlockIndicatorData.originOffset;
            var interpolatedValue = Easing.EaseInSine(0.0f, 1.0f, _interpolationTime);
            var nextPosition = Vector3.Lerp(_propPosition, _dragToPosition, interpolatedValue);
            tutorialBlockIndicatorProp.propImage.transform.position = nextPosition;
        }

        private void CheckTargetHasBeenDraggedSuccessfully()
        {
            if (_firstDraggableElement != null && _firstDraggableElement.HasSucceededDragging)
            {
                foreach (var draggableElement in _draggableElements)
                    draggableElement.IsInteractable = true;
                TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);
            }
        }

        #endregion Class Methods
    }
}