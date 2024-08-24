using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.GameCamera;

namespace Runtime.Tutorial
{
    public class TutorialBlockCameraMovementIndicator : TutorialBlockTargetIndicator<TutorialBlockCamerMovementIndicatorData>
    {
        #region Members

        private bool _hasStoppedMovingCamera;
        private float _currentCameraMoveDuration;
        private float _cameraMoveDuration;
        private Vector2 _cameraMoveDirection;
        private float _cameraMoveDistance;
        private Vector2 _cameraTargetPosition;
        private MovementCamera _movementCamera;

        #endregion Members

        #region Class Methods

        public override void Init(TutorialBlockIndicatorData tutorialBlockIndicatorData, TutorialBlockData tutorialBlockData)
        {
            base.Init(tutorialBlockIndicatorData, tutorialBlockData);
            _hasStoppedMovingCamera = false;
            _currentCameraMoveDuration = 0.0f;
            _cameraMoveDuration = OwnerBlockIndicatorData.cameraMoveDuration;
            _movementCamera = OwnerBlockIndicatorData.movementCamera;
            _movementCamera.HandOverControlForOthers();
            _cameraMoveDirection = (_cameraTargetPosition - (Vector2)_movementCamera.transform.position).normalized;
            _cameraMoveDistance = (_cameraTargetPosition - (Vector2)_movementCamera.transform.position).magnitude;
        }

        public override void InitStuff(List<TutorialBlockTargetData> tutorialBlockTargetsData)
        {
            base.InitStuff(tutorialBlockTargetsData);
            if (tutorialBlockTargetsData.Count > 0)
            {
                var cameraTargetTransform = tutorialBlockTargetsData[0].runtimeTarget.transform;
                _cameraTargetPosition = cameraTargetTransform.position;
            }
        }

        public override void RunUpdate()
        {
            base.RunUpdate();
            if (!_hasStoppedMovingCamera)
            {
                if (_currentCameraMoveDuration <= _cameraMoveDuration)
                {
                    var newCameraMoveDelta = _cameraMoveDirection * (_cameraMoveDistance * Time.deltaTime) / _cameraMoveDuration;
                    _movementCamera.SetMoveDeltaWhileControlledByOthers(newCameraMoveDelta);
                    _currentCameraMoveDuration += Time.deltaTime;
                }
                else
                {
                    _hasStoppedMovingCamera = true;
                    FinishMovingCamera();
                }
            }
        }

        private void FinishMovingCamera()
        {
            _movementCamera.TakeOverControlFromOthers();
            TutorialNavigator.CurrentTutorial.StopTutorial(OwnerBlockData.blockIndex);
        }

        #endregion Class Method
    }
}