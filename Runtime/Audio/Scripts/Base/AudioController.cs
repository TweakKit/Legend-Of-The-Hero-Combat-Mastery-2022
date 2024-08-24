using UnityEngine;
using Runtime.Manager.Data;
using Runtime.Core.Singleton;
using Cysharp.Threading.Tasks;

namespace Runtime.Audio
{
    public class AudioController : MonoSingleton<AudioController>
    {
        #region Properties

        [SerializeField]
        private bool _waitForInitializer;
        [SerializeField]
        private MusicController _musicController;
        [SerializeField]
        private SoundEffectController _soundEffectController;
        private bool _isMusicOn;
        private bool _isSoundEffectOn;

        #endregion Properties

        #region API Methods

        private void Start()
        {
            if (!_waitForInitializer)
                InitializeAsync().Forget();
        }

        #endregion API Methods

        #region Class Methods

        public async UniTask InitializeAsync()
        {
            _isMusicOn = DataManager.Local.IsMusicOn;
            _isSoundEffectOn = DataManager.Local.IsSoundEffectOn;
            SetMusicStatus(_isMusicOn);
            SetSoundEffectStatus(_isSoundEffectOn);
            await UniTask.Yield();
            _musicController.Init();
            _soundEffectController.Init();
        }

        public void PlayMusic()
            => _musicController.Play();

        public void PlayMusic(string name)
            => _musicController.Play(name);

        public void PlayDynamicMusic(string name)
            => _musicController.PlayDynamic(name);

        public void PlaySoundEffect(string name, Vector3 position)
        {
            if (_isSoundEffectOn)
                _soundEffectController.Play(name, position);
        }

        public void PlaySoundEffect(string name, GameObject gameObject)
        {
            if (_isSoundEffectOn)
                _soundEffectController.Play(name, gameObject);
        }

        public void PlaySoundEffect(string name, Transform transform)
        {
            if (_isSoundEffectOn)
                _soundEffectController.Play(name, transform);
        }

        public void PlaySoundEffect(string name)
        {
            if (_isSoundEffectOn)
                _soundEffectController.Play(name);
        }

        public void Stop()
        {
            _musicController.Stop();
            _soundEffectController.Stop();
        }

        public void SetMusicStatus(bool isMusicOn)
        {
            _isMusicOn = isMusicOn;
            _musicController.SetSoundStatus(isMusicOn);
        }

        public void SetSoundEffectStatus(bool isSoundEffectOn)
        {
            _isSoundEffectOn = isSoundEffectOn;
            _soundEffectController.SetSoundStatus(isSoundEffectOn);
        }

        public void SetPaused(bool isPaused)
        {
            if (isPaused)
                Pause();
            else
                Continue();
        }

        public void Pause()
        {
            if (_isMusicOn)
                _musicController.Pause();

            if (_isSoundEffectOn)
                _soundEffectController.Pause();
        }

        public void Continue()
        {
            if (_isMusicOn)
                _musicController.Continue();

            if (_isSoundEffectOn)
                _soundEffectController.Continue();
        }

        #endregion Class Methods
    }
}