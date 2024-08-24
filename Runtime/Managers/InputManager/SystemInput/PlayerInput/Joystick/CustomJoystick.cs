using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runtime.Manager
{
    public class CustomJoystick : Joystick
    {
        #region Members

        [SerializeField]
        private float _normalOpacity;
        [SerializeField]
        private float _pressOpacity;
        [SerializeField]
        private Image[] _joyStickImages;

        #endregion Members

        #region API Methods

        protected override void OnEnable()
        {
            base.OnEnable();
            SetColorOpacity(_normalOpacity);
        }

        #endregion API Methods

        #region Class Methods

        public override void OnPointerDown(PointerEventData eventData)
        {
            SetColorOpacity(_pressOpacity);
            joystickBackgroundTransform.anchoredPosition = ScreenPointToAnchoredPosition(eventData);
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            SetColorOpacity(_normalOpacity);
            joystickBackgroundTransform.anchoredPosition = originalJoystickBackgroundPosition;
            base.OnPointerUp(eventData);
        }

        private void SetColorOpacity(float opacity)
        {
            foreach (var image in _joyStickImages)
            {
                var newColor = image.color;
                newColor.a = opacity;
                image.color = newColor;
            }
        }

        private Vector2 ScreenPointToAnchoredPosition(PointerEventData pointerEventData)
        {
            Vector2 localPosition = Vector2.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickContainerTransform, pointerEventData.position, pointerEventData.pressEventCamera, out localPosition))
            {
                Vector2 pivotOffset = new Vector2(joystickContainerTransform.rect.width * 0.5f, joystickContainerTransform.rect.height * 0.5f);
                return localPosition + pivotOffset;
            }

            return Vector2.zero;
        }

        #endregion Class Methods
    }
}