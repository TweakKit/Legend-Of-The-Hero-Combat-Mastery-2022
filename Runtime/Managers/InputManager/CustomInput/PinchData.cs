using UnityEngine;

namespace Runtime.InputSystem
{
    public class PinchData
    {
        #region Members

        public Vector3 pinchCenter;
        public float pinchDistance;
        public float pinchStartDistance;
        public float pinchAngleDelta;
        public float pinchAngleDeltaNormalized;
        public float pinchTiltDelta;
        public float pinchTotalFingerMovement;

        #endregion Members
    }
}