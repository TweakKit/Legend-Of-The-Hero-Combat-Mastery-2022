using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Runtime.FeatureSystem;

namespace Runtime.Tutorial
{
    public class TutorialBlockFeatureUnlockIndicator : TutorialBlockTargetIndicator<TutorialBlockFeatureUnlockIndicatorData>
    {
        #region Members

        private UnlockFeature _targetUnlockFeature;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            _targetUnlockFeature?.UpdateStatus(false);
            StopAllCoroutines();
            StartCoroutine(WaitToUnlockFeature());
        }

        public override void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            base.InitStuff(tutorialBlockTargetsData);
            if (tutorialBlockTargetsData.Count > 0)
            {
                var target = tutorialBlockTargetsData[0].runtimeTarget;
                _targetUnlockFeature = target.GetComponent<UnlockFeature>();
            }
        }

        private IEnumerator WaitToUnlockFeature()
        {
            yield return new WaitForSeconds(OwnerBlockIndicatorData.waitToUnlockFeatureDelay);
            UnlockFeature();
        }

        private void UnlockFeature()
        {
            _targetUnlockFeature?.UpdateStatus(true);
            TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);
        }

        #endregion Class Method
    }
}