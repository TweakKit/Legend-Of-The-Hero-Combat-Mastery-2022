using System.Collections.Generic;
using UnityEngine;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.UI
{
    public class ScreenTouchEffectController : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private int _initialSpawnCount = 5;
        [SerializeField]
        private ScreenTouchEffect _screenTouchEffectPrefab;
        private Registry<ScreenGotTouchedMessage> _screenGotTouchedRegistry;
        private Queue<ScreenTouchEffect> _screenTouchEffectsQueue;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _screenGotTouchedRegistry = Messenger.Subscriber().Subscribe<ScreenGotTouchedMessage>(OnScreenGotTouched);
            Initialize();
        }

        private void OnDestroy()
        {
            _screenGotTouchedRegistry.Dispose();
            CleanPool();
        }

        #endregion API Methods

        #region Class Methods

        private void OnScreenGotTouched(ScreenGotTouchedMessage screenGotTouchedMessage)
            => DisplayScreenTouchEffect(screenGotTouchedMessage.TouchPosition);

        private void Initialize()
        {
            _screenTouchEffectsQueue = new Queue<ScreenTouchEffect>();
            CreatePool();
        }

        private void DisplayScreenTouchEffect(Vector2 screenPosition)
        {
            if (_screenTouchEffectsQueue.Count <= 0)
                CreatePool();

            var displayedScreenTouchEffect = _screenTouchEffectsQueue.Dequeue();
            displayedScreenTouchEffect.transform.position = screenPosition;
            displayedScreenTouchEffect.SetVisibility(true);
        }

        private void CreatePool()
        {
            for (int i = 0; i < _initialSpawnCount; i++)
            {
                ScreenTouchEffect screenTouchEffect = Instantiate(_screenTouchEffectPrefab, transform, true);
                screenTouchEffect.Init(ReturnToPool);
                _screenTouchEffectsQueue.Enqueue(screenTouchEffect);
            }
        }

        private void ReturnToPool(ScreenTouchEffect screentouchEffect)
        {
            screentouchEffect.SetVisibility(false);
            _screenTouchEffectsQueue.Enqueue(screentouchEffect);
        }

        private void CleanPool()
        {
            while (_screenTouchEffectsQueue.Count > 0)
            {
                ScreenTouchEffect screenTouchEffect = _screenTouchEffectsQueue.Dequeue();
                Destroy(screenTouchEffect.gameObject);
            }
            _screenTouchEffectsQueue.Clear();
            _screenTouchEffectsQueue = null;
        }

        #endregion Class Methods
    }
}