using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Runtime.Tutorial
{
    /// <summary>
    /// Add to game object that is UI (grahpic UI) to trigger tutorial event.
    /// </summary>
    public class TutorialGraphicPointerTriggerObject : MonoBehaviour,
                                                       IPointerClickHandler,
                                                       IPointerDownHandler,
                                                       IPointerUpHandler,
                                                       IPointerEnterHandler,
                                                       IPointerExitHandler
    {
        #region Members

        private PointerTriggerType _pointerTriggerType;
        private int _sequenceIndex;
        private int _blockIndex;
        private bool _hasPointerIn;
        private UnityEvent _triggeredEventCallback;

        #endregion Members

        #region Class Methods

        public void SetupTriggerData(PointerTriggerType pointerTriggerType, int sequenceIndex, int blockIndex, UnityEvent triggeredEventCallback)
        {
            this._pointerTriggerType = pointerTriggerType;
            this._sequenceIndex = sequenceIndex;
            this._blockIndex = blockIndex;
            this._triggeredEventCallback = triggeredEventCallback;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_pointerTriggerType == PointerTriggerType.PointerClick)
                PlayAction();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _hasPointerIn = true;
            if (_pointerTriggerType == PointerTriggerType.PointerDown)
                PlayAction();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_hasPointerIn &&
                _pointerTriggerType == PointerTriggerType.PointerUp)
                PlayAction();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hasPointerIn = true;
            if (_pointerTriggerType == PointerTriggerType.PointerEnter)
                PlayAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hasPointerIn = false;
            if (_pointerTriggerType == PointerTriggerType.PointerExit)
                PlayAction();
        }

        private void PlayAction()
        {
            _triggeredEventCallback?.Invoke();
            TutorialNavigator.CurrentTutorial.StopTutorial(_sequenceIndex, _blockIndex);
        }

        #endregion Class Methods
    }
}