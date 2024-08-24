using System;
using System.Collections;
using System.Threading;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

namespace UnityEngine.InputSystem.Custom
{
    public class PerfectActionHoldButton : PerfectActionButton
    {
        #region Members

        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region API Methods

        private void OnDisable()
            => _cancellationTokenSource?.Cancel();

        #endregion API Methods

        #region Class Methods

        public override void OnPointerUp(PointerEventData eventData)
        {
            _cancellationTokenSource?.Cancel();
            base.OnPointerUp(eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            base.OnPointerDown(eventData);
        }

        protected override IEnumerator InvokeOnClickAction()
        {
            yield return new WaitForSeconds(ProceedOnClickDelayTime);
            StartHoldAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid StartHoldAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                PlayerActionInput.Main.Trigger(buttonInputType);
                await UniTask.Delay(TimeSpan.FromSeconds(PreventSpamDelayTime), cancellationToken: cancellationToken);
            }
        }

        #endregion Class Methods
    }
}