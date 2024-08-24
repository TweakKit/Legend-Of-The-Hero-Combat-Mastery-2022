using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Runtime.InputSystem;
using Sirenix.OdinInspector;

namespace Runtime.Gameplay.GameCamera
{
    [RequireComponent(typeof(Camera))]
    public class PinchZoomCamera : MovementCamera
    {
        #region Members

        public enum PlaneAxis
        {
            XYSideScroll,
            XZTopDown
        }

        public enum Projection
        {
            Perspective,
            Orthographic
        }

        public enum PerspectiveZoomMode
        {
            FieldOfView,
            Translation,
        }

        private const int MOMENTUM_SAMPLES_COUNT = 5;
        private const float PINCH_DISTANCE_FOR_TILT_BREAKOUT = 0.05f;
        private const float PINCH_ACCUMULATED_BREAKOUT = 0.025f;
        private const float MAX_HORIZONTAL_FALLBACK_DISTANCE = 10000;

        [SerializeField]
        [Tooltip("Define whether the camera is a side-view camera (which is the default when using the 2D mode of unity) or a top-down looking camera. " +
                 "This parameter tells the system whether to scroll in XY direction, or in XZ direction.")]
        private PlaneAxis _renderPlaneAxis = PlaneAxis.XYSideScroll;

        [SerializeField]
        [Tooltip("Define Whether the camera's projection is perspective or orthographic.")]
        private Projection _projection = Projection.Orthographic;

        [ShowIf(nameof(_projection), Projection.Perspective)]
        [SerializeField]
        [Tooltip("When using a perspective camera, the zoom can either be performed by changing the field of view, " +
                 "or by moving the camera closer to the scene.")]
        private PerspectiveZoomMode _perspectiveZoomMode = PerspectiveZoomMode.FieldOfView;

        [SerializeField]
        [Tooltip("For perspective cameras this value denotes the min field of view used for zooming (field of view zoom), " +
                 "or the min distance to the ground (translation zoom). For orthographic cameras it denotes the min camera size.")]
        private float _cameraZoomMin = 4;

        [SerializeField]
        [Tooltip("For perspective cameras this value denotes the max field of view used for zooming (field of view zoom), " +
                 "or the max distance to the ground (translation zoom). For orthographic cameras it denotes the max camera size.")]
        private float _cameraZoomMax = 12;

        [SerializeField]
        [Tooltip("The cam will overzoom the min/max values by this amount and spring back when the user releases the zoom.")]
        private float _cameraOverzoomMargin = 1;

        [SerializeField]
        [Tooltip("When dragging the camera close to the defined border, it will spring back when the user stops dragging. " +
                 "This value defines the distance from the border where the camera will spring back to.")]
        private float _cameraOverdragMargin = 5.0f;

        [SerializeField]
        [Tooltip("These values define the scrolling borders for the camera. The camera will not scroll further than defined here. " +
                 "When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 _boundaryMin = new Vector2(-1000, -1000);

        [SerializeField]
        [Tooltip("These values define the scrolling borders for the camera. The camera will not scroll further than defined here. " +
                 "When a top-down camera is used, these 2 values are applied to the X/Z position.")]
        private Vector2 _boundaryMax = new Vector2(1000, 1000);

        [SerializeField]
        [Tooltip("The lower the value, the slower the camera will follow. The higher the value, the more direct the camera " +
                 "will follow movement updates. Necessary for keeping the camera smooth when the framerate is not in sync with the touch input update rate.")]
        private float _camFollowFactor = 15.0f;

        [SerializeField]
        [Tooltip("When dragging quickly, the camera will keep autoscrolling in the last direction. The autoscrolling will slowly come to a halt. " +
                 "This value defines how fast the camera will come to a halt.")]
        private float _autoScrollDamp = 300;

        [SerializeField]
        [Tooltip("This curve allows to modulate the auto scroll damp value over time.")]
        private AnimationCurve _autoScrollDampCurve = new AnimationCurve(new Keyframe(0, 1, 0, 0),
                                                                         new Keyframe(0.7f, 0.9f, -0.5f, -0.5f),
                                                                         new Keyframe(1, 0.01f, -0.85f, -0.85f));

        [SerializeField]
        [Tooltip("The camera assumes that the scrollable content of your scene (e.g. the ground of your game-world) is located at y = 0 " +
                 "for top-down cameras or at z = 0 for side-scrolling cameras. In case this is not valid for your scene, you may adjust this property to the correct offset.")]
        private float _groundLevelOffset = 0;

        [SerializeField]
        [Tooltip("When enabled, the camera can be rotated using a 2-finger rotation gesture.")]
        private bool _enableRotation = false;

        [SerializeField]
        [Tooltip("When enabled, the camera can be tilted using a synced 2-finger up or down motion.")]
        private bool _enableTilt = false;

        [SerializeField]
        [Tooltip("Allow to enable the camera to spring being when being tilted over the limits.")]
        private bool _enableOvertiltSpring = false;

        [SerializeField]
        [Tooltip("This value is necessary to reposition the camera and do boundary update computations while " +
                 "the auto spring back from overtilt is active and larger than this value.")]
        private float _minOvertiltSpringPositionThreshold = 0.1f;

        [SerializeField]
        [Tooltip("The minimum tilt angle for the camera.")]
        private float _tiltAngleMin = 45;

        [SerializeField]
        [Tooltip("The maximum tilt angle for the camera.")]
        private float _tiltAngleMax = 90;

        [SerializeField]
        [Tooltip("When enabled, the camera is tilted automatically when zooming.")]
        private bool _enableZoomTilt = false;

        [SerializeField]
        [Tooltip("The minimum tilt angle for the camera when using zoom tilt.")]
        private float _zoomTiltAngleMin = 45;

        [SerializeField]
        [Tooltip("The maximum tilt angle for the camera when using zoom tilt.")]
        private float _zoomTiltAngleMax = 90;

        [SerializeField]
        [Tooltip("Depending on the settings the camera allows to zoom slightly over the defined value. When releasing the " +
                 "zoom the camera will spring back to the defined value. This variable defines the speed of the spring back.")]
        private float _zoomBackSpringFactor = 20;

        [SerializeField]
        [Tooltip("When close to the border the camera will spring back if the margin is bigger than 0. " +
                 "This variable defines the speed of the spring back.")]
        private float _dragBackSpringFactor = 10;

        [SerializeField]
        [Tooltip("When swiping over the screen the camera will keep scrolling a while before coming to a halt. " +
                 "This variable limits the maximum velocity of the auto scroll.")]
        private float _autoScrollVelocityMax = 60;

        [SerializeField]
        [Tooltip("This value defines how quickly the camera comes to a halt when auto scrolling.")]
        private float _dampFactorTimeMultiplier = 2;

        [SerializeField]
        [Tooltip("When setting this flag to true, the camera will behave like a popular tower defense game. " +
                 "It will either go into an exclusive tilt mode, or into a combined zoom/rotate mode. When set to false, the camera will behave like a popular city building game. " +
                 "The camera won't pan with 2 fingers, and instead zoom, rotate and tilt are done in parallel.")]
        private bool _isPinchModeExclusive = true;

        [SerializeField]
        [Tooltip("This value should be kept at 1 for pixel perfect zoom. For non-pixel perfect, slower or faster zoom however. 0.5f for " +
                 "example will make the camera zoom half as fast as in pixel perfect mode. This value is currently tested only in perspective camera mode with translation based zoom.")]
        private float _customZoomSensitivity = 1.0f;

        [SerializeField]
        [Tooltip("When setting this flag to true, the uniform camOverdragMargin will be overrided with the values set in camOverdragMargin2d.")]
        private bool _is2DOverdragMarginEnabled = false;

        [SerializeField]
        [Tooltip("Using this field allows to set the horizontal overdrag to a different value than the vertical one.")]
        private Vector2 _cameraOverdragMargin2d = Vector2.one * 5.0f;

        [SerializeField]
        [Tooltip("A gesture may be interpreted as intended rotation in case the relative rotation angle between 2 frames becomes bigger than this value.")]
        private float _rotationDetectionDeltaThreshold = 0.25f;

        [SerializeField]
        [Tooltip("Relative pinch distance must be bigger than this value in order to detect a rotation. This is to prevent errors that occur when " +
                 "the fingers are too close together to properly detect a clean rotation.")]
        private float _rotationMinPinchDistance = 0.125f;

        [SerializeField]
        [Tooltip("The rotation mode is enabled as soon as the rotation by the user becomes bigger than this value (in degrees). The value is used to " +
                 "prevent micro rotations from regular jittering of the fingers to be interpreted as rotation and helps keeping the camera more steady and less jittery.")]
        private float _rotationLockThreshold = 2.5f;

        [SerializeField]
        [Tooltip("After this amount of finger-movement (relative to screen size), the pinch mode is decided. E.g. whether tilt mode or regular mode is used.")]
        private float _pinchModeDetectionMoveTreshold = 0.025f;

        [SerializeField]
        [Tooltip("A threshold used to detect the up or down tilting motion.")]
        private float _pinchTiltModeThreshold = 0.0075f;

        [SerializeField]
        [Tooltip("The tilt sensitivity once the tilt mode has started.")]
        private float _pinchTiltSpeed = 180;

        [SerializeField]
        private float _mouseZoomFactor = 1;

        private Vector3 _lastPosition = Vector3.zero;
        private Vector3 _dragStartCameraPosition;
        private Vector3 _cameraScrollVelocity;
        private float _pinchStartCameraZoomSize;
        private Vector3 _pinchStartIntersectionCenter;
        private Vector3 _pinchCenterCurrent;
        private float _pinchDistanceCurrent;
        private float _pinchAngleCurrent = 0;
        private float _pinchDistanceStart;
        private Vector3 _pinchCenterCurrentLerp;
        private float _pinchDistanceCurrentLerp;
        private float _pinchAngleCurrentLerp;
        private bool _isRotationLock = true;
        private bool _isRotationActivated = false;
        private float _pinchAngleLastFrame = 0;
        private float _pinchTiltCurrent = 0;
        private float _pinchTiltAccumulated = 0;
        private bool _isTiltModeEvaluated = false;
        private float _pinchTiltLastFrame;
        private bool _isPinchTiltMode;
        private float _timeRealDragStop;
        private Vector3 _cameraVelocity = Vector3.zero;
        private Plane _refPlaneXY = new Plane(new Vector3(0, 0, -1), 0);
        private Plane _refPlaneXZ = new Plane(new Vector3(0, 1, 0), 0);
        private Vector3 _targetPositionClamped = Vector3.zero;
        private bool _showHorizonError = true;
        private float _cameraOvertiltMargin = 5.0f;
        private float _tiltBackSpringFactor = 30;
        private bool _useUntransformedCameraBoundary = false;
        private bool _useOldScrollDamp = false;

        #endregion Members

        #region Properties

        public PlaneAxis RenderPlaneAxis
        {
            get { return _renderPlaneAxis; }
            set { _renderPlaneAxis = value; }
        }

        public Vector2 BoundaryMin
        {
            get { return _boundaryMin; }
            set { _boundaryMin = value; }
        }

        public Vector2 BoundaryMax
        {
            get { return _boundaryMax; }
            set { _boundaryMax = value; }
        }

        private Camera Camera { get; set; }
        private bool IsPinching { get; set; }
        private bool IsDragging { get; set; }
        private bool IsSmoothingEnabled { get; set; }
        private List<Vector3> DragCameraMoveVector { get; set; }
        private Vector2 MinCameraPosition { get; set; }
        private Vector2 MaxCameraPosition { get; set; }
        private bool IsTranslationZoom { get { return (Camera.orthographic == false && _perspectiveZoomMode == PerspectiveZoomMode.Translation); } }

        private float CameraZoomValue
        {
            get
            {
                if (Camera.orthographic)
                {
                    return Camera.orthographicSize;
                }
                else
                {
                    if (IsTranslationZoom)
                    {
                        Vector3 camCenterIntersection = GetIntersectionPoint(GetCameraCenterRay());
                        return Vector3.Distance(camCenterIntersection, transform.position);
                    }
                    else return Camera.fieldOfView;
                }
            }
            set
            {
                if (Camera.orthographic)
                {
                    Camera.orthographicSize = value;
                }
                else
                {
                    if (IsTranslationZoom == true)
                    {
                        Vector3 cameraCenterIntersection = GetIntersectionPoint(GetCameraCenterRay());
                        transform.position = cameraCenterIntersection - transform.forward * value;
                    }
                    else Camera.fieldOfView = value;
                }

                ComputeCameraBoundaries();
            }
        }

        private Vector2 CammeraOverdragMargin2D
        {
            get
            {
                if (_is2DOverdragMarginEnabled)
                    return _cameraOverdragMargin2d;
                else
                    return Vector2.one * _cameraOverdragMargin;
            }
            set
            {
                _cameraOverdragMargin2d = value;
                _cameraOverdragMargin = value.x;
            }
        }

        private Plane ReferencePlane
        {
            get
            {
                if (_renderPlaneAxis == PlaneAxis.XZTopDown)
                    return _refPlaneXZ;
                else
                    return _refPlaneXY;
            }
        }

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            Camera = gameObject.GetComponent<Camera>();
            Camera.orthographic = _projection == Projection.Orthographic;
            IsSmoothingEnabled = true;
            IsPinching = false;
            IsDragging = false;
            DragCameraMoveVector = new List<Vector3>();

            _dragStartCameraPosition = Vector3.zero;
            _cameraScrollVelocity = Vector3.zero;
            _timeRealDragStop = 0;
            _pinchStartCameraZoomSize = 0;
            _refPlaneXY = new Plane(new Vector3(0, 0, -1), _groundLevelOffset);
            _refPlaneXZ = new Plane(new Vector3(0, 1, 0), -_groundLevelOffset);

            if (_enableZoomTilt == true)
                ResetZoomTilt();

            ComputeCameraBoundaries();

            if (_cameraZoomMax < _cameraZoomMin)
            {
                float camZoomMinBackup = _cameraZoomMin;
                _cameraZoomMin = _cameraZoomMax;
                _cameraZoomMax = camZoomMinBackup;
            }
        }

        public void Start()
        {
            TouchInputController touchInputController = FindObjectOfType<TouchInputController>();
            touchInputController.OnDragStart += InputControllerOnDragStart;
            touchInputController.OnDragUpdate += InputControllerOnDragUpdate;
            touchInputController.OnDragStop += InputControllerOnDragStop;
            touchInputController.OnFingerDown += InputControllerOnFingerDown;
            touchInputController.OnFingerUp += InputControllerOnFingerUp;
            touchInputController.OnPinchStart += InputControllerOnPinchStart;
            touchInputController.OnPinchUpdate += InputControllerOnPinchUpdate;
            touchInputController.OnPinchStop += InputControllerOnPinchStop;
            StartCoroutine(InitCamBoundariesDelayed());
        }

        public void Update()
        {
            // Auto scroll.
            if (_cameraScrollVelocity.sqrMagnitude > float.Epsilon)
            {
                float timeSinceDragStop = Time.realtimeSinceStartup - _timeRealDragStop;
                float dampFactor = Mathf.Clamp01(timeSinceDragStop * _dampFactorTimeMultiplier);
                float cameraScrollVelocity = _cameraScrollVelocity.magnitude;
                float cameraScrollVelocityRelative = cameraScrollVelocity / _autoScrollVelocityMax;

                Vector3 cameraVelocityDamp = dampFactor * _cameraScrollVelocity.normalized * _autoScrollDamp * Time.unscaledDeltaTime;
                cameraVelocityDamp *= EvaluateAutoScrollDampCurve(Mathf.Clamp01(1.0f - cameraScrollVelocityRelative));

                if (cameraVelocityDamp.sqrMagnitude >= _cameraScrollVelocity.sqrMagnitude)
                    _cameraScrollVelocity = Vector3.zero;
                else
                    _cameraScrollVelocity -= cameraVelocityDamp;
            }
        }

        public void LateUpdate()
        {
            // Pinch.
            UpdatePinch(Time.unscaledDeltaTime);

            // Translation.
            UpdatePosition(Time.unscaledDeltaTime);

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
            UpdateScroll();
#endif

            // Snap zoom.
            SnapZoom();
        }

        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector2 boundaryCenter2d = 0.5f * (_boundaryMin + _boundaryMax);
            Vector2 boundarySize2d = _boundaryMax - _boundaryMin;
            Vector3 boundaryCenter = UnprojectVector2(boundaryCenter2d, _groundLevelOffset);
            Vector3 boundarySize = UnprojectVector2(boundarySize2d);
            Gizmos.DrawWireCube(boundaryCenter, boundarySize);
        }

        #endregion API Methods

        #region Class Methods

        public override void Init(float cameraIntializedPositionX, float cameraIntializedPositionY)
        {
            base.Init(cameraIntializedPositionX, cameraIntializedPositionY);
            CameraZoomValue = (_cameraZoomMax + _cameraZoomMin) / 2.0f;
        }

        /// <summary>
        /// This method tilts the camera based on the values
        /// defined for the zoom tilt mode.
        /// </summary>
        public void ResetZoomTilt() => UpdateTiltForAutoTilt(CameraZoomValue);

        /// <summary>
        /// Method for retrieving the intersection-point between the given ray and the ref plane.
        /// </summary>
        public Vector3 GetIntersectionPoint(Ray ray)
        {
            float distance = 0;
            bool success = ReferencePlane.Raycast(ray, out distance);
            if (!success || (!Camera.orthographic && distance > MAX_HORIZONTAL_FALLBACK_DISTANCE))
            {
                if (_showHorizonError == true)
                {
                    Debug.LogError("Failed to compute intersection between camera ray and reference plane. Make sure the camera Axes are set up correctly.");
                    _showHorizonError = false;
                }

                // Fallback: Compute a sphere-cap on the ground and use the border point at the direction of the ray as maximum point in the distance.
                Vector3 rayOriginProjected = UnprojectVector2(ProjectVector3(ray.origin));
                Vector3 rayDirProjected = UnprojectVector2(ProjectVector3(ray.direction));
                return rayOriginProjected + rayDirProjected.normalized * MAX_HORIZONTAL_FALLBACK_DISTANCE;
            }

            return ray.origin + ray.direction * distance;
        }

        /// <summary>
        /// Custom planet intersection method that doesn't take into account rays parallel to the plane or rays shooting in the wrong direction and thus never hitting.
        /// May yield slightly better performance however and should be safe for use when the camera setup is correct (e.g. axes set correctly in this script, and camera actually pointing towards floor).
        /// </summary>
        public Vector3 GetIntersectionPointUnsafe(Ray ray)
        {
            float distance = Vector3.Dot(ReferencePlane.normal, Vector3.zero - ray.origin) / Vector3.Dot(ReferencePlane.normal, (ray.origin + ray.direction) - ray.origin);
            return ray.origin + ray.direction * distance;
        }

        /// <summary>
        /// Returns whether or not the camera is at the defined boundary.
        /// </summary>
        public bool GetIsBoundaryPosition(Vector3 testPosition)
        {
            return GetIsBoundaryPosition(testPosition, Vector2.zero);
        }

        /// <summary>
        /// Returns whether or not the camera is at the defined boundary.
        /// </summary>
        public bool GetIsBoundaryPosition(Vector3 testPosition, Vector2 margin)
        {
            bool isBoundaryPosition = false;
            switch (_renderPlaneAxis)
            {
                case PlaneAxis.XYSideScroll:
                    isBoundaryPosition = testPosition.x <= MinCameraPosition.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= MaxCameraPosition.x - margin.x;
                    isBoundaryPosition |= testPosition.y <= MinCameraPosition.y + margin.y;
                    isBoundaryPosition |= testPosition.y >= MaxCameraPosition.y - margin.y;
                    break;

                case PlaneAxis.XZTopDown:
                    isBoundaryPosition = testPosition.x <= MinCameraPosition.x + margin.x;
                    isBoundaryPosition |= testPosition.x >= MaxCameraPosition.x - margin.x;
                    isBoundaryPosition |= testPosition.z <= MinCameraPosition.y + margin.y;
                    isBoundaryPosition |= testPosition.z >= MaxCameraPosition.y - margin.y;
                    break;
            }

            return isBoundaryPosition;
        }

        /// <summary>
        /// Returns a position that is clamped to the defined boundary.
        /// </summary>
        public Vector3 GetClampToBoundaries(Vector3 newPosition, bool includeSpringBackMargin = false)
        {
            Vector2 margin = Vector2.zero;
            if (includeSpringBackMargin == true)
                margin = CammeraOverdragMargin2D;

            switch (_renderPlaneAxis)
            {
                case PlaneAxis.XYSideScroll:
                    newPosition.x = Mathf.Clamp(newPosition.x, MinCameraPosition.x + margin.x, MaxCameraPosition.x - margin.x);
                    newPosition.y = Mathf.Clamp(newPosition.y, MinCameraPosition.y + margin.y, MaxCameraPosition.y - margin.y);
                    break;
                case PlaneAxis.XZTopDown:
                    newPosition.x = Mathf.Clamp(newPosition.x, MinCameraPosition.x + margin.x, MaxCameraPosition.x - margin.x);
                    newPosition.z = Mathf.Clamp(newPosition.z, MinCameraPosition.y + margin.y, MaxCameraPosition.y - margin.y);
                    break;
            }

            return newPosition;
        }

        /// <summary>
        /// Helper method that unprojects the given Vector2 to a Vector3
        /// according to the camera axes setting.
        /// </summary>
        private Vector3 UnprojectVector2(Vector2 v2, float offset = 0)
        {
            if (_renderPlaneAxis == PlaneAxis.XYSideScroll)
                return new Vector3(v2.x, v2.y, offset);
            else
                return new Vector3(v2.x, offset, v2.y);
        }

        private Vector2 ProjectVector3(Vector3 v3)
        {
            if (_renderPlaneAxis == PlaneAxis.XYSideScroll)
                return new Vector2(v3.x, v3.y);
            else
                return new Vector2(v3.x, v3.z);
        }

        private IEnumerator InitCamBoundariesDelayed()
        {
            yield return null;
            ComputeCameraBoundaries();
        }

        /// <summary>
        /// Method that does all the computation necessary when the pinch gesture of the user
        /// has changed.
        /// </summary>
        private void UpdatePinch(float deltaTime)
        {
            if (IsPinching)
            {
                if (_isTiltModeEvaluated)
                {
                    if (_isPinchTiltMode || !_isPinchModeExclusive)
                    {
                        // Tilt.
                        float pinchTiltDelta = _pinchTiltLastFrame - _pinchTiltCurrent;
                        UpdateCameraTilt(pinchTiltDelta * _pinchTiltSpeed);
                        _pinchTiltLastFrame = _pinchTiltCurrent;
                    }

                    if (!_isPinchTiltMode || !_isPinchModeExclusive)
                    {

                        if (_isRotationActivated && _isRotationLock && Mathf.Abs(_pinchAngleCurrent) >= _rotationLockThreshold)
                            _isRotationLock = false;

                        if (IsSmoothingEnabled)
                        {
                            float lerpFactor = Mathf.Clamp01(Time.unscaledDeltaTime * _camFollowFactor);
                            _pinchDistanceCurrentLerp = Mathf.Lerp(_pinchDistanceCurrentLerp, _pinchDistanceCurrent, lerpFactor);
                            _pinchCenterCurrentLerp = Vector3.Lerp(_pinchCenterCurrentLerp, _pinchCenterCurrent, lerpFactor);
                            if (!_isRotationLock)
                                _pinchAngleCurrentLerp = Mathf.Lerp(_pinchAngleCurrentLerp, _pinchAngleCurrent, lerpFactor);
                        }
                        else
                        {
                            _pinchDistanceCurrentLerp = _pinchDistanceCurrent;
                            _pinchCenterCurrentLerp = _pinchCenterCurrent;
                            if (!_isRotationLock)
                                _pinchAngleCurrentLerp = _pinchAngleCurrent;
                        }

                        // Rotation.
                        if (_isRotationActivated && !_isRotationLock)
                        {
                            float pinchAngleDelta = _pinchAngleCurrentLerp - _pinchAngleLastFrame;
                            Vector3 rotationAxis = GetRotationAxis();
                            transform.RotateAround(_pinchCenterCurrent, rotationAxis, pinchAngleDelta);
                            _pinchAngleLastFrame = _pinchAngleCurrentLerp;
                            ComputeCameraBoundaries();
                        }

                        // Zoom.
                        float zoomFactor = (_pinchDistanceStart / Mathf.Max(((_pinchDistanceCurrentLerp - _pinchDistanceStart) * _customZoomSensitivity) + _pinchDistanceStart, 0.0001f));
                        float cameraSize = _pinchStartCameraZoomSize * zoomFactor;
                        cameraSize = Mathf.Clamp(cameraSize, _cameraZoomMin - _cameraOverzoomMargin, _cameraZoomMax + _cameraOverzoomMargin);
                        if (_enableZoomTilt)
                            UpdateTiltForAutoTilt(cameraSize);
                        CameraZoomValue = cameraSize;
                    }

                    // Position update.
                    DoPositionUpdateForTilt(false);
                }
            }
            else
            {
                // Spring back.
                if (_enableTilt && _enableOvertiltSpring)
                {
                    float overtiltSpringValue = ComputeOvertiltSpringBackFactor(_cameraOvertiltMargin);
                    if (Mathf.Abs(overtiltSpringValue) > _minOvertiltSpringPositionThreshold)
                    {
                        UpdateCameraTilt(overtiltSpringValue * deltaTime * _tiltBackSpringFactor);
                        DoPositionUpdateForTilt(true);
                    }
                }
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
        private void UpdateScroll()
        {
            float mouseScrollDelta = Mouse.current.scroll.ReadValue().y * _mouseZoomFactor * Time.deltaTime;

            if (!Mathf.Approximately(mouseScrollDelta, 0))
            {
                float zoomAmount = mouseScrollDelta * _customZoomSensitivity;
                float cameraSizeDiff = DoEditorCameraZoom(zoomAmount);
                Vector3 intersectionScreenCenter = GetIntersectionPoint(Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0)));
                Vector3 pinchFocusVector = GetIntersectionPoint(Camera.ScreenPointToRay(Mouse.current.position.ReadValue())) - intersectionScreenCenter;
                float multiplier = (1.0f / CameraZoomValue * cameraSizeDiff);
                transform.position += pinchFocusVector * multiplier;
            }
        }
#endif

        private void SnapZoom()
        {
            if (!IsPinching && !IsDragging)
            {
                float cameraZoomDeltaToNormal = 0;
                if (CameraZoomValue > _cameraZoomMax)
                    cameraZoomDeltaToNormal = CameraZoomValue - _cameraZoomMax;
                else if (CameraZoomValue < _cameraZoomMin)
                    cameraZoomDeltaToNormal = CameraZoomValue - _cameraZoomMin;

                if (!Mathf.Approximately(cameraZoomDeltaToNormal, 0))
                {
                    float cameraSizeCorrection = Mathf.Lerp(0, cameraZoomDeltaToNormal, _zoomBackSpringFactor * Time.unscaledDeltaTime);
                    if (Mathf.Abs(cameraSizeCorrection) > Mathf.Abs(cameraZoomDeltaToNormal))
                        cameraSizeCorrection = cameraZoomDeltaToNormal;
                    CameraZoomValue -= cameraSizeCorrection;
                }
            }

            _cameraVelocity = (transform.position - _lastPosition) / Time.unscaledDeltaTime;
            _lastPosition = transform.position;
        }

        private void UpdateTiltForAutoTilt(float newCameraSize)
        {
            float zoomProgress = Mathf.Clamp01((newCameraSize - _cameraZoomMin) / (_cameraZoomMax - _cameraZoomMin));
            float tiltTarget = Mathf.Lerp(_zoomTiltAngleMin, _zoomTiltAngleMax, zoomProgress);
            float tiltAngleDiff = tiltTarget - GetCurrentTiltAngleDeg(GetTiltRotationAxis());
            UpdateCameraTilt(tiltAngleDiff);
        }

        /// <summary>
        /// Method that computes the updated camera position when the user tilts the camera.
        /// </summary>
        private void DoPositionUpdateForTilt(bool isSpringBack)
        {
            // Position update.
            Vector3 intersectionDragCurrent;
            if (isSpringBack || (_isPinchTiltMode && _isPinchModeExclusive))
                intersectionDragCurrent = GetIntersectionPoint(GetCameraCenterRay()); //In exclusive tilt mode always rotate around the screen center.
            else
                intersectionDragCurrent = GetIntersectionPoint(Camera.ScreenPointToRay(_pinchCenterCurrentLerp));

            Vector3 dragUpdateVector = intersectionDragCurrent - _pinchStartIntersectionCenter;
            if (isSpringBack && !_isPinchModeExclusive)
                dragUpdateVector = Vector3.zero;

            Vector3 targetPosition = GetClampToBoundaries(transform.position - dragUpdateVector);
            transform.position = targetPosition; //Disable smooth follow for the pinch-move update to prevent oscillation during the zoom phase.
            SetTargetPosition(targetPosition);
        }

        /// <summary>
        /// Helper method for computing the tilt spring back.
        /// </summary>
        private float ComputeOvertiltSpringBackFactor(float margin)
        {
            float springBackValue = 0;
            Vector3 rotationAxis = GetTiltRotationAxis();
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);

            if (tiltAngle < _tiltAngleMin + margin)
                springBackValue = (_tiltAngleMin + margin) - tiltAngle;
            else if (tiltAngle > _tiltAngleMax - margin)
                springBackValue = (_tiltAngleMax - margin) - tiltAngle;

            return springBackValue;
        }

        /// <summary>
        /// Method that computes all necessary parameters for a tilt update caused by the user's tilt gesture.
        /// </summary>
        private void UpdateCameraTilt(float angle)
        {
            Vector3 rotationAxis = GetTiltRotationAxis();
            Vector3 rotationPoint = GetIntersectionPoint(new Ray(transform.position, transform.forward));
            transform.RotateAround(rotationPoint, rotationAxis, angle);
            ClampCameraTilt(rotationPoint, rotationAxis);
            ComputeCameraBoundaries();
        }

        /// <summary>
        /// Method that ensures that all limits are met when the user tilts the camera.
        /// </summary>
        private void ClampCameraTilt(Vector3 rotationPoint, Vector3 rotationAxis)
        {
            float tiltAngle = GetCurrentTiltAngleDeg(rotationAxis);
            if (tiltAngle < _tiltAngleMin)
            {
                float tiltClampDiff = _tiltAngleMin - tiltAngle;
                transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
            else if (tiltAngle > _tiltAngleMax)
            {
                float tiltClampDiff = _tiltAngleMax - tiltAngle;
                transform.RotateAround(rotationPoint, rotationAxis, tiltClampDiff);
            }
        }

        /// <summary>
        /// Method to get the current tilt angle of the camera.
        /// </summary>
        private float GetCurrentTiltAngleDeg(Vector3 rotationAxis)
        {
            Vector3 cameraForwardOnPlane = Vector3.Cross(ReferencePlane.normal, rotationAxis);
            float tiltAngle = Vector3.Angle(cameraForwardOnPlane, -transform.forward);
            return tiltAngle;
        }

        /// <summary>
        /// Returns the rotation axis of the camera. This purely depends
        /// on the defined camera axis.
        /// </summary>
        private Vector3 GetRotationAxis()
            => ReferencePlane.normal;

        /// <summary>
        /// Returns the tilt rotation axis.
        /// </summary>
        private Vector3 GetTiltRotationAxis()
            => transform.right;

        /// <summary>
        /// Method to compute all the necessary updates when the user moves the camera.
        /// </summary>
        private void UpdatePosition(float deltaTime)
        {
            if (IsPinching && _isPinchTiltMode)
                return;

            if (isControlledByOthers)
            {
                transform.position += controlledByOthersMoveDelta;
            }
            else
            {
                if (IsDragging || IsPinching)
                {
                    Vector3 lastPosition = transform.position;
                    if (IsSmoothingEnabled)
                        transform.position = Vector3.Lerp(transform.position, _targetPositionClamped, Mathf.Clamp01(Time.unscaledDeltaTime * _camFollowFactor));
                    else
                        transform.position = _targetPositionClamped;

                    DragCameraMoveVector.Add((lastPosition - transform.position) / Time.unscaledDeltaTime);
                    if (DragCameraMoveVector.Count > MOMENTUM_SAMPLES_COUNT)
                        DragCameraMoveVector.RemoveAt(0);
                }
            }

            Vector2 autoScrollVector = -_cameraScrollVelocity * deltaTime;
            Vector3 cameraPosition = transform.position;

            switch (_renderPlaneAxis)
            {
                case PlaneAxis.XYSideScroll:
                    cameraPosition.x += autoScrollVector.x;
                    cameraPosition.y += autoScrollVector.y;
                    break;

                case PlaneAxis.XZTopDown:
                    cameraPosition.x += autoScrollVector.x;
                    cameraPosition.z += autoScrollVector.y;
                    break;
            }

            if (!IsDragging && !IsPinching)
            {
                Vector3 overdragSpringVector = ComputeOverdragSpringBackVector(cameraPosition, CammeraOverdragMargin2D, ref _cameraScrollVelocity);
                if (overdragSpringVector.magnitude > float.Epsilon)
                    cameraPosition += Time.unscaledDeltaTime * overdragSpringVector * _dragBackSpringFactor;
            }

            transform.position = GetClampToBoundaries(cameraPosition);
        }

        /// <summary>
        /// Computes the camera drag spring back when the user is close to a boundary.
        /// </summary>
        private Vector3 ComputeOverdragSpringBackVector(Vector3 cameraPosition, Vector2 margin, ref Vector3 currentCameraScrollVelocity)
        {
            Vector3 springBackVector = Vector3.zero;
            if (cameraPosition.x < MinCameraPosition.x + margin.x)
            {
                springBackVector.x = (MinCameraPosition.x + margin.x) - cameraPosition.x;
                currentCameraScrollVelocity.x = 0;
            }
            else if (cameraPosition.x > MaxCameraPosition.x - margin.x)
            {
                springBackVector.x = (MaxCameraPosition.x - margin.x) - cameraPosition.x;
                currentCameraScrollVelocity.x = 0;
            }

            switch (_renderPlaneAxis)
            {
                case PlaneAxis.XYSideScroll:
                    if (cameraPosition.y < MinCameraPosition.y + margin.y)
                    {
                        springBackVector.y = (MinCameraPosition.y + margin.y) - cameraPosition.y;
                        currentCameraScrollVelocity.y = 0;
                    }
                    else if (cameraPosition.y > MaxCameraPosition.y - margin.y)
                    {
                        springBackVector.y = (MaxCameraPosition.y - margin.y) - cameraPosition.y;
                        currentCameraScrollVelocity.y = 0;
                    }
                    break;

                case PlaneAxis.XZTopDown:
                    if (cameraPosition.z < MinCameraPosition.y + margin.y)
                    {
                        springBackVector.z = (MinCameraPosition.y + margin.y) - cameraPosition.z;
                        currentCameraScrollVelocity.z = 0;
                    }
                    else if (cameraPosition.z > MaxCameraPosition.y - margin.y)
                    {
                        springBackVector.z = (MaxCameraPosition.y - margin.y) - cameraPosition.z;
                        currentCameraScrollVelocity.z = 0;
                    }
                    break;
            }

            return springBackVector;
        }

        /// <summary>
        /// Internal helper method for setting the desired camera's position.
        /// </summary>
        private void SetTargetPosition(Vector3 newPositionClamped)
        {
            _targetPositionClamped = newPositionClamped;
        }

        /// <summary>
        /// Method that computes the camera's boundaries used for the current rotation and tilt of the camera.
        /// This computation is complex and needs to be invoked when the camera is rotated or tilted.
        /// </summary>
        private void ComputeCameraBoundaries()
        {
            _refPlaneXY = new Plane(Vector3.back, _groundLevelOffset);
            _refPlaneXZ = new Plane(Vector3.up, -_groundLevelOffset);

            if (_useUntransformedCameraBoundary == true)
            {
                MinCameraPosition = _boundaryMin;
                MaxCameraPosition = _boundaryMax;
            }
            else
            {
                // Get camera position projected vertically onto the ref plane. This allows to compute the offset that arises from camera tilt.
                Vector2 cameraProjectedCenter = GetIntersection2d(new Ray(transform.position, -ReferencePlane.normal));

                // Fetch camera boundary as world-space coordinates projected to the ground.
                Vector2 cameraRight = GetIntersection2d(Camera.ScreenPointToRay(new Vector3(Screen.width, Screen.height * 0.5f, 0)));
                Vector2 cameraLeft = GetIntersection2d(Camera.ScreenPointToRay(new Vector3(0, Screen.height * 0.5f, 0)));
                Vector2 cameraUp = GetIntersection2d(Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height, 0)));
                Vector2 cameraDown = GetIntersection2d(Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, 0, 0)));
                Vector2 cameraProjectedMin = GetVector2Min(cameraRight, cameraLeft, cameraUp, cameraDown);
                Vector2 cameraProjectedMax = GetVector2Max(cameraRight, cameraLeft, cameraUp, cameraDown);
                Vector2 projectionCorrectionMin = cameraProjectedCenter - cameraProjectedMin;
                Vector2 projectionCorrectionMax = cameraProjectedCenter - cameraProjectedMax;

                MinCameraPosition = _boundaryMin + projectionCorrectionMin;
                MaxCameraPosition = _boundaryMax + projectionCorrectionMax;

                Vector2 margin = CammeraOverdragMargin2D;
                if (MaxCameraPosition.x - MinCameraPosition.x < margin.x * 2)
                {
                    float midPoint = (MaxCameraPosition.x + MinCameraPosition.x) * 0.5f;
                    MaxCameraPosition = new Vector2(midPoint + margin.x, MaxCameraPosition.y);
                    MinCameraPosition = new Vector2(midPoint - margin.x, MinCameraPosition.y);
                }

                if (MaxCameraPosition.y - MinCameraPosition.y < margin.y * 2)
                {
                    float midPoint = (MaxCameraPosition.y + MinCameraPosition.y) * 0.5f;
                    MaxCameraPosition = new Vector2(MaxCameraPosition.x, midPoint + margin.y);
                    MinCameraPosition = new Vector2(MinCameraPosition.x, midPoint - margin.y);
                }
            }
        }

        /// <summary>
        /// Method for retrieving the intersection of the given ray with the defined ground
        /// in 2d space.
        /// </summary>
        private Vector2 GetIntersection2d(Ray ray)
        {
            Vector3 intersection3d = GetIntersectionPoint(ray);
            Vector2 intersection2d = new Vector2(intersection3d.x, 0);
            switch (_renderPlaneAxis)
            {
                case PlaneAxis.XYSideScroll:
                    intersection2d.y = intersection3d.y;
                    break;

                case PlaneAxis.XZTopDown:
                    intersection2d.y = intersection3d.z;
                    break;
            }

            return intersection2d;
        }

        private Vector2 GetVector2Min(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
            => new Vector2(Mathf.Min(v0.x, v1.x, v2.x, v3.x), Mathf.Min(v0.y, v1.y, v2.y, v3.y));

        private Vector2 GetVector2Max(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
            => new Vector2(Mathf.Max(v0.x, v1.x, v2.x, v3.x), Mathf.Max(v0.y, v1.y, v2.y, v3.y));

        /// <summary>
        /// Editor helper code.
        /// </summary>
        private float DoEditorCameraZoom(float amount)
        {
            float newCamZoom = CameraZoomValue - amount;
            newCamZoom = Mathf.Clamp(newCamZoom, _cameraZoomMin, _cameraZoomMax);
            float camSizeDiff = CameraZoomValue - newCamZoom;
            if (_enableZoomTilt)
                UpdateTiltForAutoTilt(newCamZoom);
            CameraZoomValue = newCamZoom;
            return (camSizeDiff);
        }

        /// <summary>
        /// Helper method used for auto scroll.
        /// </summary>
        private float EvaluateAutoScrollDampCurve(float t)
        {
            if (_autoScrollDampCurve == null || _autoScrollDampCurve.length == 0)
                return 1;

            return _autoScrollDampCurve.Evaluate(t);
        }

        private void InputControllerOnFingerDown(Vector3 psotition)
            => _cameraScrollVelocity = Vector3.zero;

        private void InputControllerOnFingerUp()
            => IsDragging = false;

        private Vector3 GetDragVector(Vector3 dragPosStart, Vector3 dragPosCurrent)
        {
            Vector3 intersectionDragStart = GetIntersectionPoint(Camera.ScreenPointToRay(dragPosStart));
            Vector3 intersectionDragCurrent = GetIntersectionPoint(Camera.ScreenPointToRay(dragPosCurrent));
            return (intersectionDragCurrent - intersectionDragStart);
        }

        /// <summary>
        /// Helper method that computes the suggested auto cam velocity from
        /// the last few frames of the user drag.
        /// </summary>
        private Vector3 GetVelocityFromMoveHistory()
        {
            Vector3 momentum = Vector3.zero;
            if (DragCameraMoveVector.Count > 0)
            {
                for (int i = 0; i < DragCameraMoveVector.Count; ++i)
                    momentum += DragCameraMoveVector[i];
                momentum /= DragCameraMoveVector.Count;
            }

            if (_renderPlaneAxis == PlaneAxis.XZTopDown)
            {
                momentum.y = momentum.z;
                momentum.z = 0;
            }

            return momentum;
        }

        private void InputControllerOnDragStart(Vector3 dragPosStart, bool isLongTap)
        {
            if (isControlledByOthers)
                return;

            _cameraScrollVelocity = Vector3.zero;
            _dragStartCameraPosition = transform.position;
            IsDragging = true;
            DragCameraMoveVector.Clear();
            SetTargetPosition(transform.position);
        }

        private void InputControllerOnDragUpdate(Vector3 dragPosStart, Vector3 dragPosCurrent, Vector3 correctionOffset)
        {
            if (isControlledByOthers)
                return;

            Vector3 dragVector = GetDragVector(dragPosStart, dragPosCurrent + correctionOffset);
            Vector3 posNewClamped = GetClampToBoundaries(_dragStartCameraPosition - dragVector);
            SetTargetPosition(posNewClamped);
        }

        private void InputControllerOnDragStop(Vector3 dragStopPos, Vector3 dragFinalMomentum)
        {
            if (isControlledByOthers)
                return;

            if (_useOldScrollDamp)
            {
                _cameraScrollVelocity = GetVelocityFromMoveHistory();
                if (_cameraScrollVelocity.sqrMagnitude >= _autoScrollVelocityMax * _autoScrollVelocityMax)
                    _cameraScrollVelocity = _cameraScrollVelocity.normalized * _autoScrollVelocityMax;
            }
            else _cameraScrollVelocity = -ProjectVector3(_cameraVelocity) * 0.5f;

            _timeRealDragStop = Time.realtimeSinceStartup;
            IsDragging = false;
            DragCameraMoveVector.Clear();
        }

        private void InputControllerOnPinchStart(Vector3 pinchCenter, float pinchDistance)
        {
            _pinchStartCameraZoomSize = CameraZoomValue;
            _pinchStartIntersectionCenter = GetIntersectionPoint(Camera.ScreenPointToRay(pinchCenter));

            _pinchCenterCurrent = pinchCenter;
            _pinchDistanceCurrent = pinchDistance;
            _pinchDistanceStart = pinchDistance;

            _pinchCenterCurrentLerp = pinchCenter;
            _pinchDistanceCurrentLerp = pinchDistance;

            SetTargetPosition(transform.position);
            IsPinching = true;
            _isRotationActivated = false;
            ResetPinchRotation(0);

            _pinchTiltCurrent = 0;
            _pinchTiltAccumulated = 0;
            _pinchTiltLastFrame = 0;
            _isTiltModeEvaluated = false;
            _isPinchTiltMode = false;

            if (!_enableTilt)
            {
                // Early out of this evaluation in case tilt is not enabled.
                _isTiltModeEvaluated = true;
            }
        }

        private void InputControllerOnPinchUpdate(PinchData pinchUpdateData)
        {
            if (_enableTilt)
            {
                _pinchTiltCurrent += pinchUpdateData.pinchTiltDelta;
                _pinchTiltAccumulated += Mathf.Abs(pinchUpdateData.pinchTiltDelta);

                if (!_isTiltModeEvaluated && pinchUpdateData.pinchTotalFingerMovement > _pinchModeDetectionMoveTreshold)
                {
                    _isPinchTiltMode = Mathf.Abs(_pinchTiltCurrent) > _pinchTiltModeThreshold;
                    _isTiltModeEvaluated = true;
                    if (_isPinchTiltMode && _isPinchModeExclusive)
                        _pinchStartIntersectionCenter = GetIntersectionPoint(GetCameraCenterRay());
                }
            }

            if (_isTiltModeEvaluated)
            {
                if (_isPinchModeExclusive)
                {
                    _pinchCenterCurrent = pinchUpdateData.pinchCenter;

                    if (_isPinchTiltMode)
                    {
                        // Evaluate a potential break-out from a tilt. Under certain tweak-settings the tilt may trigger prematurely and needs to be overrided.
                        if (_pinchTiltAccumulated < PINCH_ACCUMULATED_BREAKOUT)
                        {
                            bool breakoutZoom = Mathf.Abs(_pinchDistanceStart - pinchUpdateData.pinchDistance) > PINCH_DISTANCE_FOR_TILT_BREAKOUT;
                            bool breakoutRot = _enableRotation && Mathf.Abs(_pinchAngleCurrent) > _rotationLockThreshold;
                            if (breakoutZoom || breakoutRot)
                            {
                                InputControllerOnPinchStart(pinchUpdateData.pinchCenter, pinchUpdateData.pinchDistance);
                                _isTiltModeEvaluated = true;
                                _isPinchTiltMode = false;
                            }
                        }
                    }
                }

                _pinchDistanceCurrent = pinchUpdateData.pinchDistance;

                if (_enableRotation)
                {
                    if (Mathf.Abs(pinchUpdateData.pinchAngleDeltaNormalized) > _rotationDetectionDeltaThreshold)
                        _pinchAngleCurrent += pinchUpdateData.pinchAngleDelta;

                    if (_pinchDistanceCurrent > _rotationMinPinchDistance)
                    {
                        if (!_isRotationActivated)
                        {
                            ResetPinchRotation(0);
                            _isRotationActivated = true;
                        }
                    }
                    else _isRotationActivated = false;
                }
            }
        }

        private void ResetPinchRotation(float currentPinchRotation)
        {
            _pinchAngleCurrent = currentPinchRotation;
            _pinchAngleCurrentLerp = currentPinchRotation;
            _pinchAngleLastFrame = currentPinchRotation;
            _isRotationLock = true;
        }

        private void InputControllerOnPinchStop()
        {
            IsPinching = false;
            DragCameraMoveVector.Clear();
            _isPinchTiltMode = false;
            _isTiltModeEvaluated = false;
        }

        private Ray GetCameraCenterRay()
            => Camera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));

        #endregion Class Methods
    }
}