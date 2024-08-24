using System;
using UnityEngine;

namespace Runtime.Audio
{
    [Serializable]
    public class AudioEntry
    {
        #region Members

        public string audioName;
        public AudioClip audioFile;

        #endregion Members

        #region Class Methods

        public AudioEntry() { }

        #endregion Class Methods
    }

    [Serializable]
    public class MusicEntry : AudioEntry { }

    [Serializable]
    public class SoundEntry : AudioEntry { }
}