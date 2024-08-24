using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockHighlightIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public Vector2 rectOffset;
        public Vector2 positionOffset;

        #endregion Members
    }
}