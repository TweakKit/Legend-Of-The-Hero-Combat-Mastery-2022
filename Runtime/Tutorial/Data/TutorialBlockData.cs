using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockData
    {
        #region Members

        [ReadOnly]
        public int sequenceIndex;
        [ReadOnly]
        public int blockIndex;
        public string name;
        public TutorialTriggerData startTriggerData;
        public TutorialTriggerData endTriggerData;
        public TutorialSwitchData switchData;
        public TutorialEventData[] eventsData;
        public TutorialBlockTargetData[] focusTargetsData;
        [SerializeReference]
        public TutorialBlockIndicatorData[] blockIndicatorsData;
        private Dictionary<EventType, TutorialEventData> _eventsDictionary;

        #endregion Members

        #region Properties

        public Dictionary<EventType, TutorialEventData> EventsDictionary
        {
            get { return _eventsDictionary; }
        }

        #endregion Properties

        #region Class Methods

#if UNITY_EDITOR
        public void ValidateData(int sequenceIndex, int blockIndex)
        {
            this.sequenceIndex = sequenceIndex;
            this.blockIndex = blockIndex;
        }
#endif

        public void Init(int sequenceIndex, int blockIndex)
        {
            this.sequenceIndex = sequenceIndex;
            this.blockIndex = blockIndex;
            for (int i = 0; i < blockIndicatorsData.Length; i++)
            {
                blockIndicatorsData[i].SequenceIndex = sequenceIndex;
                this.blockIndicatorsData[i].BlockIndex = blockIndex;
            }
        }

        public void UpdateTargetScreenRect()
        {
            for (int i = 0; i < focusTargetsData.Length; i++)
            {
                if (focusTargetsData[i].runtimeTarget != null && focusTargetsData[i].runtimeTarget.activeInHierarchy)
                    focusTargetsData[i].UpdateTargetScreenRect();
            }
        }

        public void InitializedEvents()
        {
            if (_eventsDictionary == null)
                _eventsDictionary = new Dictionary<EventType, TutorialEventData>();
            _eventsDictionary.Clear();

            for (int i = 0; i < eventsData.Length; i++)
            {
                if (!_eventsDictionary.ContainsKey(eventsData[i].type))
                    _eventsDictionary.Add(eventsData[i].type, eventsData[i]);
            }
        }

        #endregion Class Methods
    }
}