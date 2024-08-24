using System;
using Sirenix.OdinInspector;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockTimeDelayIndicatorData : TutorialBlockTimeIndicatorData
    {
        #region Members

        public float delayTime;
        public bool blockScreen;
        public bool showTip;
        [ShowIf(nameof(showTip), true)]
        public string tipDescriptionId;

        #endregion Members
    }
}