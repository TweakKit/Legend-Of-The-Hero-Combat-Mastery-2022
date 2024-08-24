using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Utilities;

namespace Runtime.Tutorial
{
    public class TutorialBlockPopupIndicator : TutorialBlockTargetIndicator<TutorialBlockPopupIndicatorData>
    {
        #region Members

        [SerializeField]
        private Button _closeButton;
        private Vector3 _targetLocation;
        private List<Coroutine> _scaleCoroutines;
        private Vector2 _interceptPoint;
        private Vector3 _smoothDampVelocity;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            _scaleCoroutines = new List<Coroutine>();
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            SetUpClosePopup();
        }

        public override void Stop()
        {
            base.Stop();
            for (int i = 0; i < props.Count; i++)
                ConfigurePopupProp(props[i], true);
            ScaleInOutProps(false);
        }

        public override void SetUpProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.SetUpProp(tutorialBlockIndicatorProp, isMain);
            ConfigurePopupProp(tutorialBlockIndicatorProp);
            PositionPopup(tutorialBlockIndicatorProp, true);
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            PositionPopup(tutorialBlockIndicatorProp);
        }

        protected override void PostInit()
        {
            base.PostInit();
            ScaleInOutProps(true);
        }

        private void PositionPopup(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool init = false)
        {
            var rectTransform = tutorialBlockIndicatorProp.transform as RectTransform;
            if (rectTransform != null)
            {
                if (init)
                    rectTransform.sizeDelta = OwnerBlockIndicatorData.popupSize;

                if (OwnerBlockIndicatorData.trackOutOfScreen && tutorialBlockIndicatorProp.tutorialBlockTargetData.OutOfScreen)
                {
                    if (tutorialBlockIndicatorProp.tutorialBlockTargetData.runtimeTarget != null)
                    {
                        SetPropStatus(tutorialBlockIndicatorProp, true);
                        MathUtility.CalculateOutOfScreenPosition(tutorialBlockIndicatorProp.tutorialBlockTargetData.runtimeTarget, TutorialNavigator.CurrentTutorial.Camera, Canvas, ref _targetLocation, OwnerBlockIndicatorData.screenOffset);
                        rectTransform.localPosition = init ? _targetLocation : Vector3.Slerp(rectTransform.localPosition, _targetLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                    }
                    else SetPropStatus(tutorialBlockIndicatorProp, false);
                }
                else
                {
                    CalculateRect(tutorialBlockIndicatorProp);
                    if (tutorialBlockIndicatorProp.propImageRect.size.magnitude != 0)
                    {
                        SetPropStatus(tutorialBlockIndicatorProp, true);
                        if (OwnerBlockIndicatorData.dontSetTransform)
                        {
                            _targetLocation = Vector3.zero;
                        }
                        else
                        {
                            if (OwnerBlockIndicatorData.autoAnchor)
                            {
                                var tempRect = tutorialBlockIndicatorProp.propImageRect;
                                tempRect.size += OwnerBlockIndicatorData.rectOffset;
                                tempRect.center -= OwnerBlockIndicatorData.rectOffset / 2;
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
                        }
                        rectTransform.localPosition = init ? _targetLocation : Vector3.Slerp(rectTransform.localPosition, _targetLocation, OwnerBlockIndicatorData.smoothValue * Time.unscaledDeltaTime);
                    }
                    else SetPropStatus(tutorialBlockIndicatorProp, false);
                }
            }
        }


        private void ScaleInOutProps(bool scaleIn)
        {
            foreach (var scaleCoroutine in _scaleCoroutines)
                StopCoroutine(scaleCoroutine);

            for (int i = 0; i < props.Count; i++)
            {
                if (OwnerBlockIndicatorData.scaleInOut)
                {
                    var coroutine = StartCoroutine(ScaleInOutProp(props[i], scaleIn));
                    _scaleCoroutines.Add(coroutine);
                }
            }
        }

        private void SetUpClosePopup()
        {
            if (OwnerBlockIndicatorData.canCloseAsTouchOutSidePopup)
            {
                _closeButton.gameObject.SetActive(true);
                _closeButton.interactable = false;
                if (OwnerBlockIndicatorData.canCloseAsTouchOutSidePopup)
                    StartCoroutine(EnableCloseAsTouchOutside());
            }
            else _closeButton.gameObject.SetActive(false);
        }

        private IEnumerator ScaleInOutProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool scaleIn)
        {
            var targetScale = scaleIn ? Vector3.one : Vector3.zero;
            tutorialBlockIndicatorProp.transform.localScale = scaleIn ? Vector3.zero : tutorialBlockIndicatorProp.transform.localScale;
            while ((tutorialBlockIndicatorProp.transform.localScale - targetScale).magnitude > 0.01f)
            {
                tutorialBlockIndicatorProp.transform.localScale = Vector3.SmoothDamp(tutorialBlockIndicatorProp.transform.localScale,
                                                                                     targetScale,
                                                                                     ref _smoothDampVelocity,
                                                                                     OwnerBlockIndicatorData.scaleAnimationSmoothValue,
                                                                                     OwnerBlockIndicatorData.smoothValue,
                                                                                     Time.unscaledDeltaTime);
                yield return null;
            }
            tutorialBlockIndicatorProp.transform.localScale = targetScale;
        }

        private IEnumerator EnableCloseAsTouchOutside()
        {
            yield return new WaitForSeconds(OwnerBlockIndicatorData.enableCloseAsTouchOutsizePopUpDelay);
            _closeButton.interactable = true;
            _closeButton.onClick.AddListener(CloseIndicator);
        }

        private void ConfigurePopupProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool clear = false)
        {
            var tutorialBlockPopupIndicatorProp = tutorialBlockIndicatorProp as TutorialBlockPopupIndicatorProp;
            if (tutorialBlockPopupIndicatorProp != null)
            {
                if (!clear)
                {
                    tutorialBlockPopupIndicatorProp.closeButton?.onClick.RemoveAllListeners();
                    tutorialBlockPopupIndicatorProp.closeButton?.onClick.AddListener(CloseIndicator);
                    tutorialBlockPopupIndicatorProp.advanceButton?.onClick.RemoveAllListeners();
                    tutorialBlockPopupIndicatorProp.advanceButton?.onClick.AddListener(AdvanceIndicator);
                    tutorialBlockPopupIndicatorProp.backButton?.onClick.RemoveAllListeners();
                    tutorialBlockPopupIndicatorProp.backButton?.onClick.AddListener(StepBackIndicator);
                    tutorialBlockPopupIndicatorProp.SetUpContent(OwnerBlockIndicatorData.useTemporaryInfo, OwnerBlockIndicatorData.titleId, OwnerBlockIndicatorData.messageId);
                }
                else
                {
                    tutorialBlockPopupIndicatorProp.closeButton?.onClick.RemoveAllListeners();
                    tutorialBlockPopupIndicatorProp.advanceButton?.onClick.RemoveAllListeners();
                    tutorialBlockPopupIndicatorProp.backButton?.onClick.RemoveAllListeners();
                }

                if (TutorialNavigator.CurrentTutorial.CheckSequenceStart(OwnerBlockData.blockIndex))
                    tutorialBlockPopupIndicatorProp.backButton?.gameObject.SetActive(false);
            }
        }

        private void CloseIndicator()
            => TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex, OwnerBlockData.switchData.switchType == SwitchType.Automatic);

        private void AdvanceIndicator()
            => TutorialNavigator.CurrentTutorial.AdvanceTutorial();

        private void StepBackIndicator()
            => TutorialNavigator.CurrentTutorial.StepbackTutorial();

        #endregion Class Methods
    }
}