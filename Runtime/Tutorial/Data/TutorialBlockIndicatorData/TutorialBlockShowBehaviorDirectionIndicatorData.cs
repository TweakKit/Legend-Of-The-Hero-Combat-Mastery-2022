using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockShowBehaviorDirectionIndicatorData : TutorialBlockArrowIndicatorData
    {
        #region Members

        public Vector3 originOffset;
        public Vector3 destinationOffset;

        #endregion Members
    }
}