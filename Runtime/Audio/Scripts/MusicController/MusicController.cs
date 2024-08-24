using System;
using UnityEngine;
using UnityEngine.Audio;

namespace Runtime.Audio
{
    public class MusicController : BaseAudioController
    {
        #region Members

        [Header("=== MUSIC CONTROLLER SPECIFIC SETTINGS ===")]
        public MusicControllerSpecificSettingsData musicControllerSpecificSettingsData;
        public bool startAtEnable = true;
        public MusicContainer musicContainer;

        #endregion Members

        #region Class Methods

        public override void Init()
        {
            base.Init();
            if (musicContainer != null)
            {
                musicContainer.Init(this);
                if (startAtEnable)
                    musicContainer.Play();
            }
        }

        public override void SetSoundStatus(bool isSoundOn)
        {
            base.SetSoundStatus(isSoundOn);
            musicContainer.SetSoundStatus(isSoundOn);
            if (isSoundOn)
                Unmute();
            else
                Mute();
        }

        public override void Pause() => musicContainer.Pause();
        public override void Continue() => musicContainer.Continue();
        public void Play() => musicContainer.Play();
        public void Play(string name) => musicContainer.Play(name);
        public void PlayDynamic(string name) => musicContainer.PlayDynamic(name);
        public void Stop() => musicContainer.Stop();
        public void Mute() => musicContainer.Mute();
        public void Unmute() => musicContainer.Unmute();

        #endregion Class Methods
    }

    [Serializable]
    public class MusicControllerSpecificSettingsData
    {
        #region Members

        public string audioSourceTemplateWithFilter = "MusicSourceWithFilter";
        public AudioMixerGroup mixerGroupForDynamic;

        #endregion Members
    }
}