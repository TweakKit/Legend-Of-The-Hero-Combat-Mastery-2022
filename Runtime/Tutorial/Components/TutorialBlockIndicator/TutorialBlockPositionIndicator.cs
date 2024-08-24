using UnityEngine;
using Runtime.Utilities;

namespace Runtime.Tutorial
{
    public class TutorialBlockPositionIndicator : TutorialBlockTargetIndicator<TutorialBlockPositionIndicatorData>
    {
        #region Members

        private Vector3 _targetLocation;
        private Vector3 _arrowLocation;
        private float _arrowOffset;
        private Vector3 _pointDirection;
        private Vector3 _targetScreenPosition;
        private Quaternion _arrowRotation;
        private bool _showArrow = false;
        private bool _outOfScreen;
        private string _displayDistance;
        private string _distanceFormat;
        private string _distancePostfix;

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
            base.SetUpProp(tutorialBlockIndicatorProp, isMain);
            PositionMask(tutorialBlockIndicatorProp, true);
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            PositionMask(tutorialBlockIndicatorProp);
        }

        private void PositionMask(TutorialBlockIndicatorProp mask, bool init = false)
        {
            var rectTransform = mask.transform as RectTransform;
            var positionMask = mask as TutorialBlockIndicatorPositionProp;
            if (positionMask != null)
                _arrowOffset = (OwnerBlockIndicatorData.maskSize / 2).magnitude + OwnerBlockIndicatorData.rectOffset;

            if (rectTransform != null)
            {
                if (init)
                {
                    // Set position mask size to config specified size on initialization.
                    rectTransform.sizeDelta = OwnerBlockIndicatorData.maskSize;
                    if (positionMask != null)
                    {
                        if (positionMask.arrowMask != null)
                        {
                            positionMask.arrowMask.rectTransform.sizeDelta = OwnerBlockIndicatorData.arrowSize;
                            positionMask.arrowMask.overrideSprite = OwnerBlockIndicatorData.arrowSprite;
                            positionMask.arrowMask.color = OwnerBlockIndicatorData.arrowColor;
                            positionMask.arrowMask.rectTransform.localPosition = OwnerBlockIndicatorData.arrowDirection * _arrowOffset;
                        }

                        if (positionMask.displayText != null)
                        {
                            positionMask.displayText.rectTransform.sizeDelta = OwnerBlockIndicatorData.fontRect.size;
                            positionMask.displayText.rectTransform.localPosition = OwnerBlockIndicatorData.fontRect.position;
                        }
                    }
                }

                // If using tracking when target is out of screen, the position will circle around the screen, otherwise, it would just fade out.
                if (mask.tutorialBlockTargetData.runtimeTarget != null)
                {
                    if (MathUtility.CalculateOutOfScreenPosition(mask.tutorialBlockTargetData.runtimeTarget.transform.position + OwnerBlockIndicatorData.positionOffset, TutorialNavigator.CurrentTutorial.Camera, Canvas, ref _targetLocation, OwnerBlockIndicatorData.screenOffset, out _outOfScreen))
                    {
                        if (OwnerBlockIndicatorData.trackOutOfScreen || !_outOfScreen)
                        {
                            SetPropStatus(mask, true);
                            SetDistance(positionMask, OwnerBlockIndicatorData.displayDistance, mask.tutorialBlockTargetData.runtimeTarget.transform.position);
                            rectTransform.localPosition = init ? _targetLocation : Vector3.Slerp(rectTransform.localPosition, _targetLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);

                            if (OwnerBlockIndicatorData.displayArrow)
                            {
                                if (positionMask != null && positionMask.arrowMask != null)
                                {
                                    if (_outOfScreen)
                                    {
                                        // Calculate how far the target is from display mask.
                                        if (MathUtility.CalculateLocalRectPoint(mask.tutorialBlockTargetData.runtimeTarget.transform.position, TutorialNavigator.CurrentTutorial.Camera, Canvas, out _targetScreenPosition))
                                        {
                                            // Determine if arrow should be shown.
                                            _showArrow = false;
                                            // If it is in front of camera, use target screen position with recttransform, else, use center and rect transform.
                                            if (_targetScreenPosition.z > 0)
                                            {
                                                _pointDirection = _targetScreenPosition - rectTransform.localPosition;
                                                if (_pointDirection.magnitude > _arrowOffset)
                                                    _showArrow = true;
                                            }
                                            else
                                            {
                                                _pointDirection = rectTransform.position - new Vector3(TutorialNavigator.CurrentTutorial.Camera.pixelWidth / 2, TutorialNavigator.CurrentTutorial.Camera.pixelHeight / 2);
                                                _showArrow = true;
                                            }

                                            if (_showArrow)
                                            {
                                                _pointDirection.z = 0;
                                                SetArrowMask(positionMask, true);

                                                if (positionMask.arrowMask.gameObject.activeSelf)
                                                {
                                                    _arrowLocation = _pointDirection.normalized * _arrowOffset;
                                                    positionMask.arrowMask.rectTransform.localPosition = init ? _arrowLocation : Vector3.Slerp(positionMask.arrowMask.rectTransform.localPosition, _arrowLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                                                    _arrowRotation = Quaternion.FromToRotation(OwnerBlockIndicatorData.arrowDirection, _pointDirection);
                                                    positionMask.arrowMask.rectTransform.localRotation = init ? _arrowRotation : Quaternion.Slerp(positionMask.arrowMask.rectTransform.localRotation, _arrowRotation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                                                }
                                                return;
                                            }
                                        }
                                    }
                                }
                            }

                            SetArrowMask(positionMask, false);
                            return;
                        }
                    }
                }

                SetDistance(positionMask, false, mask.tutorialBlockTargetData.runtimeTarget.transform.position);
                SetArrowMask(positionMask, false);
                SetPropStatus(mask, false);
            }
        }

        private void SetArrowMask(TutorialBlockIndicatorPositionProp pMask, bool enable)
        {
            if (pMask != null && pMask.arrowMask != null && pMask.arrowMask.gameObject.activeSelf != enable)
                pMask.arrowMask.gameObject.SetActive(enable);
        }

        private void SetDistance(TutorialBlockIndicatorPositionProp mask, bool display, Vector3 targetLocation)
        {
            if (mask != null)
            {
                bool shouldDisplay = display;
                _displayDistance = null;
                if (display)
                {
                    var referenceGameObject = TutorialNavigator.CurrentTutorial.Camera.gameObject;
                    if (referenceGameObject != null)
                    {
                        var distance = (targetLocation - referenceGameObject.transform.position).magnitude;
                        switch (OwnerBlockIndicatorData.distUnit)
                        {
                            case UnitType.Meter:
                                _distancePostfix = "m";
                                _distanceFormat = "N0";
                                break;

                            case UnitType.KiloMeter:
                                distance /= 1000;
                                _distancePostfix = "km";
                                _distanceFormat = "g4";
                                break;

                            case UnitType.Mile:
                                distance *= 0.0006213712f;
                                _distancePostfix = "mi";
                                _distanceFormat = "g4";
                                break;

                            case UnitType.Feet:
                                distance *= 3.28084f;
                                _distancePostfix = "ft";
                                _distanceFormat = "N0";
                                break;
                        }

                        if (distance > OwnerBlockIndicatorData.distThreshold)
                            _displayDistance = distance.ToString(_distanceFormat) + " " + _distancePostfix;
                        else
                            shouldDisplay = false;
                    }
                    else shouldDisplay = false;
                }

                mask.SetDisplayText(shouldDisplay, _displayDistance);
            }
        }

        #endregion Class Methods
    }
}