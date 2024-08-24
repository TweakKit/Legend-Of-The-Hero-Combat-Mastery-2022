using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Runtime.UI
{
    public class ScreenTouchEffect : MonoBehaviour
    {
        #region Members

        [SerializeField]
        [Min(0.001f)]
        private float _displayTime = 1.0f;

        #endregion Members

        #region Properties

        private Action<ScreenTouchEffect> EndEffect { get; set; }

        #endregion Properties

        #region API Methods

        protected virtual void OnEnable()
            => FinishDisplayAsync().Forget();

        #endregion API Methods

        #region Class Methods

        public void Init(Action<ScreenTouchEffect> endEffect)
        {
            EndEffect = endEffect;
            SetVisibility(false);
        }

        public void SetVisibility(bool isActive)
            => gameObject.SetActive(isActive);

        private async UniTask FinishDisplayAsync()
        {
            await UniTask.Delay((int)(_displayTime * 1000), ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
            EndEffect(this);
        }

        #endregion Class Methods
    }
}