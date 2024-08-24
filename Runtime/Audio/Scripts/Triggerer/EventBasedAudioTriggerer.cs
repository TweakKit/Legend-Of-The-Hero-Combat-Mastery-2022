using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    public class EventBasedAudioTriggerer : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private List<EventEntry> _eventEntries;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.Awake)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void Start()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.Start)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnEnable()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnEnable)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnDisable()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnDisable)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnDestroy()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnDestroy)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnTriggerEnter)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnTriggerExit)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnCollisionEnter)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnCollisionExit)
                    eventEntry.PlayAudio(transform);
            }
        }

        private void OnApplicationQuit()
        {
            foreach (EventEntry eventEntry in _eventEntries)
            {
                if (eventEntry.triggerType == EventEntry.TriggerType.OnApplicationQuit)
                    eventEntry.PlayAudio(transform);
            }
        }
    }

    #endregion API Methods

    [Serializable]
    public class EventEntry
    {
        public enum TriggerType : int
        {
            Awake,
            Start,
            OnEnable,
            OnDisable,
            OnDestroy,
            OnTriggerEnter,
            OnTriggerExit,
            OnCollisionEnter,
            OnCollisionExit,
            OnApplicationQuit,
        }

        #region Properties

        public TriggerType triggerType = TriggerType.OnDestroy;
        public AudioType audioType = AudioType.SFX;
        public string audioName;
        public bool isDynamic;
        public bool attackOnThisTransform;
        public Transform attackToTransform;
        public bool playOnPosition;
        public Vector3 playPosition;

        #endregion Members

        #region Class Methods

        public void PlayAudio(Transform transform)
        {
            switch (audioType)
            {
                case AudioType.Music:
                    if (!isDynamic)
                    {
                        if (string.IsNullOrEmpty(audioName))
                            AudioController.Instance.PlayMusic();
                        else
                            AudioController.Instance.PlayMusic(audioName);
                    }
                    else AudioController.Instance.PlayDynamicMusic(audioName);
                    break;

                case AudioType.SFX:
                    if (attackOnThisTransform)
                        AudioController.Instance.PlaySoundEffect(audioName, transform);
                    else if (attackToTransform != null)
                        AudioController.Instance.PlaySoundEffect(audioName, attackToTransform);
                    else if (playOnPosition)
                        AudioController.Instance.PlaySoundEffect(audioName, playPosition);
                    else
                        AudioController.Instance.PlaySoundEffect(audioName);
                    break;

                default:
                    break;
            }
        }

        #endregion Class Methods
    }
}