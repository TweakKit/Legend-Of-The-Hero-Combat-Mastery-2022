using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Audio
{
    public class SoundEffectController : BaseAudioController
    {
        #region Members

        public bool disableFollowUpdate = false;
        public float minObjectFollowUpdate = 0.2f;
        public float maxObjectFollowUpdate = 0.3f;
        public List<SoundResult> attachedSoundResults = new List<SoundResult>();
        private Coroutine _checkCoroutine = null;
        private Dictionary<string, SoundEffect> _soundEffectsDictionary = new Dictionary<string, SoundEffect>();

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            if (_checkCoroutine == null && !disableFollowUpdate)
                _checkCoroutine = StartCoroutine(RunFollowUpdate());
        }

        private void OnDisable()
        {
            if (_checkCoroutine != null)
            {
                StopCoroutine(_checkCoroutine);
                _checkCoroutine = null;
            }
        }

        #endregion API Methods

        #region Class Methods

        public override void SetSoundStatus(bool isSoundOn)
        {
            base.SetSoundStatus(isSoundOn);
            if (isSoundOn)
                Unmute();
            else
                Mute();
        }

        public override void Pause()
        {
            foreach (SoundEffect soundEffect in _soundEffectsDictionary.Values)
                soundEffect.Pause();
        }

        public override void Continue()
        {
            foreach (SoundEffect soundEffect in _soundEffectsDictionary.Values)
                soundEffect.Continue();
        }

        public void Play(string name, Vector3 position)
        {
            if (!isSoundOn)
                return;

            if (!_soundEffectsDictionary.ContainsKey(name))
            {
                Debug.LogError("The audio entry does not exists for the key " + name);
                return;
            }
            _soundEffectsDictionary[name].Play(position);
        }

        public void Play(string name, GameObject gameObject)
        {
            if (!isSoundOn)
                return;

            if (!_soundEffectsDictionary.ContainsKey(name))
            {
                Debug.LogError("The audio entry does not exists for the key " + name);
                return;
            }
            _soundEffectsDictionary[name].Play(gameObject);
        }

        public void Play(string name, Transform transform)
        {
            if (!isSoundOn)
                return;

            if (!_soundEffectsDictionary.ContainsKey(name))
            {
                Debug.LogError("The audio entry does not exists for the key " + name);
                return;
            }
            _soundEffectsDictionary[name].Play(transform);
        }

        public void Play(string name)
        {
            if (!isSoundOn)
                return;

            if (!_soundEffectsDictionary.ContainsKey(name))
            {
                Debug.LogError("The audio entry does not exists for the key " + name);
                return;
            }
            _soundEffectsDictionary[name].Play();
        }

        public void Stop()
        {
            foreach (SoundEffect soundEffect in _soundEffectsDictionary.Values)
                soundEffect.Stop();
        }

        public void Mute()
        {
            foreach (SoundEffect soundEffect in _soundEffectsDictionary.Values)
                soundEffect.Mute();
        }

        public void Unmute()
        {
            foreach (SoundEffect soundEffect in _soundEffectsDictionary.Values)
                soundEffect.Unmute();
        }

        public void Register(SoundEffect soundEffect)
        {
            _soundEffectsDictionary.Add(soundEffect.name, soundEffect);
            soundEffect.Init(this);
        }

        public void Unregister(SoundEffect soundEffect)
            => _soundEffectsDictionary.Remove(soundEffect.name);

        public void RegisterAttachedSound(SoundResult soundResult)
        {
            if (!attachedSoundResults.Contains(soundResult))
                attachedSoundResults.Add(soundResult);
        }

        public void UnregisterAttachedSound(SoundResult soundResult)
        {
            if (attachedSoundResults.Contains(soundResult))
                attachedSoundResults.Remove(soundResult);
        }

        private IEnumerator RunFollowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minObjectFollowUpdate, maxObjectFollowUpdate));
                for (int i = attachedSoundResults.Count - 1; i >= 0; i--)
                {
                    SoundResult soundResult = attachedSoundResults[i];
                    if (soundResult != null)
                        soundResult.UpdatePosition();
                }
            }
        }

        #endregion Class Methods
    }
}