using System.Collections;
using UnityEngine.EventSystems;
using Runtime.Core.Mics;

namespace UnityEngine.InputSystem.Custom
{
    public class PerfectActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IClickable
    {
        #region Members

        protected static readonly float ProceedOnClickDelayTime = 0.1f;
        protected static readonly float PreventSpamDelayTime = 0.15f;
        [SerializeField]
        protected ButtonInputType buttonInputType;
        protected bool blockInput = false;

        #endregion Members

        #region Class Methods

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!blockInput)
            {
                blockInput = true;
                Press();
                StartCoroutine(BlockInputTemporarily());
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData) { }

        public virtual void Click()
            => PlayerActionInput.Main.Trigger(buttonInputType);

        protected virtual bool Press()
        {
            StartCoroutine(InvokeOnClickAction());
            return true;
        }

        protected virtual IEnumerator InvokeOnClickAction()
        {
            yield return new WaitForSeconds(ProceedOnClickDelayTime);
            PlayerActionInput.Main.Trigger(buttonInputType);
        }

        protected virtual IEnumerator BlockInputTemporarily()
        {
            yield return new WaitForSeconds(PreventSpamDelayTime);
            blockInput = false;
        }

        #endregion Class Methods
    }
}