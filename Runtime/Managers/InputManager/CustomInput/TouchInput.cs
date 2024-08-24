using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Runtime.InputSystem
{
    public static class TouchInput
    {
        #region Properties

        public static int TouchCount
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
                if (Mouse.current.leftButton.isPressed)
                    return 1;
                else
                    return 0;
#else
                return Input.touchCount;
#endif
            }
        }

        public static TouchData InitialTouch
        {
            get
            {
                if (TouchCount > 0)
                {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
                    return new TouchData() { Position = Mouse.current.position.ReadValue() };
#else
                    return TouchData.FromTouch(Input.touches[0]);
#endif
                }
                else return null;
            }
        }

        public static bool IsFingerDown
        {
            get { return TouchCount > 0; }
        }

        public static List<TouchData> Touches
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
                return new List<TouchData>() { InitialTouch };
#else
                return GetTouchesFromInputTouches();
#endif
            }
        }

        public static Vector2 AverageTouchPosition
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
                return Mouse.current.position.ReadValue();
#else
                return GetAverageTouchPositionFromInputTouches();
#endif
            }
        }

        #endregion Properties

        #region Class Methods

        private static List<TouchData> GetTouchesFromInputTouches()
        {
            List<TouchData> touches = new List<TouchData>();

            foreach (var touch in Input.touches)
                touches.Add(TouchData.FromTouch(touch));

            return touches;
        }

        private static Vector2 GetAverageTouchPositionFromInputTouches()
        {
            Vector2 averagePosition = Vector2.zero;

            if (Input.touches != null && Input.touches.Length > 0)
            {
                foreach (var touch in Input.touches)
                    averagePosition += touch.position;

                averagePosition /= Input.touches.Length;
            }

            return averagePosition;
        }
    }

    #endregion Class Methods
}