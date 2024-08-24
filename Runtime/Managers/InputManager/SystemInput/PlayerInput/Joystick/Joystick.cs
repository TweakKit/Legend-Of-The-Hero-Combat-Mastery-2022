using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Runtime.Manager
{
    public enum AxisDirection
    {
        Both,
        Horizontal,
        Vertical
    }

    public class Joystick : OnScreenControl, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Members

        [InputControl(layout = "Vector2")]
        [SerializeField]
        private string _controlPath;
        [SerializeField]
        protected float deadZone;
        [SerializeField]
        protected AxisDirection axisDirection = AxisDirection.Both;
        [SerializeField]
        protected RectTransform joystickBackgroundTransform;
        [SerializeField]
        protected RectTransform joystickKnobTransform;

        protected Vector2 joystickBackgroundHalfSizeDelta;
        protected RectTransform joystickContainerTransform;
        protected Canvas worldCanvas;
        protected Vector2 input;
        protected Vector2 originalJoystickBackgroundPosition;

        #endregion Members

        #region Properties

        protected override string controlPathInternal
        {
            get => _controlPath;
            set => _controlPath = value;
        }

        #endregion Properties

        #region API Methods

        protected virtual void Start()
        {
            joystickContainerTransform = gameObject.GetComponent<RectTransform>();
            worldCanvas = gameObject.GetComponentInParent<Canvas>();
            joystickBackgroundTransform.pivot = Vector2.one * 0.5f;
            joystickKnobTransform.pivot = Vector2.one * 0.5f;
            joystickKnobTransform.anchoredPosition = Vector2.zero;
            joystickBackgroundHalfSizeDelta = joystickBackgroundTransform.sizeDelta / 2;
            originalJoystickBackgroundPosition = joystickBackgroundTransform.anchoredPosition;
        }

        #endregion API Methods

        #region Class Methods

        public virtual void OnPointerDown(PointerEventData pointerEventData) => OnDrag(pointerEventData);

        public virtual void OnDrag(PointerEventData pointerEventData)
        {
            Vector2 joystickBackgroundScreenPosition = RectTransformUtility.WorldToScreenPoint(pointerEventData.pressEventCamera, joystickBackgroundTransform.position);
            input = (pointerEventData.position - joystickBackgroundScreenPosition) / (joystickBackgroundHalfSizeDelta * worldCanvas.scaleFactor);

            // Check and update the input.
            CheckAndUpdateInput(input.magnitude, input.normalized);
            SendValueToControl(input);

            joystickKnobTransform.anchoredPosition = input * joystickBackgroundHalfSizeDelta;
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            input = Vector2.zero;
            joystickKnobTransform.anchoredPosition = Vector2.zero;
            SendValueToControl(input);
        }

        protected virtual void CheckAndUpdateInput(float inputMagnitude, Vector2 inputNormalized)
        {
            if (inputMagnitude > deadZone)
            {
                if (inputMagnitude > 1)
                    input = inputNormalized;
            }
            else input = Vector2.zero;

            if (axisDirection == AxisDirection.Horizontal)
                input = new Vector2(input.x, 0.0f);
            else if (axisDirection == AxisDirection.Vertical)
                input = new Vector2(0.0f, input.y);
        }

        #endregion Class Methods
    }
}