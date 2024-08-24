using UnityEngine;

namespace Runtime.Tutorial
{
    public class TutorialBlockHighlightIndicator : TutorialBlockTargetIndicator<TutorialBlockHighlightIndicatorData>
    {
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
            CalculateRect(tutorialBlockIndicatorProp);
            tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta = new Vector2(tutorialBlockIndicatorProp.propImageRect.size.x + OwnerBlockIndicatorData.rectOffset.x, tutorialBlockIndicatorProp.propImageRect.size.y + OwnerBlockIndicatorData.rectOffset.y);
            tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImageRect.center + OwnerBlockIndicatorData.positionOffset;
        }

        public override void FollowObject(TutorialBlockIndicatorProp tutorialBlockIndicatorProp, bool isMain)
        {
            base.FollowObject(tutorialBlockIndicatorProp, isMain);
            CalculateRect(tutorialBlockIndicatorProp);
            if (!tutorialBlockIndicatorProp.tutorialBlockTargetData.OutOfScreen && tutorialBlockIndicatorProp.propImageRect.size.magnitude != 0)
            {
                SetPropStatus(tutorialBlockIndicatorProp, true);
                tutorialBlockIndicatorProp.propImage.rectTransform.sizeDelta = new Vector2(tutorialBlockIndicatorProp.propImageRect.size.x + OwnerBlockIndicatorData.rectOffset.x, tutorialBlockIndicatorProp.propImageRect.size.y + OwnerBlockIndicatorData.rectOffset.y);
                tutorialBlockIndicatorProp.propImage.rectTransform.localPosition = tutorialBlockIndicatorProp.propImageRect.center + OwnerBlockIndicatorData.positionOffset;
            }
            else SetPropStatus(tutorialBlockIndicatorProp, false);
        }

        #endregion Class Methods
    }
}