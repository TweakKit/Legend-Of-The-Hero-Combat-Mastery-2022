using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Runtime.Tutorial
{
    public class TutorialBlockIndicatorPositionProp : TutorialBlockIndicatorProp
    {
        #region Members

        private const string ARROW_NAME = "Arrow";
        public TextMeshProUGUI displayText;
        public Image arrowMask;
        private Color _originArrowColor;
        private Vector3 _originArrowPos;
        private Vector2 _originArrowSize;

        #endregion Members

        #region Class Methods

        public override void InitAtAwake()
        {
            base.InitAtAwake();
            var arrow = transform.Find(ARROW_NAME);
            if (arrow != null)
                arrowMask = arrow.GetComponentInChildren<Image>();
        }

        public override void Init()
        {
            base.Init();
            if (arrowMask != null)
            {
                _originArrowColor = arrowMask.color;
                _originArrowPos = arrowMask.rectTransform.localPosition;
                _originArrowSize = arrowMask.rectTransform.sizeDelta;
            }
            if (displayText != null)
            {
                displayText.color = displayText.color;
                displayText.text = string.Empty;
            }
        }

        public override void Destroy()
        {
            base.Destroy();
            if (arrowMask != null)
            {
                arrowMask.color = _originArrowColor;
                arrowMask.rectTransform.localPosition = _originArrowPos;
                arrowMask.rectTransform.sizeDelta = _originArrowSize;
            }
            if (displayText != null)
                displayText.color = displayText.color;
        }

        public void SetDisplayText(bool display, string displayInfo)
        {
            if (displayText != null)
            {
                if (displayText.gameObject.activeSelf != display)
                    displayText.gameObject.SetActive(displayText);
                displayText.text = displayInfo;
            }
        }

        #endregion Class Methods
    }
}