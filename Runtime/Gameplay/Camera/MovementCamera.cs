using UnityEngine;
using Runtime.Core.Singleton;

namespace Runtime.Gameplay.GameCamera
{
    [RequireComponent(typeof(Camera))]
    public class MovementCamera : MonoSingleton<MovementCamera>
    {
        #region Members

        protected bool isControlledByOthers;
        protected Vector3 controlledByOthersMoveDelta;

        #endregion Members

        #region Properties

        public bool IsControledByOthers => isControlledByOthers;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            isControlledByOthers = false;
        }

        #endregion Class Methods

        #region Class Methods

        public virtual void Init(float cameraIntializedPositionX, float cameraIntializedPositionY)
        {
            var position = new Vector3(cameraIntializedPositionX, cameraIntializedPositionY, transform.position.z);
            transform.position = position;
        }

        public virtual void HandOverControlForOthers()
            => isControlledByOthers = true;

        public virtual void TakeOverControlFromOthers()
            => isControlledByOthers = false;

        public virtual void SetMoveDeltaWhileControlledByOthers(Vector3 moveDelta)
            => controlledByOthersMoveDelta = moveDelta;

        #endregion Class Methods
    }
}