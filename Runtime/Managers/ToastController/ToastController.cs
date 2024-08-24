using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Runtime.Core.Singleton;
using Cysharp.Threading.Tasks;

namespace Runtime.Manager
{
    public enum VisualToastType
    {
        Normal = 0,
    }

    public enum PositionToastType
    {
        Middle = 0,
        Top = 1,
        Bottom = 2,
    }

    public class ToastController : MonoSingleton<ToastController>
    {
        #region Members

        [SerializeField]
        private Transform _bottomPosition;
        [SerializeField]
        private Transform _topPosition;
        [SerializeField]
        private Transform _middlePosition;
        private Dictionary<VisualToastType, ToastQueue> _toastQueueDictionary;

        #endregion Members

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _toastQueueDictionary = new();
        }

        #endregion API Methods

        #region Class Methods

        public void Show(string content, PositionToastType positionToastType = PositionToastType.Middle, VisualToastType visualToastType = VisualToastType.Normal)
            => LoadToastAsync(content, positionToastType, visualToastType).Forget();

        private async UniTask LoadToastAsync(string content, PositionToastType positionToastType = PositionToastType.Middle, VisualToastType visualToastType = VisualToastType.Normal)
        {
            var result = _toastQueueDictionary.TryGetValue(visualToastType, out var toastQueue);
            if (!result)
            {
                var prefabId = $"toast_{visualToastType.ToString().ToLower()}";
                var prefab = await Addressables.LoadAssetAsync<GameObject>(prefabId).WithCancellation(this.GetCancellationTokenOnDestroy());
                toastQueue = new ToastQueue(prefab.GetComponent<Toast>());
                _toastQueueDictionary.TryAdd(visualToastType, toastQueue);
            }

            var dequeueResult = toastQueue.queue.TryDequeue(out var toast);
            if (!dequeueResult)
                toast = Instantiate(toastQueue.prefab);

            Transform positionTransform;
            switch (positionToastType)
            {
                case PositionToastType.Middle:
                    positionTransform = _middlePosition;
                    break;
                case PositionToastType.Top:
                    positionTransform = _topPosition;
                    break;
                case PositionToastType.Bottom:
                    positionTransform = _bottomPosition;
                    break;
                default:
                    positionTransform = _middlePosition;
                    break;
            }

            toast.Init(content, positionTransform, ReturnPool);
        }

        private void ReturnPool(Toast toast)
        {
            var result = _toastQueueDictionary.TryGetValue(toast.VisualToastType, out var toastQueue);
            if (result)
                toastQueue.queue.Enqueue(toast);
        }

        #endregion Class Methods

        #region Owner Classes

        private class ToastQueue
        {
            #region Members

            public Toast prefab;
            public Queue<Toast> queue;

            #endregion Members

            #region Class Methods

            public ToastQueue(Toast prefab)
            {
                this.prefab = prefab;
                queue = new Queue<Toast>();
            }

            #endregion Class Methods
        }

        #endregion Owner Classes
    }
}