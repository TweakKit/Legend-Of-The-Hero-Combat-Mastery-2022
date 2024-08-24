using System;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public abstract class TutorialBlockIndicatorData
    {
        #region Members

        public GameObject indicatorPrefab;
        public GameObject maskPrefab;
        public float smoothValue;

        #endregion Members

        #region Properties

        public int SequenceIndex { get; set; }
        public int BlockIndex { get; set; }

        #endregion Properties
    }
}