using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Tutorial
{
    public class TutorialBlockFocusZoneIndicator : TutorialBlockTargetIndicator<TutorialBlockFocusZoneIndicatorData>
    {
        #region Members

        [Tooltip("The dimming image for the entire screen.")]
        public Image dimImage;
        public Button outFocusZoneButton;
        private GameObject _outFocusZoneEffect;
        private Image _endTutorialByClickTriggerImage;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            if (OwnerBlockIndicatorData.showOutFocusZoneEffect)
            {
                outFocusZoneButton.onClick.RemoveAllListeners();
                outFocusZoneButton.onClick.AddListener(OnOutFocusZoneButtonClicked);
            }
            var dimImageColor = dimImage.color;
            dimImageColor.a = OwnerBlockIndicatorData.dimImageAlpha;
            dimImage.color = dimImageColor;
            dimImage.raycastTarget = !OwnerBlockIndicatorData.disableRaycastOnDimImage;
        }

        public override void RearrangeOrder()
            => transform.SetAsFirstSibling();

        public override void CreateStuff(TutorialBlockTargetData target)
        {
            if (target.runtimeTarget == null)
                return;
            base.CreateStuff(target);
        }

        public override void SetUpProp(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.SetUpProp(tutorialBlockIndicatorProp, isMain);

            CalculateRect(tutorialBlockIndicatorProp);
            tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta = new Vector2(tutorialBlockIndicatorProp.propImageRect.size.x + OwnerBlockIndicatorData.rectOffset.x, tutorialBlockIndicatorProp.propImageRect.size.y + OwnerBlockIndicatorData.rectOffset.y);
            tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImageRect.center + OwnerBlockIndicatorData.positionOffset;

            if (isMain)
            {
                // Set dimming image as last child, so it would correctly mask out the target area.
                if (dimImage != null)
                    dimImage.transform.SetAsLastSibling();

                // Set up the end tutorial by click trigger image.
                DestroyEndTutorialByClickTriggerImage();
                if (OwnerBlockIndicatorData.endTutorialByClickInFocusImage)
                    CreateEndTutorialByClickTriggerImage(tutorialBlockIndicatorProp);

                // Create out focus zone effect.
                if (OwnerBlockIndicatorData.showOutFocusZoneEffect)
                    CreateOutFocusZoneEffectEffect(tutorialBlockIndicatorProp);
                else
                    DestroyOutFocusZoneEffectEffect();
            }
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            CalculateRect(tutorialBlockIndicatorProp);
            // Show the mask image when not out of screen or has overlaps.
            if (!tutorialBlockIndicatorProp.tutorialBlockTargetData.OutOfScreen && tutorialBlockIndicatorProp.propImageRect.size.magnitude != 0 && dimImage.rectTransform.rect.Overlaps(tutorialBlockIndicatorProp.propImage.rectTransform.rect))
            {
                SetPropStatus(tutorialBlockIndicatorProp, true);
                tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta = new Vector2(tutorialBlockIndicatorProp.propImageRect.size.x + OwnerBlockIndicatorData.rectOffset.x, tutorialBlockIndicatorProp.propImageRect.size.y + OwnerBlockIndicatorData.rectOffset.y);
                tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImageRect.center + OwnerBlockIndicatorData.positionOffset;

                if (isMain)
                {
                    if (_endTutorialByClickTriggerImage != null)
                    {
                        _endTutorialByClickTriggerImage.rectTransform.sizeDelta = tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta;
                        _endTutorialByClickTriggerImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImage.rectTransform.localPosition;
                    }
                }
            }
            else SetPropStatus(tutorialBlockIndicatorProp, false);
        }

        private void DestroyEndTutorialByClickTriggerImage()
        {
            if (_endTutorialByClickTriggerImage != null)
                Destroy(_endTutorialByClickTriggerImage.gameObject);
        }

        /// <summary>
        //  Create an image and set it as the last child (lower than the dimming image in the hiearachy)
        //  inside this indicator. And by that way the image can be listened for clicking events
        //  (not being prevented by the dimming image any more).
        /// </summary>
        private void CreateEndTutorialByClickTriggerImage(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            _endTutorialByClickTriggerImage = new GameObject(nameof(_endTutorialByClickTriggerImage)).AddComponent<Image>();
            _endTutorialByClickTriggerImage.transform.SetParent(transform);
            _endTutorialByClickTriggerImage.transform.SetAsLastSibling();
            _endTutorialByClickTriggerImage.color = Color.clear;
            _endTutorialByClickTriggerImage.rectTransform.sizeDelta = tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta;
            _endTutorialByClickTriggerImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImage.rectTransform.localPosition;
            _endTutorialByClickTriggerImage.rectTransform.localScale = Vector3.one;
            var tutorialGraphicPointerTriggerObject = _endTutorialByClickTriggerImage.gameObject.AddComponent<TutorialGraphicPointerTriggerObject>();
            tutorialGraphicPointerTriggerObject.SetupTriggerData(OwnerBlockIndicatorData.endTutorialConditionPointerTrigger,
                                                                 OwnerBlockIndicatorData.SequenceIndex,
                                                                 OwnerBlockIndicatorData.BlockIndex,
                                                                 OwnerBlockIndicatorData.endTutorialEvent);
        }

        private void CreateOutFocusZoneEffectEffect(TutorialBlockIndicatorProp tutorialBlockIndicatorProp)
        {
            if (_outFocusZoneEffect == null)
                _outFocusZoneEffect = Instantiate(OwnerBlockIndicatorData.outfocusZoneEffectPrefab);

            if (_outFocusZoneEffect != null)
            {
                _outFocusZoneEffect.transform.SetParent(transform);
                _outFocusZoneEffect.transform.localPosition = tutorialBlockIndicatorProp.propImage.rectTransform.localPosition;
                _outFocusZoneEffect.transform.localRotation = Quaternion.identity;
                _outFocusZoneEffect.transform.localScale = Vector3.one;
                _outFocusZoneEffect.gameObject.SetActive(false);
            }
        }

        private void DestroyOutFocusZoneEffectEffect()
        {
            if (_outFocusZoneEffect != null)
                Destroy(_outFocusZoneEffect);
        }

        private void OnOutFocusZoneButtonClicked()
        {
            if (_outFocusZoneEffect != null)
                _outFocusZoneEffect.SetActive(true);
        }

        #endregion Class Methods
    }
}