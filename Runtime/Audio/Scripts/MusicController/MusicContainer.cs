using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    [Serializable]
    public class MusicContainer : AudioContainer
    {
        #region Members

        public List<MusicEntry> musicEntries;
        public List<MusicEntry> dynamicMusicEntries;
        private Dictionary<string, MusicEntry> _musicEntriesDictionary = new Dictionary<string, MusicEntry>();
        private Dictionary<string, MusicEntry> _dynamicMusicEntriesDictionary = new Dictionary<string, MusicEntry>();
        private bool _isSoundOn;

        #endregion Members

        #region Class Methods

        public override void Init(BaseAudioController audioController)
        {
            base.Init(audioController);
            InitNextSelection(musicEntries);

            _musicEntriesDictionary.Clear();
            foreach (MusicEntry entry in musicEntries)
            {
                if (string.IsNullOrEmpty(entry.audioName))
                {
                    if (entry.audioFile == null)
                        continue;
                    else
                        entry.audioName = entry.audioFile.name;
                }
                _musicEntriesDictionary.Add(entry.audioName, entry);
            }

            _dynamicMusicEntriesDictionary.Clear();
            foreach (MusicEntry entry in dynamicMusicEntries)
            {
                if (string.IsNullOrEmpty(entry.audioName))
                {
                    if (entry.audioFile == null)
                        continue;
                    else
                        entry.audioName = entry.audioFile.name;
                }
                _dynamicMusicEntriesDictionary.Add(entry.audioName, entry);
            }
        }

        public void SetSoundStatus(bool isSoundOn)
            => _isSoundOn = isSoundOn;

        public void Play()
        {
            AudioController.Instance.StartCoroutine(WaitForOneAudioSource("", false));
        }

        public void Play(string name)
        {
            if (!_musicEntriesDictionary.ContainsKey(name))
            {
                Debug.LogError("The audio entry does not exist for the key '" + name);
                return;
            }

            AudioController.Instance.StartCoroutine(WaitForOneAudioSource(name, false));
        }

        public void PlayDynamic(string name)
        {
            if (!_dynamicMusicEntriesDictionary.ContainsKey(name))
            {
                Debug.LogError("The dynamic audio entry does not exist for the key '" + name);
                return;
            }

            AudioController.Instance.StartCoroutine(WaitForOneAudioSource(name, true));
        }

        private IEnumerator WaitForOneAudioSource(string name, bool useDynamicEntries)
        {
            bool selectByName = !string.IsNullOrEmpty(name);
            while (true)
            {
                if (runningAudioSources.Count < 2)
                {
                    int index = -1;
                    foreach (MusicResult runningAudioSource in runningAudioSources)
                    {
                        // Start fading the other audio source.
                        runningAudioSource.fadeOutAudioSource = true;
                    }
                    if (selectByName)
                    {
                        if (useDynamicEntries)
                            index = dynamicMusicEntries.IndexOf(_dynamicMusicEntriesDictionary[name]);
                        else
                            index = musicEntries.IndexOf(_musicEntriesDictionary[name]);
                    }

                    MusicResult musicResult = PrepareAudioInstance(index, useDynamicEntries);
                    if (musicResult != null)
                    {
                        runningAudioSources.Add(musicResult);
                        PlayAudioSource(musicResult);
                    }
                    break;
                }
                else
                {
                    foreach (MusicResult runningMusicResult in runningAudioSources)
                    {
                        if (runningMusicResult.isAudioSourceReady)
                            runningMusicResult.fadeOutAudioSource = true;
                    }
                }

                yield return null;
            }
        }

        public override void Mute()
        {
            base.Mute();
            foreach (MusicResult musicResult in runningAudioSources)
                musicResult.audioSource.volume = 0.0f;
        }

        public override void Unmute()
        {
            base.Unmute();
            foreach (MusicResult musicResult in runningAudioSources)
                musicResult.audioSource.volume = musicResult.LastVolume;
        }

        public override void Stop()
        {
            foreach (MusicResult musicResult in runningAudioSources)
                musicResult.fadeOutAudioSource = true;
        }

        private MusicResult PrepareAudioInstance(int index, bool useDynamicList)
        {
            MusicEntry musicEntry = null;

            if (!useDynamicList && index != -1)
            {
                musicEntry = musicEntries[index];
            }
            else if (useDynamicList && index != -1)
            {
                musicEntry = dynamicMusicEntries[index];
            }
            else
            {
                switch (audioContainerGlobalSettingsData.selectNextType)
                {
                    case SelectNextType.None:
                        break;

                    case SelectNextType.SequentialType:
                        index = SelectNext(musicEntries);
                        break;

                    case SelectNextType.RandomType:
                        index = SelectNextRandomly(musicEntries);
                        break;

                    case SelectNextType.RandomButPickOnce:
                        index = SelectNextRandomlyButPickOnce(musicEntries);
                        break;

                    case SelectNextType.RandomWeightedType:
                        index = SelectNextRandomlyWeighted(musicEntries);
                        break;

                    case SelectNextType.RandomDynamicWeightForFairness:
                        index = SelectNextRandomlyDynamicWeightedForFairness(musicEntries);
                        break;

                    default:
                        break;
                }

                if (index != -1)
                    musicEntry = musicEntries[index];
            }

            if (musicEntry == null)
                return null;

            string template = "";
            if (audioContainerGlobalSettingsData.overrideTemplate)
                template = audioContainerGlobalSettingsData.audioSourceTemplate;

            var audioSourceGameObject = AudioPoolManager.Instance.GetObject(template);
            if (audioSourceGameObject == null)
                return null;

            AudioSource currentAudioSource = audioSourceGameObject.GetComponent<AudioSource>();
            SetupAudioSource(ref currentAudioSource, musicEntry, useDynamicList);

            currentAudioSource.transform.parent = controller.audioListener.transform;
            currentAudioSource.transform.localPosition = Vector3.zero;
            currentAudioSource.transform.localRotation = Quaternion.identity;
            currentAudioSource.transform.localScale = Vector3.one;
            currentAudioSource.playOnAwake = false;

            string filter = "";
            if (audioContainerGlobalSettingsData.overrideFilter)
            {
                if (audioContainerGlobalSettingsData.useFilter)
                    filter = audioContainerGlobalSettingsData.filter;
                else
                    filter = "";
            }
            else filter = "";

            if (!string.IsNullOrEmpty(filter))
                FilterManager.Instance.SetFilter(currentAudioSource.gameObject, filter);
            else
                FilterManager.Instance.RemoveAllFilters(currentAudioSource.gameObject);

            return new MusicResult(currentAudioSource, musicEntry, currentAudioSource.volume);
        }

        public void SetupAudioSource(ref AudioSource audioSource, AudioEntry audioEntry, bool isDynamic)
        {
            audioSource.clip = audioEntry.audioFile;
            if (isDynamic)
            {
                MusicController musicController = controller as MusicController;
                audioSource.outputAudioMixerGroup = musicController.musicControllerSpecificSettingsData.mixerGroupForDynamic;
            }
            else
            {
                if (audioContainerGlobalSettingsData.overrideMixerGroup)
                    audioSource.outputAudioMixerGroup = audioContainerGlobalSettingsData.mixerGroup;
            }

            audioSource.loop = true;
            if (audioContainerGlobalSettingsData.overrideVolume)
                audioSource.volume = UnityEngine.Random.Range(audioContainerGlobalSettingsData.minVolume, audioContainerGlobalSettingsData.maxVolume);
            if (audioContainerGlobalSettingsData.overridePitch)
                audioSource.pitch = UnityEngine.Random.Range(audioContainerGlobalSettingsData.minPitch, audioContainerGlobalSettingsData.maxPitch);
        }

        private void PlayAudioSource(MusicResult musicResult)
        {
            musicResult.fadeOutAudioSource = false;
            AudioSource currentAudioSource = musicResult.audioSource;
            currentAudioSource.gameObject.SetActive(true);
            currentAudioSource.volume = _isSoundOn ? musicResult.audioSource.volume : 0.0f;
            currentAudioSource.Play();
        }

        #endregion Class Methods
    }

    [Serializable]
    public class AudioResult
    {
        #region Members

        public AudioSource audioSource;
        public bool fadeOutAudioSource = false;

        #endregion Members

        public AudioResult(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            fadeOutAudioSource = false;
        }

        public void Stop()
            => fadeOutAudioSource = true;
    }

    [Serializable]
    public class MusicResult : AudioResult
    {
        #region Members

        public MusicEntry musicEntry;
        public bool isAudioSourceReady = true;

        #endregion Members

        #region Properties

        public float LastVolume { get; private set; }

        #endregion Properties

        #region Class Methods

        public MusicResult(AudioSource audioSource, MusicEntry musicEntry, float initialVolume) : base(audioSource)
        {
            this.musicEntry = musicEntry;
            isAudioSourceReady = true;
            LastVolume = initialVolume;
        }

        #endregion Class Methods
    }
}