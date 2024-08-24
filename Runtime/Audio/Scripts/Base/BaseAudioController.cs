using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Runtime.Audio
{
    public class BaseAudioController : MonoBehaviour
    {
        #region Members

        [HideInInspector]
        public AudioListener audioListener;
        protected bool isSoundOn;

        #endregion Members

        #region Class Methods

        public virtual void Init()
        {
            AudioListener[] audioListeners = GameObject.FindObjectsOfType<AudioListener>();
            foreach (AudioListener audioListener in audioListeners)
            {
                if (audioListener.enabled)
                {
                    this.audioListener = audioListener;
                    break;
                }
            }

            if (audioListener == null && audioListeners.Length > 0)
                audioListener = audioListeners[0];

            if (audioListener == null)
                Debug.LogError("No active audio listener has not been given!");
        }

        public virtual void SetSoundStatus(bool isSoundOn)
            => this.isSoundOn = isSoundOn;

        public virtual void Pause() { }
        public virtual void Continue() { }

        #endregion Class Methods
    }

    [Serializable]
    public class AudioControllerGlobalSettingsData
    {
        #region Members

        public string audioSourceTemplate = "AudioSource";
        public AudioMixerGroup mixerGroup;
        public bool useFilter = false;
        public string filter;
        [Range(0f, 1f)]
        public float minVolume = 1;
        [Range(0f, 1f)]
        public float maxVolume = 1;
        [Range(-3f, 3f)]
        public float minPitch = 1;
        [Range(-3f, 3f)]
        public float maxPitch = 1;

        #endregion Members
    }
}