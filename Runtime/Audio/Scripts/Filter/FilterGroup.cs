using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    public class FilterGroup : MonoBehaviour
    {
        #region Members

        [Header("=== REVERB FILTER ===")]
        public bool enableReverbFilter = false;
        [Range(0, 5)]
        public int orderReverbFilter = 0;
        public FilterEntryReverb reverbFilter;

        [Header("=== LOW PASS FILTER ===")]
        public bool enableLowPassFilter = false;
        [Range(0, 5)]
        public int orderLowPassFilter = 1;
        public FilterEntryLowPass lowPassFilter;

        [Header("=== HIGH PASS FILTER ===")]
        public bool enableHighPassFilter = false;
        [Range(0, 5)]
        public int orderHighPassFilter = 2;
        public FilterEntryHighPass highPassFilter;

        [Header("=== Echo Filter ===")]
        public bool enableEchoFilter = false;
        [Range(0, 5)]
        public int orderEchoFilter = 3;
        public FilterEntryEcho echoFilter;

        [Header("=== CHORUS FILTER ===")]
        public bool enableChorusFilter = false;
        [Range(0, 5)]
        public int orderChorusFilter = 4;
        public FilterEntryChorus chorusFilter;

        [Header("=== DISTORTION FILTER ===")]
        public bool enableDistortionFilter = false;
        [Range(0, 5)]
        public int orderDistortionFilter = 5;
        public FilterEntryDistortion distortionFilter;

        #endregion Members

        #region Properties

        public SortedDictionary<int, FilterEntry> OrderedFilterEntriesDictionary { get; private set; } = new SortedDictionary<int, FilterEntry>();

        #endregion Properties

        #region Class Methods

        public void Init()
        {
            OrderedFilterEntriesDictionary.Clear();

            if (enableReverbFilter)
                OrderedFilterEntriesDictionary.Add(orderReverbFilter, reverbFilter);

            if (enableLowPassFilter)
                OrderedFilterEntriesDictionary.Add(orderLowPassFilter, lowPassFilter);

            if (enableHighPassFilter)
                OrderedFilterEntriesDictionary.Add(orderHighPassFilter, highPassFilter);

            if (enableEchoFilter)
                OrderedFilterEntriesDictionary.Add(orderEchoFilter, echoFilter);

            if (enableChorusFilter)
                OrderedFilterEntriesDictionary.Add(orderChorusFilter, chorusFilter);

            if (enableDistortionFilter)
                OrderedFilterEntriesDictionary.Add(orderDistortionFilter, distortionFilter);
        }

        #endregion Class Methods
    }

    [Serializable]
    public abstract class FilterEntry
    {
        #region Class Methods

        public abstract void AddFilterTo(GameObject gameObject);

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryReverb : FilterEntry
    {
        #region Members

        public int dryLevel = 0;
        public int room = 0;
        public int roomHF = 0;
        public int roomLF = 0;
        public float decayTime = 1;
        public float decayHFRatio = 0.5f;
        public int reflectionsLevel = -10000;
        public float reflectionsDelay = 0;
        public int reverbLevel = 0;
        public float reverbDelay = 0.04f;
        public int hFReference = 5000;
        public int lFReference = 250;
        public int diffusion = 100;
        public int density = 100;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioReverbFilter filter = gameObject.AddComponent<AudioReverbFilter>();
            filter.dryLevel = dryLevel;
            filter.room = room;
            filter.roomHF = roomHF;
            filter.roomLF = roomLF;
            filter.decayTime = decayTime;
            filter.decayHFRatio = decayHFRatio;
            filter.reflectionsLevel = reflectionsLevel;
            filter.reflectionsDelay = reflectionsDelay;
            filter.reverbLevel = reverbLevel;
            filter.reverbDelay = reverbDelay;
            filter.hfReference = hFReference;
            filter.lfReference = lFReference;
            filter.diffusion = diffusion;
            filter.density = density;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryLowPass : FilterEntry
    {
        #region Members

        public int cutoffFrequency = 5000;
        public float lowpassResonanceQ = 1;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioLowPassFilter filter = gameObject.AddComponent<AudioLowPassFilter>();
            filter.cutoffFrequency = cutoffFrequency;
            filter.lowpassResonanceQ = lowpassResonanceQ;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryHighPass : FilterEntry
    {
        #region Members

        public int cutoffFrequency = 5000;
        public float highpassResonanceQ = 1;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioHighPassFilter filter = gameObject.AddComponent<AudioHighPassFilter>();
            filter.cutoffFrequency = cutoffFrequency;
            filter.highpassResonanceQ = highpassResonanceQ;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryEcho : FilterEntry
    {
        #region Members

        public int delay = 500;
        public float decayRatio = 0.5f;
        public float wetMix = 1;
        public float dryMix = 1;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioEchoFilter filter = gameObject.AddComponent<AudioEchoFilter>();
            filter.delay = delay;
            filter.decayRatio = decayRatio;
            filter.wetMix = wetMix;
            filter.dryMix = dryMix;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryChorus : FilterEntry
    {
        #region Members

        public float dryMix = 0.5f;
        public float wetMix1 = 0.5f;
        public float wetMix2 = 0.5f;
        public float wetMix3 = 0.5f;
        public float delay = 40;
        public float rate = 0.8f;
        public float depth = 0.03f;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioChorusFilter filter = gameObject.AddComponent<AudioChorusFilter>();
            filter.dryMix = dryMix;
            filter.wetMix1 = wetMix1;
            filter.wetMix2 = wetMix2;
            filter.wetMix3 = wetMix3;
            filter.delay = delay;
            filter.rate = rate;
            filter.depth = depth;
        }

        #endregion Class Methods
    }

    [Serializable]
    public class FilterEntryDistortion : FilterEntry
    {
        #region Members

        public float distortionLevel = 0.5f;

        #endregion Members

        #region Class Methods

        public override void AddFilterTo(GameObject gameObject)
        {
            AudioDistortionFilter filter = gameObject.AddComponent<AudioDistortionFilter>();
            filter.distortionLevel = distortionLevel;
        }

        #endregion Class Methods
    }
}