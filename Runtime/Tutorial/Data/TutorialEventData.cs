using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialEventData
    {
        #region Members

        public EventType type;
        public UnityEvent subEvent;

        #endregion Members

        #region Class Methods

        public static TutorialEventData GetEvent(Dictionary<EventType, TutorialEventData> events, EventType type)
        {
            if (events != null && events.ContainsKey(type))
                return events[type];
            return null;
        }

        #endregion Class Methods
    }
}