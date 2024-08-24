using System;
using UnityEngine;
using UnityEngine.UI;
using Runtime.Core.Mics;
using Runtime.Audio;

namespace Runtime.UI
{
    [RequireComponent(typeof(Toggle))]
    public class CustomToggle : MonoBehaviour, IClickable
    {
        #region Members

        [SerializeField]
        private GameObject _activeWhenOnItem;
        [SerializeField]
        private GameObject _activeWhenOffItem;
        [SerializeField]
        private GameObject _lockInteractableObject;
        [SerializeField]
        private Button _lockButton;
        [SerializeField]
        private Toggle _toggle;

        #endregion Members

        #region API Methods

        private void OnValidate()
        {
            _toggle = GetComponent<Toggle>();
        }

        private void Awake()
            => ToggleItem(_toggle.isOn);

        #endregion API Methods

        #region Class Methods

        public void Click()
            => _toggle.isOn = true;

        public void SetOn(bool isOn)
            => _toggle.isOn = isOn;

        public void RemoveAllListeners()
            => _toggle.onValueChanged.RemoveAllListeners();

        public void AddListener(Action<bool> toggleAction)
        {
            SetInteractable(true);
            _toggle.onValueChanged.AddListener((isOn) => {
                toggleAction?.Invoke(isOn);
                ToggleItem(isOn);
                if (isOn)
                    AudioController.Instance.PlaySoundEffect(AudioConstants.UI_CLICK);
            });
        }

        public void Lock(Action lockResponseAction)
        {
            SetInteractable(false);
            if (_lockButton)
            {
                _lockButton.onClick.RemoveAllListeners();
                _lockButton.onClick.AddListener(() => lockResponseAction?.Invoke());
            }
        }

        private void ToggleItem(bool isOn)
        {
            if (_activeWhenOnItem)
                _activeWhenOnItem.SetActive(isOn);
            if (_activeWhenOffItem)
                _activeWhenOffItem.SetActive(!isOn);
        }

        private void SetInteractable(bool interactable)
        {
            _toggle.interactable = interactable;
            if (_lockButton)
                _lockButton.gameObject.SetActive(!interactable);
            if (_lockInteractableObject)
                _lockInteractableObject.SetActive(!interactable);
        }

        #endregion Class Methods
    }
}