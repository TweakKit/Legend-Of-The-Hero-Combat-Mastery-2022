using System;
using System.Collections.Generic;
using UnityEngine.Video;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockVideoIndicatorData : TutorialBlockTimeIndicatorData
    {
        #region Members

        public int numberOfVideoClips;
        public List<VideoClip> videoClips;

        #endregion Members
    }
}