using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Runtime.Audio
{
    public enum SelectNextType
    {
        None,
        SequentialType,
        RandomType,
        RandomButPickOnce,
        RandomWeightedType,
        RandomDynamicWeightForFairness
    };

    [Serializable]
    public class AudioContainerGlobalSettingsData
    {
        #region Members

        public bool overrideTemplate = false;
        public string audioSourceTemplate = "AudioSource";

        public bool overrideMixerGroup = false;
        public AudioMixerGroup mixerGroup;

        public bool overrideFilter = false;
        public bool useFilter = false;
        public string filter;

        public bool overrideVolume = false;
        [Range(0f, 1f)]
        public float minVolume = 1;
        [Range(0f, 1f)]
        public float maxVolume = 1;

        public bool overridePitch = false;
        [Range(-3f, 3f)]
        public float minPitch = 1;
        [Range(-3f, 3f)]
        public float maxPitch = 1;

        public float fadeInTime = 3;
        public float fadeOutTime = 3;

        public float minDelayedStartTime = 1;
        public float maxDelayedStartTime = 10;

        public SelectNextType selectNextType = SelectNextType.RandomButPickOnce;

        #endregion Members
    }

    [Serializable]
    public class AudioContainer
    {
        #region Members

        [Header("=== AUDIO CONTAINER GLOBAL SETTINGS DATA ===")]
        public AudioContainerGlobalSettingsData audioContainerGlobalSettingsData;

        protected int lastIndex = -1;
        protected Dictionary<int, bool> playAudioIndexesDictionary = new Dictionary<int, bool>();
        protected Dictionary<AudioEntry, int> weightModificatorsDictionary = new Dictionary<AudioEntry, int>();
        protected BaseAudioController controller = null;
        protected List<AudioResult> runningAudioSources = new List<AudioResult>();

        #endregion Members

        #region Class Methods

        public virtual void Stop() { }
        public virtual void Stop(int id) { }
        public virtual void Mute() { }
        public virtual void Unmute() { }
        public virtual void Pause() => Mute();
        public virtual void Continue() => Unmute();

        public virtual void Init(BaseAudioController controller)
            => this.controller = controller;

        public void InitNextSelection<T>(List<T> audioEntries) where T : AudioEntry
        {
            InitPlayAudioIndexesDictionary(audioEntries);
            CreateWeightModificator(audioEntries);
        }

        protected void InitPlayAudioIndexesDictionary<T>(List<T> audioEntries) where T : AudioEntry
        {
            playAudioIndexesDictionary.Clear();
            if (audioEntries.Count > 0)
            {
                for (int i = 0; i < audioEntries.Count; i++)
                    playAudioIndexesDictionary.Add(i, false);
            }
        }

        protected void CreateWeightModificator<T>(List<T> audioEntries) where T : AudioEntry
        {
            weightModificatorsDictionary.Clear();
            if (audioEntries.Count > 0)
            {
                foreach (var entry in audioEntries)
                    weightModificatorsDictionary.Add(entry, 0);
            }
        }

        protected int SelectNext<T>(List<T> audioEntries) where T : AudioEntry
        {
            if (audioEntries.Count == 0)
                return -1;

            int nextIndex = 0;
            if (lastIndex < audioEntries.Count - 1)
                nextIndex = Mathf.Min(nextIndex + 1, audioEntries.Count - 1);
            else
                nextIndex = 0;

            lastIndex = nextIndex;
            return nextIndex;
        }

        protected int SelectNextRandomly<T>(List<T> audioEntries) where T : AudioEntry
        {
            if (audioEntries.Count == 0)
                return -1;

            int nextIndex = UnityEngine.Random.Range(0, audioEntries.Count);
            if (nextIndex == lastIndex && audioEntries.Count > 1)
            {
                if (nextIndex < audioEntries.Count - 1)
                    nextIndex++;
                else
                    nextIndex--;
            }

            lastIndex = nextIndex;
            return nextIndex;
        }

        protected int SelectNextRandomlyButPickOnce<T>(List<T> audioEntries) where T : AudioEntry
        {
            if (audioEntries.Count == 0)
                return -1;

            int nextIndex = UnityEngine.Random.Range(0, audioEntries.Count);
            bool firstSong = false;
            if (playAudioIndexesDictionary.Count == 0)
            {
                InitPlayAudioIndexesDictionary(audioEntries);
                firstSong = true;
            }

            bool indexSearchDirection = UnityEngine.Random.value > 0.5f;
            for (int i = 0; i < audioEntries.Count; i++)
            {
                if (!(firstSong && nextIndex == lastIndex) && playAudioIndexesDictionary.ContainsKey(nextIndex))
                    break;

                if (indexSearchDirection)
                {
                    if (nextIndex < audioEntries.Count - 1)
                        nextIndex++;
                    else
                        nextIndex = 0;
                }
                else
                {
                    if (nextIndex > 0)
                        nextIndex--;
                    else
                        nextIndex = audioEntries.Count - 1;
                }
            }

            playAudioIndexesDictionary.Remove(nextIndex);
            lastIndex = nextIndex;
            return nextIndex;
        }

        protected int SelectNextRandomlyWeighted<T>(List<T> audioEntries) where T : AudioEntry
        {
            if (audioEntries.Count == 0)
                return -1;

            List<T> entries = new List<T>();
            entries.AddRange(audioEntries);
            if (lastIndex != -1)
                entries.RemoveAt(lastIndex);

            float randomValue = UnityEngine.Random.value;
            int maxWeight = 0;
            int weightIndex = Mathf.RoundToInt(maxWeight * randomValue);
            int nextIndex = 0;
            maxWeight = 0;
            foreach (T entry in entries)
            {
                if (lastIndex != nextIndex && weightIndex < maxWeight)
                    break;
                nextIndex++;
            }

            nextIndex = audioEntries.IndexOf(entries[Mathf.Min(nextIndex, entries.Count - 1)]);
            lastIndex = nextIndex;
            return nextIndex;
        }

        protected int SelectNextRandomlyDynamicWeightedForFairness<T>(List<T> audioEntries) where T : AudioEntry
        {
            if (audioEntries.Count == 0)
                return -1;

            if (playAudioIndexesDictionary.Count == 0)
            {
                List<AudioEntry> keys = new List<AudioEntry>(weightModificatorsDictionary.Keys);
                foreach (T key in keys)
                    weightModificatorsDictionary[key] = 0;
                InitPlayAudioIndexesDictionary(audioEntries);
            }

            List<T> entries = new List<T>();
            entries.AddRange(audioEntries);
            if (lastIndex != -1)
                entries.RemoveAt(lastIndex);

            for (int i = entries.Count - 1; i >= 0; i--)
            {
                if (Mathf.Max(weightModificatorsDictionary[entries[i]], 0) == 0)
                    entries.RemoveAt(i);
            }

            float randomValue = UnityEngine.Random.value;
            int maxWeight = 0;
            foreach (T entry in entries)
                maxWeight += Mathf.Max(weightModificatorsDictionary[entry], 0);

            int weightIndex = Mathf.RoundToInt(maxWeight * randomValue);
            int nextIndex = 0;
            maxWeight = 0;
            T currentEntry = null;
            foreach (T entry in entries)
            {
                maxWeight += Mathf.Max(weightModificatorsDictionary[entry], 0);
                currentEntry = entry;
                if (lastIndex != nextIndex && weightIndex < maxWeight)
                    break;
                nextIndex++;
            }

            if (entries.Count == 0)
            {
                nextIndex = UnityEngine.Random.Range(0, audioEntries.Count);
                currentEntry = audioEntries[nextIndex];
            }
            else nextIndex = audioEntries.IndexOf(entries[Mathf.Min(nextIndex, entries.Count - 1)]);

            lastIndex = nextIndex;
            weightModificatorsDictionary[currentEntry] = Mathf.Max(weightModificatorsDictionary[currentEntry] - 1, 0);

            if (playAudioIndexesDictionary.ContainsKey(lastIndex))
                playAudioIndexesDictionary.Remove(lastIndex);

            return nextIndex;
        }

        #endregion Class Methods
    }
}