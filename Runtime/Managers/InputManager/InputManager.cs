using UnityEngine;
using Runtime.Message;
using Runtime.Core.Singleton;
using UnityEngine.InputSystem;
using Core.Foundation.PubSub;
using Runtime.Definition;
using UnityEditor;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.EnhancedTouch;
using NewInputTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
#else
using OldInputTouch = UnityEngine.Touch;
#endif

namespace Runtime.Manager
{
    public class InputManager : PersistentMonoSingleton<InputManager>
    {
        #region Members

        private GameInput _gameInput;
        public GameInput InputActions => _gameInput;

        #endregion Members

        #region Properties

#if ENABLE_INPUT_SYSTEM
        public static int TouchCount
        {
            get
            {
                return NewInputTouch.activeTouches.Count;
            }
        }

        public static NewInputTouch[] Touches
        {
            get
            {
                return NewInputTouch.activeTouches.ToArray();
            }
        }

        public static bool IsTouchSupported
        {
            get
            {
                return EnhancedTouchSupport.enabled;
            }
        }

        public static Vector3 MousePosition
        {
            get
            {
                return Mouse.current.position.ReadValue();
            }
        }
#else
        public static int TouchCount
        {
            get
            {
                return Input.touchCount;
            }
        }

        public static OldInputTouch[] Touches
        {
            get
            {
                return Input.touches;
            }
        }

        public static bool IsTouchSupported
        {
            get
            {
                return Input.touchSupported;
            }
        }

        public static Vector3 MousePosition
        {
            get
            {
                return Input.mousePosition;
            }
        }
#endif

        #endregion Properties

        #region API Methods

        private void Start() => SetUpInputEvents();

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Messenger.Publisher().Publish(new GameStateChangedMessage(GameStateEventType.PressBackKey));

            if (Input.GetMouseButtonDown(0))
                Messenger.Publisher().Publish(new ScreenGotTouchedMessage(Input.mousePosition));

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space))
                EditorApplication.isPaused = true;
#endif
        }

        #endregion API Methods

        #region Class Methods

#if ENABLE_INPUT_SYSTEM
        public static bool GetKeyDown(KeyCode keyCode)
            => GetKeyDown(keyCode.ToString().ToLower());

        public static bool GetKeyDown(string keyCode)
            => ((KeyControl)Keyboard.current[keyCode]).isPressed;

        public static bool GetMouseButtonDown(int button)
        {
            switch (button)
            {
                case 1:
                    return Mouse.current.rightButton.wasReleasedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasReleasedThisFrame;
                default:
                    return Mouse.current.leftButton.wasReleasedThisFrame;
            }
        }

        public static bool GetMouseButtonUp(int button)
        {
            switch (button)
            {
                case 1:
                    return Mouse.current.rightButton.wasPressedThisFrame;
                case 2:
                    return Mouse.current.middleButton.wasPressedThisFrame;
                default:
                    return Mouse.current.leftButton.wasPressedThisFrame;
            }
        }

        public static bool GetMouseButton(int button)
        {
            switch (button)
            {
                case 1:
                    return Mouse.current.rightButton.isPressed;
                case 2:
                    return Mouse.current.middleButton.isPressed;
                default:
                    return Mouse.current.leftButton.isPressed;
            }
        }

        public static Vector3 GetTouchPosition(int touchIdx)
            => Touches[touchIdx].screenPosition;
#else
        public static bool GetKeyDown(KeyCode keyCode)
            => GetKeyDown(keyCode.ToString().ToLower());

        public static bool GetKeyDown(string keyCode)
            => Input.GetKeyDown(keyCode);

        public static bool GetKey(KeyCode keyCode)
            => GetKey(keyCode.ToString().ToLower());

        public static bool GetKey(string keyCode)
            => Input.GetKey(keyCode);

        public static bool GetMouseButtonDown(int button)
            => Input.GetMouseButtonDown(button);

        public static bool GetMouseButtonUp(int button)
            => Input.GetMouseButtonUp(button);

        public static bool GetMouseButton(int button)
            => Input.GetMouseButton(button);

        public static Vector3 GetTouchPosition(int touchIdx)
            => Touches[touchIdx].position;
#endif

        public static Vector3 GetPointerPosition()
        {
            if (IsTouchSupported && TouchCount > 0)
                return GetTouchPosition(0);
            else
                return MousePosition;
        }

        private void SetUpInputEvents()
        {
            _gameInput = new GameInput();
            _gameInput.UI.Click.started += OnClick;
            _gameInput.Enable();
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            var touchPosition =  _gameInput.UI.Point.ReadValue<Vector2>();
            Messenger.Publisher().Publish(new ScreenGotTouchedMessage(touchPosition));
        }

        #endregion Class Methods
    }
}