using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialSequenceData
    {
        #region Members

        [ReadOnly]
        public int sequenceIndex;
        public string sequenceName;
        public bool markTutCompletedAsSequenceSaved;
        public TutorialTriggerData startTriggerData;
        public TutorialTriggerData endTriggerData;
        public TutorialSwitchData switchData;
        public TutorialEventData[] eventsData;
        public TutorialBlockData[] tutorialBlocksData;
        [HideInInspector]
        public PlayState state;
        private Dictionary<EventType, TutorialEventData> _eventsDictionary;

        #endregion Members

        #region Properties

        public bool CanSkip { get { return false; } }
        public int CurrentStep { get; set; }

        public Dictionary<EventType, TutorialEventData> EventsDictionary
        {
            get { return _eventsDictionary; }
        }

        #endregion Properties

        #region Class Methods

#if UNITY_EDITOR
        public void ValidateData(int sequenceIndex)
        {
            this.sequenceIndex = sequenceIndex;
            if (tutorialBlocksData != null)
                for (int i = 0; i < tutorialBlocksData.Length; i++)
                    tutorialBlocksData[i].ValidateData(sequenceIndex, i + 1);
        }
#endif

        public void Init(int sequenceIndex)
        {
            this.sequenceIndex = sequenceIndex;
            for (int i = 0; i < tutorialBlocksData.Length; i++)
                tutorialBlocksData[i].Init(sequenceIndex, i + 1);

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