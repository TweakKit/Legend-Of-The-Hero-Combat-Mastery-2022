using System;
using UnityEngine;
using TMPro;

namespace Runtime.Manager
{
    public class Toast : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private TextMeshProUGUI _displayText;
        [SerializeField]
        private VisualToastType _visualToastType;

        #endregion Members

        #region Properties

        public VisualToastType VisualToastType => _visualToastType;
        private Action<Toast> EndToast { get; set; }

        #endregion Properties

        #region Class Methods

        public void Init(string content, Transform positionTransform, Action<Toast> endToastAction)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup)
                canvasGroup.alpha = 1;
            transform.SetParent(positionTransform);
            transform.position = positionTransform.position;
            gameObject.SetActive(true);
            _displayText.text = content;
            EndToast = endToastAction;
        }

        public void Finishing()
        {
            EndToast?.Invoke(this);
            gameObject.SetActive(false);
        }

        #endregion Class Methods
    }
}
