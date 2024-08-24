using UnityEngine;

namespace Runtime.InputSystem
{
    public class TouchData
    {
        #region Properties

        public Vector3 Position { get; set; }
        public int FingerID { get; set; }

        #endregion Properties

        #region Class Methods

        public TouchData() => FingerID = -1;
        public static TouchData FromTouch(Touch touch) => new TouchData() { Position = touch.position, FingerID = touch.fingerId };

        #endregion Class Methods
    }
}