using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Utilities;

namespace Runtime.InputSystem
{
    public class TouchInputController : MonoBehaviour
    {
        #region Members

        private const float DRAG_DURATION_THRESHOLD = 0.01f;
        private const int MOMENTUMS_SAMPLES_COUNT = 5;

        public delegate void InputDragStartDelegate(Vector3 position, bool isLongTap);
        public delegate void Input1PositionDelegate(Vector3 position);

        public event InputDragStartDelegate OnDragStart;
        public event Input1PositionDelegate OnFingerDown;
        public event Action OnFingerUp;

        public delegate void DragUpdateDelegate(Vector3 dragPositionStart, Vector3 dragPositionCurrent, Vector3 correctionOffset);
        public event DragUpdateDelegate OnDragUpdate;

        public delegate void DragStopDelegate(Vector3 dragStopPosition, Vector3 dragFinalMomentum);
        public event DragStopDelegate OnDragStop;

        public delegate void PinchStartDelegate(Vector3 pinchCenter, float pinchDistance);
        public event PinchStartDelegate OnPinchStart;

        public delegate void PinchUpdateDelegate(PinchData pinchUpdateData);
        public event PinchUpdateDelegate OnPinchUpdate;

        public event Action OnPinchStop;

        public delegate void InputLongTapProgress(float progress);
        public event InputLongTapProgress OnLongTapProgress;

        public delegate void InputClickDelegate(Vector3 clickPosition, bool isDoubleClick, bool isLongTap);
        public event InputClickDelegate OnInputClick;

        [SerializeField]
        [Tooltip("When the finger is held on an item for at least this duration without moving, the gesture is recognized as a long tap.")]
        private float _clickDurationThreshold = 0.7f;

        [SerializeField]
        [Tooltip("A double click gesture is recognized when the time between two consecutive taps is shorter than this duration.")]
        private float _doubleclickDurationThreshold = 0.5f;

        [SerializeField]
        [Tooltip("This value controls how close to a vertical line the user has to perform a tilt gesture for it to be recognized as such.")]
        private float _tiltMoveDotTreshold = 0.7f;

        [SerializeField]
        [Tooltip("Threshold value for detecting whether the fingers are horizontal enough for starting the tilt. Using this value you can prevent vertical finger placement to be counted as tilt gesture.")]
        private float _tiltHorizontalDotThreshold = 0.5f;

        [SerializeField]
        [Tooltip("A drag is started as soon as the user moves his finger over a longer distance than this value. The value is defined as normalized value. Dragging the entire width of the screen equals 1. Dragging the entire height of the screen also equals 1.")]
        private float _dragStartDistanceThresholdRelative = 0.05f;

        [SerializeField]
        [Tooltip("When this flag is enabled the drag started event is invoked immediately when the long tap time is succeeded.")]
        private bool _longTapStartsDrag = true;

        private float _lastFingerDownRealTime;
        private float _lastClickRealTime;
        private bool _wasFingerDownLastFrame;
        private Vector3 _lastIniialFingerDownPosition;
        private bool _isDragging;
        private Vector3 _dragStartPosition;
        private Vector3 _dragStartOffset;
        private float _pinchStartDistance;
        private List<Vector3> _pinchStartPositions;
        private List<Vector3> _touchPositionsLastFrame;
        private Vector3 _pinchRotationVectorStart = Vector3.zero;
        private Vector3 _pinchVectorLastFrame = Vector3.zero;
        private float _totalFingerMovement;
        private bool _wasDraggingLastFrame;
        private bool _wasPinchingLastFrame;
        private bool _isPinching;
        private bool _isInputOnLockedArea = false;
        private float _timeSinceDragStart = 0;
        private bool _isClickPrevented;
        private bool _isFingerDown;

        #endregion Members

        #region Properties

        private List<Vector3> DragFinalMomentumVector { get; set; }
        public bool LongTapStartsDrag { get { return _longTapStartsDrag; } }

        public bool IsInputOnLockedArea
        {
            get { return _isInputOnLockedArea; }
            set { _isInputOnLockedArea = value; }
        }

        #endregion Properties

        #region API Methods

        private void Awake()
        {
            _lastFingerDownRealTime = 0;
            _lastClickRealTime = 0;
            _lastIniialFingerDownPosition = Vector2.zero;
            _dragStartPosition = Vector3.zero;
            _isDragging = false;
            _wasFingerDownLastFrame = false;
            _pinchStartDistance = 1;
            _isPinching = false;
            _isClickPrevented = false;
            _pinchStartPositions = new List<Vector3>() { Vector3.zero, Vector3.zero };
            _touchPositionsLastFrame = new List<Vector3>() { Vector3.zero, Vector3.zero };
            DragFinalMomentumVector = new List<Vector3>();
        }

        public void Update()
        {
            if (InputUtility.IsPointerOverUIObject())
                return;

            if (!TouchInput.IsFingerDown)
                _isInputOnLockedArea = false;

            bool pinchToDragCurrentFrame = false;
            if (!_isInputOnLockedArea)
            {
                // Pinch.
                if (!_isPinching)
                {
                    if (TouchInput.TouchCount == 2)
                    {
                        _isPinching = true;
                        StartPinch();
                    }
                }
                else
                {
                    if (TouchInput.TouchCount < 2)
                    {
                        _isPinching = false;
                        StopPinch();
                    }
                    else if (TouchInput.TouchCount == 2)
                    {
                        UpdatePinch();
                    }
                }

                // Drag.
                if (!_isPinching)
                {
                    if (!_wasPinchingLastFrame)
                    {
                        if (_wasFingerDownLastFrame && TouchInput.IsFingerDown)
                        {
                            if (!_isDragging)
                            {
                                float dragDistance = GetRelativeDragDistance(TouchInput.InitialTouch.Position, _dragStartPosition);
                                float dragTime = Time.realtimeSinceStartup - _lastFingerDownRealTime;
                                bool isLongTap = dragTime > _clickDurationThreshold;

                                if (OnLongTapProgress != null)
                                {
                                    float longTapProgress = 0;
                                    if (Mathf.Approximately(_clickDurationThreshold, 0) == false)
                                        longTapProgress = Mathf.Clamp01(dragTime / _clickDurationThreshold);

                                    OnLongTapProgress(longTapProgress);
                                }

                                if ((dragDistance >= _dragStartDistanceThresholdRelative && dragTime >= DRAG_DURATION_THRESHOLD) ||
                                    (_longTapStartsDrag == true && isLongTap == true))
                                {
                                    _isDragging = true;
                                    _dragStartOffset = _lastIniialFingerDownPosition - _dragStartPosition;
                                    _dragStartPosition = _lastIniialFingerDownPosition;
                                    DragStart(_dragStartPosition, isLongTap, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (TouchInput.IsFingerDown)
                        {
                            _isDragging = true;
                            _dragStartPosition = TouchInput.InitialTouch.Position;
                            pinchToDragCurrentFrame = true;
                            DragStart(_dragStartPosition, false, false);
                        }
                    }

                    if (_isDragging)
                    {
                        if (!TouchInput.IsFingerDown)
                        {
                            _isDragging = false;
                            DragStop(_lastIniialFingerDownPosition);
                        }
                        else DragUpdate(TouchInput.InitialTouch.Position);
                    }
                }

                if (!_isPinching && !_isDragging && !_wasPinchingLastFrame && !_wasDraggingLastFrame && !_isClickPrevented)
                {
                    if (!_wasFingerDownLastFrame && TouchInput.IsFingerDown)
                    {
                        _lastFingerDownRealTime = Time.realtimeSinceStartup;
                        _dragStartPosition = TouchInput.InitialTouch.Position;
                        FingerDown(TouchInput.AverageTouchPosition);
                    }

                    if (_wasFingerDownLastFrame && !TouchInput.IsFingerDown)
                    {
                        float fingerDownUpDuration = Time.realtimeSinceStartup - _lastFingerDownRealTime;

                        if (!_wasDraggingLastFrame && !_wasPinchingLastFrame)
                        {
                            float clickDuration = Time.realtimeSinceStartup - _lastClickRealTime;
                            bool isDoubleClick = clickDuration < _doubleclickDurationThreshold;
                            bool isLongTap = fingerDownUpDuration > _clickDurationThreshold;

                            OnInputClick?.Invoke(_lastIniialFingerDownPosition, isDoubleClick, isLongTap);
                            _lastClickRealTime = Time.realtimeSinceStartup;
                        }
                    }
                }
            }

            if (_isDragging && TouchInput.IsFingerDown && !pinchToDragCurrentFrame)
            {
                DragFinalMomentumVector.Add(TouchInput.InitialTouch.Position - _lastIniialFingerDownPosition);
                if (DragFinalMomentumVector.Count > MOMENTUMS_SAMPLES_COUNT)
                    DragFinalMomentumVector.RemoveAt(0);
            }

            if (!_isInputOnLockedArea)
                _wasFingerDownLastFrame = TouchInput.IsFingerDown;

            if (_wasFingerDownLastFrame)
                _lastIniialFingerDownPosition = TouchInput.InitialTouch.Position;

            _wasDraggingLastFrame = _isDragging;
            _wasPinchingLastFrame = _isPinching;

            if (TouchInput.TouchCount == 0)
            {
                _isClickPrevented = false;
                if (_isFingerDown == true)
                    FingerUp();
            }
        }

        #endregion API Methods

        #region Class Methods

        private void StartPinch()
        {
            _pinchStartPositions[0] = _touchPositionsLastFrame[0] = TouchInput.Touches[0].Position;
            _pinchStartPositions[1] = _touchPositionsLastFrame[1] = TouchInput.Touches[1].Position;
            _pinchStartDistance = GetPinchDistance(_pinchStartPositions[0], _pinchStartPositions[1]);
            OnPinchStart?.Invoke((_pinchStartPositions[0] + _pinchStartPositions[1]) * 0.5f, _pinchStartDistance);
            _isClickPrevented = true;
            _pinchRotationVectorStart = TouchInput.Touches[1].Position - TouchInput.Touches[0].Position;
            _pinchVectorLastFrame = _pinchRotationVectorStart;
            _totalFingerMovement = 0;
        }

        private void UpdatePinch()
        {
            float pinchDistance = GetPinchDistance(TouchInput.Touches[0].Position, TouchInput.Touches[1].Position);
            Vector3 pinchVector = TouchInput.Touches[1].Position - TouchInput.Touches[0].Position;
            float pinchAngleSign = Vector3.Cross(_pinchVectorLastFrame, pinchVector).z < 0 ? -1 : 1;
            float pinchAngleDelta = 0;

            if (!Mathf.Approximately(Vector3.Distance(_pinchVectorLastFrame, pinchVector), 0))
                pinchAngleDelta = Vector3.Angle(_pinchVectorLastFrame, pinchVector) * pinchAngleSign;

            float pinchVectorDeltaMag = Mathf.Abs(_pinchVectorLastFrame.magnitude - pinchVector.magnitude);
            float pinchAngleDeltaNormalized = 0;

            if (!Mathf.Approximately(pinchVectorDeltaMag, 0))
                pinchAngleDeltaNormalized = pinchAngleDelta / pinchVectorDeltaMag;

            Vector3 pinchCenter = (TouchInput.Touches[0].Position + TouchInput.Touches[1].Position) * 0.5f;

            // Tilting gesture.
            float pinchTiltDelta = 0;
            Vector3 touch0DeltaRelative = GetTouchPositionRelative(TouchInput.Touches[0].Position - _touchPositionsLastFrame[0]);
            Vector3 touch1DeltaRelative = GetTouchPositionRelative(TouchInput.Touches[1].Position - _touchPositionsLastFrame[1]);
            float touch0DotUp = Vector2.Dot(touch0DeltaRelative.normalized, Vector2.up);
            float touch1DotUp = Vector2.Dot(touch1DeltaRelative.normalized, Vector2.up);
            float pinchVectorDotHorizontal = Vector3.Dot(pinchVector.normalized, Vector3.right);

            if (Mathf.Sign(touch0DotUp) == Mathf.Sign(touch1DotUp))
            {
                if (Mathf.Abs(touch0DotUp) > _tiltMoveDotTreshold && Mathf.Abs(touch1DotUp) > _tiltMoveDotTreshold)
                {
                    if (Mathf.Abs(pinchVectorDotHorizontal) >= _tiltHorizontalDotThreshold)
                    {
                        pinchTiltDelta = 0.5f * (touch0DeltaRelative.y + touch1DeltaRelative.y);
                    }
                }
            }

            _totalFingerMovement += touch0DeltaRelative.magnitude + touch1DeltaRelative.magnitude;
            PinchData pinchUpdateData = new PinchData()
            {
                pinchCenter = pinchCenter,
                pinchDistance = pinchDistance,
                pinchStartDistance = _pinchStartDistance,
                pinchAngleDelta = pinchAngleDelta,
                pinchAngleDeltaNormalized = pinchAngleDeltaNormalized,
                pinchTiltDelta = pinchTiltDelta,
                pinchTotalFingerMovement = _totalFingerMovement
            };

            OnPinchUpdate?.Invoke(pinchUpdateData);
            _pinchVectorLastFrame = pinchVector;
            _touchPositionsLastFrame[0] = TouchInput.Touches[0].Position;
            _touchPositionsLastFrame[1] = TouchInput.Touches[1].Position;
        }

        private float GetPinchDistance(Vector3 pos0, Vector3 pos1)
        {
            float distanceX = Mathf.Abs(pos0.x - pos1.x) / Screen.width;
            float distanceY = Mathf.Abs(pos0.y - pos1.y) / Screen.height;
            return (Mathf.Sqrt(distanceX * distanceX + distanceY * distanceY));
        }

        private void StopPinch()
        {
            _dragStartOffset = Vector3.zero;
            OnPinchStop?.Invoke();
        }

        private void DragStart(Vector3 position, bool isLongTap, bool isInitialDrag)
        {
            OnDragStart?.Invoke(position, isLongTap);
            _isClickPrevented = true;
            _timeSinceDragStart = 0;
            DragFinalMomentumVector.Clear();
        }

        private void DragUpdate(Vector3 currentDragPosition)
        {
            if (OnDragUpdate != null)
            {
                _timeSinceDragStart += Time.unscaledDeltaTime;
                Vector3 offset = Vector3.Lerp(Vector3.zero, _dragStartOffset, Mathf.Clamp01(_timeSinceDragStart * 10.0f));
                OnDragUpdate(_dragStartPosition, currentDragPosition, offset);
            }
        }

        private void DragStop(Vector3 stopDragPosition)
        {
            if (OnDragStop != null)
            {
                Vector3 momentum = Vector3.zero;
                if (DragFinalMomentumVector.Count > 0)
                {
                    for (int i = 0; i < DragFinalMomentumVector.Count; ++i)
                        momentum += DragFinalMomentumVector[i];
                    momentum /= DragFinalMomentumVector.Count;
                }

                OnDragStop(stopDragPosition, momentum);
            }

            DragFinalMomentumVector.Clear();
        }

        private void FingerDown(Vector3 pos)
        {
            _isFingerDown = true;
            OnFingerDown?.Invoke(pos);
        }

        private void FingerUp()
        {
            _isFingerDown = false;
            OnFingerUp?.Invoke();
        }

        private Vector3 GetTouchPositionRelative(Vector3 touchPosition)
            => new Vector3(touchPosition.x / (float)Screen.width, touchPosition.y / (float)Screen.height, touchPosition.z);

        private float GetRelativeDragDistance(Vector3 position1, Vector3 position2)
        {
            Vector2 dragVector = position1 - position2;
            float dragDistance = new Vector2(dragVector.x /  Screen.width, dragVector.y /  Screen.height).magnitude;
            return dragDistance;
        }

        #endregion Class Methods
    }
}