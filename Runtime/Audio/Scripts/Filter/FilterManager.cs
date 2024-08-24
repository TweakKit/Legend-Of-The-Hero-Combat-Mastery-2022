using System.Collections.Generic;
using UnityEngine;
using Runtime.Core.Singleton;

namespace Runtime.Audio
{
    public class FilterManager : MonoSingleton<FilterManager>
    {
        #region Members

        private Dictionary<string, FilterGroup> _filterGroupsDictionary;

        #endregion Members

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _filterGroupsDictionary = new Dictionary<string, FilterGroup>();
            FilterGroup[] filterGroups = gameObject.GetComponentsInChildren<FilterGroup>();
            foreach (FilterGroup filterGroup in filterGroups)
            {
                filterGroup.Init();
                if (string.IsNullOrEmpty(filterGroup.gameObject.name))
                    continue;
                _filterGroupsDictionary.Add(filterGroup.gameObject.name, filterGroup);
            }
        }

        #endregion API Methods

        #region Class Methods

        public bool SetFilter(GameObject gameObject, string filterName)
        {
            if (!_filterGroupsDictionary.ContainsKey(filterName))
            {
                Debug.LogError("AudioController, filter with this name does not exist: " + filterName);
                return false;
            }

            RemoveAllFilters(gameObject);

            FilterGroup filterGroup = _filterGroupsDictionary[filterName];
            foreach (KeyValuePair<int, FilterEntry> kvp in filterGroup.OrderedFilterEntriesDictionary)
            {
                FilterEntry filterEntry = kvp.Value;
                filterEntry.AddFilterTo(gameObject);
            }

            return true;
        }

        public void RemoveAllFilters(GameObject gameObject)
        {
            if (gameObject.GetComponent<AudioChorusFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioChorusFilter>());

            if (gameObject.GetComponent<AudioDistortionFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioDistortionFilter>());

            if (gameObject.GetComponent<AudioEchoFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioEchoFilter>());

            if (gameObject.GetComponent<AudioLowPassFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioLowPassFilter>());

            if (gameObject.GetComponent<AudioReverbFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioReverbFilter>());

            if (gameObject.GetComponent<AudioHighPassFilter>())
                DestroyImmediate(gameObject.GetComponent<AudioHighPassFilter>());
        }

        #endregion Class Methods
    }
}