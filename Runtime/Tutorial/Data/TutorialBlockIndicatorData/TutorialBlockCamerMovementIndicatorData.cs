using System;
using Runtime.Gameplay.GameCamera;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialBlockCamerMovementIndicatorData : TutorialBlockTargetIndicatorData
    {
        #region Members

        public float cameraMoveDuration;
        public MovementCamera movementCamera;

        #endregion Members
    }
}