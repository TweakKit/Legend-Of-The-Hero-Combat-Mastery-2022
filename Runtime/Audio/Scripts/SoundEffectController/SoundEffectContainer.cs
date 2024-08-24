using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    [System.Serializable]
    public class SoundEffectContainer : AudioContainer
    {
        #region Members

        public List<SoundEntry> soundEntries;
        private Dictionary<string, SoundEntry> _audioEntriesDictionary = new Dictionary<string, SoundEntry>();

        #endregion Members

        #region Properties

        private SoundEffect SoundEffect { get; set; }
        private bool IsLoop { get; set; } = false;
        private bool PickAutomaticallyNext { get; set; } = false;

        #endregion Properties

        #region Class Methods

        public void SetSoundEffect(SoundEffect soundEffect)
            => SoundEffect = soundEffect;

        public void SetSFXSoundLoopStatus(bool isLoop)
            => IsLoop = isLoop;

        public void SetPickAutomaticallyNextStatus(bool pickAutomaticallyNext)
            => PickAutomaticallyNext = pickAutomaticallyNext;

        public SoundResult Play(Vector3 position)
        {
            SoundResult soundResult = PrepareAudioInstance(-1);
            if (soundResult != null)
            {
                soundResult.fadeOutAudioSource = false;
                soundResult.audioSource.transform.parent = SoundEffect.transform;
                soundResult.audioSource.transform.position = position;
                soundResult.audioSource.transform.localRotation = Quaternion.identity;
                soundResult.audioSource.transform.localScale = Vector3.one;
                runningAudioSources.Add(soundResult);
                AudioController.Instance.StartCoroutine(PlayAudioSource(soundResult));
            }

            return soundResult;
        }

        public SoundResult Play(GameObject gameObject)
            => Play(gameObject.transform);

        public SoundResult Play(Transform transform)
        {
            SoundResult soundResult = PrepareAudioInstance(-1);
            if (soundResult != null)
            {
                soundResult.fadeOutAudioSource = false;
                soundResult.audioSource.transform.parent = SoundEffect.transform;
                soundResult.attachedOnTransform = transform;
                SoundEffect.Controller.RegisterAttachedSound(soundResult);

                soundResult.audioSource.transform.localPosition = Vector3.zero;
                soundResult.audioSource.transform.localRotation = Quaternion.identity;
                soundResult.audioSource.transform.localScale = Vector3.one;

                runningAudioSources.Add(soundResult);
                AudioController.Instance.StartCoroutine(PlayAudioSource(soundResult));
            }

            return soundResult;
        }

        public SoundResult Play()
        {
            SoundResult soundResult = PrepareAudioInstance(-1);
            if (soundResult != null)
            {
                soundResult.fadeOutAudioSource = false;
                soundResult.audioSource.transform.parent = controller.audioListener.transform;
                soundResult.audioSource.transform.localPosition = Vector3.zero;
                soundResult.audioSource.transform.localRotation = Quaternion.identity;
                soundResult.audioSource.transform.localScale = Vector3.one;

                runningAudioSources.Add(soundResult);
                AudioController.Instance.StartCoroutine(PlayAudioSource(soundResult));
            }

            return soundResult;
        }

        private SoundResult PrepareAudioInstance(int predefinedIndex)
        {
            int index = 0;
            SoundEntry soundEntry = null;
            if (predefinedIndex != -1)
            {
                index = predefinedIndex;
                soundEntry = soundEntries[index];
                predefinedIndex = -1;
            }
            else
            {
                switch (audioContainerGlobalSettingsData.selectNextType)
                {
                    case SelectNextType.None:
                        break;

                    case SelectNextType.SequentialType:
                        index = SelectNext(soundEntries);
                        break;

                    case SelectNextType.RandomType:
                        index = SelectNextRandomly(soundEntries);
                        break;

                    case SelectNextType.RandomButPickOnce:
                        index = SelectNextRandomlyButPickOnce(soundEntries);
                        break;

                    case SelectNextType.RandomWeightedType:
                        index = SelectNextRandomlyWeighted(soundEntries);
                        break;

                    case SelectNextType.RandomDynamicWeightForFairness:
                        index = SelectNextRandomlyDynamicWeightedForFairness(soundEntries);
                        break;

                    default:
                        break;
                }

                if (index != -1)
                    soundEntry = soundEntries[index];
            }

            if (soundEntry == null)
                return null;

            string template = "";
            if (audioContainerGlobalSettingsData.overrideTemplate)
                template = audioContainerGlobalSettingsData.audioSourceTemplate;

            var audioSourceGameObject = AudioPoolManager.Instance.GetObject(template);
            if (audioSourceGameObject == null)
                return null;

            AudioSource currentAudioSource = audioSourceGameObject.GetComponent<AudioSource>();
            SetupAudioSource(ref currentAudioSource, soundEntry);
            float currentVolume = currentAudioSource.volume;
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

            return new SoundResult(currentAudioSource, soundEntry, SoundEffect.Controller, currentVolume);
        }

        public override void Mute()
        {
            base.Mute();
            foreach (SoundResult sourceResult in runningAudioSources)
                sourceResult.audioSource.volume = 0.0f;
        }

        public override void Unmute()
        {
            base.Unmute();
            foreach (SoundResult sourceResult in runningAudioSources)
                sourceResult.audioSource.volume = sourceResult.CurrentVolume;
        }

        public override void Stop()
        {
            foreach (SoundResult sourceResult in runningAudioSources)
                sourceResult.fadeOutAudioSource = true;
        }

        public override void Init(BaseAudioController controller)
        {
            base.Init(controller);
            InitNextSelection(soundEntries);

            _audioEntriesDictionary.Clear();
            foreach (SoundEntry entry in soundEntries)
            {
                if (string.IsNullOrEmpty(entry.audioName))
                {
                    if (entry.audioFile == null)
                        continue;
                    else
                        entry.audioName = entry.audioFile.name;
                }
                _audioEntriesDictionary.Add(entry.audioName, entry);

            }
        }

        public void SetupAudioSource(ref AudioSource audioSource, AudioEntry audioEntry)
        {
            audioSource.clip = audioEntry.audioFile;
            if (audioContainerGlobalSettingsData.overrideMixerGroup)
                audioSource.outputAudioMixerGroup = audioContainerGlobalSettingsData.mixerGroup;

            audioSource.loop = IsLoop;
            if (audioContainerGlobalSettingsData.overrideVolume)
                audioSource.volume = UnityEngine.Random.Range(audioContainerGlobalSettingsData.minVolume, audioContainerGlobalSettingsData.maxVolume);
            if (audioContainerGlobalSettingsData.overridePitch)
                audioSource.pitch = UnityEngine.Random.Range(audioContainerGlobalSettingsData.minPitch, audioContainerGlobalSettingsData.maxPitch);
        }

        private IEnumerator PlayAudioSource(SoundResult soundResult)
        {
            soundResult.fadeOutAudioSource = false;
            AudioSource currentAudioSource = soundResult.audioSource;
            SoundEntry soundEntry = soundResult.soundEntry;
            currentAudioSource.gameObject.SetActive(true);
            float audioLength = soundEntry.audioFile.length / Mathf.Abs(currentAudioSource.pitch);
            currentAudioSource.volume = soundResult.CurrentVolume;
            currentAudioSource.Play();

            yield return new WaitForSeconds(audioLength);
            if (currentAudioSource != null)
            {
                SoundEffect.Controller.UnregisterAttachedSound(soundResult);
                AudioPoolManager.Instance.ReturnObject(currentAudioSource.gameObject);
            }

            runningAudioSources.Remove(soundResult);
            soundResult.audioSource = null;
            soundResult.attachedOnTransform = null;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class SoundResult : AudioResult
    {
        #region Members

        public SoundEntry soundEntry;
        public Transform attachedOnTransform;
        public SoundEffectController controller;

        #endregion Members

        #region Properties

        public float CurrentVolume { get; private set; }

        #endregion Properties

        #region Class Methods

        public SoundResult(AudioSource audioSource, SoundEntry soundEntry, SoundEffectController controller, float initialVolume) : base(audioSource)
        {
            this.soundEntry = soundEntry;
            this.controller = controller;
            CurrentVolume = initialVolume;
        }

        public void UpdatePosition()
        {
            if (attachedOnTransform != null)
            {
                audioSource.transform.position = attachedOnTransform.position;
            }
            else
            {
                attachedOnTransform = null;
                controller.UnregisterAttachedSound(this);
                fadeOutAudioSource = true;
            }
        }

        #endregion Class Methods
    }
}