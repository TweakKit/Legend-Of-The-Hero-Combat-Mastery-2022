using UnityEngine;
using UnityEngine.EventSystems;
using Runtime.Manager;

namespace Runtime.Tutorial
{
    /// <summary>
    /// Add to game object to trigger event.
    /// </summary>
    public class TutorialTriggerObject : MonoBehaviour,
                                         IPointerClickHandler,
                                         IPointerDownHandler,
                                         IPointerUpHandler,
                                         IPointerEnterHandler,
                                         IPointerExitHandler
    {
        #region Members

        private TutorialTriggerData _ownerTutorialTriggerData;
        private int _sequenceIndex;
        private int _blockIndex;
        private bool _isStartTriggerData;
        private bool _isSequence;
        private bool _hasPointerIn;

        #endregion Members

        #region Properties

        public bool IsStartTriggerData
        {
            get { return _isStartTriggerData; }
        }

        public bool IsSequence
        {
            get { return _isSequence; }
        }

        #endregion Properties

        #region API Methods

        private void Update()
        {
            if (_ownerTutorialTriggerData.triggerType == TriggerType.KeyCode)
            {
                if (InputManager.GetKeyDown(_ownerTutorialTriggerData.keyCode))
                    PlayAction();
            }
        }

        private void OnTriggerEnter(Collider collider)
        {
            if (_ownerTutorialTriggerData.triggerType == TriggerType.Collider)
            {
                if (CheckTriggerEvent(collider.gameObject))
                    PlayAction();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_ownerTutorialTriggerData.triggerType == TriggerType.Collider)
            {
                if (CheckTriggerEvent(collision.gameObject))
                    PlayAction();
            }
        }

        #endregion API Methods

        #region Class Methods

        public void SetupTriggerData(TutorialTriggerData tutorialTriggerData, int sequenceIndex, int blockIndex, bool isStartTriggerData, bool isSequence)
        {
            this._ownerTutorialTriggerData = tutorialTriggerData;
            this._sequenceIndex = sequenceIndex;
            this._blockIndex = blockIndex;
            this._isSequence = isSequence;
            this._isStartTriggerData = isStartTriggerData;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_ownerTutorialTriggerData.graphicTriggerData.graphic != null &&
                _ownerTutorialTriggerData.graphicTriggerData.pointerTrigger == PointerTriggerType.PointerClick)
                PlayAction();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _hasPointerIn = true;
            if (_ownerTutorialTriggerData.graphicTriggerData.graphic != null &&
                _ownerTutorialTriggerData.graphicTriggerData.pointerTrigger == PointerTriggerType.PointerDown)
                PlayAction();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_hasPointerIn &&
                _ownerTutorialTriggerData.graphicTriggerData.graphic != null &&
                _ownerTutorialTriggerData.graphicTriggerData.pointerTrigger == PointerTriggerType.PointerUp)
                PlayAction();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hasPointerIn = true;
            if (_ownerTutorialTriggerData.graphicTriggerData.graphic != null &&
                _ownerTutorialTriggerData.graphicTriggerData.pointerTrigger == PointerTriggerType.PointerEnter)
                PlayAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hasPointerIn = false;
            if (_ownerTutorialTriggerData.graphicTriggerData.graphic != null &&
                _ownerTutorialTriggerData.graphicTriggerData.pointerTrigger == PointerTriggerType.PointerExit)
                PlayAction();
        }

        private bool CheckTriggerEvent(GameObject gameObject)
        {
            if (_ownerTutorialTriggerData.colliderTriggerData.useLayerFiltering)
            {
                if (_ownerTutorialTriggerData.colliderTriggerData.filterLayer != (_ownerTutorialTriggerData.colliderTriggerData.filterLayer | (1 << gameObject.layer)))
                    return false;
            }

            if (_ownerTutorialTriggerData.colliderTriggerData.useTagFiltering)
            {
                if (_ownerTutorialTriggerData.colliderTriggerData.filterTag != gameObject.tag)
                    return false;
            }

            return true;
        }

        private void PlayAction()
        {
            if (_isStartTriggerData)
            {
                if (_isSequence)
                    TutorialNavigator.CurrentTutorial.PlaySequence(_sequenceIndex);
                else
                    TutorialNavigator.CurrentTutorial.PlayTutorial(_sequenceIndex, _blockIndex);
            }
            else
            {
                if (_isSequence)
                    TutorialNavigator.CurrentTutorial.StopSequence();
                else
                    TutorialNavigator.CurrentTutorial.StopTutorial(_sequenceIndex, _blockIndex);
            }
        }

        #endregion Class Methods
    }
}