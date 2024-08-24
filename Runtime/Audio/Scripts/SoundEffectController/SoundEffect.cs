using System.Collections;
using UnityEngine;

namespace Runtime.Audio
{
    public enum RepeatType
    {
        Once,
        Continuously,
        Loop,
        RepeatAfterPlayed
    }

    public enum PlayType
    {
        PlayType,
        PlayOnPositionType,
        PlayOnGameObjectType
    };

    public class SoundEffect : MonoBehaviour
    {
        #region Members

        public RepeatType repeatType = RepeatType.Once;
        public float minRepeatTime = 2;
        public float maxRepeatTime = 5;
        public AnimationCurve repeatWeight = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public int maxInstances = 3;
        public SoundEffectContainer soundEffectContainer;
        private Coroutine _playingCoroutine = null;

        #endregion Members

        #region Properties

        public SoundEffectController Controller { get; private set; }

        #endregion Properties

        #region API Methods

        private void OnEnable()
        {
            Controller = transform.parent.GetComponent<SoundEffectController>();
            if (Controller == null)
                Debug.LogError("SoundEffectController has not been attached under the AudioController.");
            Controller.Register(this);
        }

        private void OnDisable()
            => Controller.Unregister(this);

        #endregion API Methods

        #region Class Methods

        public void Init(BaseAudioController controller)
        {
            if (soundEffectContainer != null)
            {
                soundEffectContainer.SetSoundEffect(this);
                soundEffectContainer.Init(controller);
            }
        }

        public SoundResult Play(Vector3 position)
            => Play(position, null, PlayType.PlayOnPositionType);

        public SoundResult Play(GameObject gameObject)
            => Play(Vector3.zero, gameObject, PlayType.PlayOnGameObjectType);

        public SoundResult Play(Transform transform)
            => Play(Vector3.zero, transform.gameObject, PlayType.PlayOnGameObjectType);

        public SoundResult Play()
            => Play(Vector3.zero, null, PlayType.PlayType);

        public SoundResult Play(Vector3 position, GameObject gameObject, PlayType type)
        {
            if (_playingCoroutine == null)
            {
                if (maxRepeatTime > 0 && repeatType == RepeatType.Continuously)
                {
                    _playingCoroutine = StartCoroutine(PlayCoroutine(position, gameObject, type));
                }
                else
                {
                    if (repeatType == RepeatType.Loop)
                        soundEffectContainer.SetSFXSoundLoopStatus(true);
                    else
                        soundEffectContainer.SetSFXSoundLoopStatus(false);

                    if (repeatType == RepeatType.RepeatAfterPlayed)
                        soundEffectContainer.SetPickAutomaticallyNextStatus(true);
                    else
                        soundEffectContainer.SetPickAutomaticallyNextStatus(false);

                    switch (type)
                    {
                        case PlayType.PlayType:
                            return soundEffectContainer.Play();

                        case PlayType.PlayOnPositionType:
                            return soundEffectContainer.Play(position);

                        case PlayType.PlayOnGameObjectType:
                            return soundEffectContainer.Play(gameObject);

                        default:
                            break;
                    }
                }
            }

            return null;
        }

        private IEnumerator PlayCoroutine(Vector3 position, GameObject gameObject, PlayType type)
        {
            while (true)
            {
                float normalized = repeatWeight.Evaluate((Random.Range(minRepeatTime, maxRepeatTime + 1)) / maxRepeatTime);
                float weightedValue = normalized * maxRepeatTime;
                yield return new WaitForSeconds(weightedValue);

                if (transform.childCount < maxInstances)
                {
                    soundEffectContainer.SetSFXSoundLoopStatus(false);
                    switch (type)
                    {
                        case PlayType.PlayType:
                            soundEffectContainer.Play();
                            break;

                        case PlayType.PlayOnPositionType:
                            soundEffectContainer.Play(position);
                            break;

                        case PlayType.PlayOnGameObjectType:
                            soundEffectContainer.Play(gameObject);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        public void Stop()
        {
            if (_playingCoroutine != null)
            {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }
            soundEffectContainer.Stop();
        }

        public void Mute()
            => soundEffectContainer.Mute();

        public void Unmute()
            => soundEffectContainer.Unmute();

        public void Pause()
            => soundEffectContainer.Pause();

        public void Continue()
            => soundEffectContainer.Continue();

        #endregion Class Methods
    }
}