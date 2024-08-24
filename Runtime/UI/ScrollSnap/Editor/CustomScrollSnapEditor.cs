using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Runtime.UI;

namespace GameEditor.UI
{
    [CustomEditor(typeof(CustomScrollSnapEditor))]
    public class CustomScrollSnapEditor : Editor
    {
        #region Members

        private bool _showMovementAndLayoutSettings = true;
        private bool _showNavigationSettings = true;
        private bool _showSnapSettings = true;
        private bool _showEvents = false;

        private SerializedProperty _movementType;
        private SerializedProperty _movementAxis;
        private SerializedProperty _useAutomaticLayout;
        private SerializedProperty _sizeControl;
        private SerializedProperty _size;
        private SerializedProperty _automaticLayoutSpacing;
        private SerializedProperty _automaticLayoutMargins;
        private SerializedProperty _useInfiniteScrolling;
        private SerializedProperty _infiniteScrollingSpacing;
        private SerializedProperty _useOcclusionCulling;
        private SerializedProperty _startingPanel;
        private SerializedProperty _useSwipeGestures;
        private SerializedProperty _minimumSwipeSpeed;
        private SerializedProperty _previousButton;
        private SerializedProperty _nextButton;
        private SerializedProperty _pagination;
        private SerializedProperty _useToggleNavigation;
        private SerializedProperty _snapTarget;
        private SerializedProperty _snapSpeed;
        private SerializedProperty _thresholdSpeedToSnap;
        private SerializedProperty _useHardSnapping;
        private SerializedProperty _useUnscaledTime;
        private SerializedProperty _onTransitionEffects;
        private SerializedProperty _onPanelSelecting;
        private SerializedProperty _onPanelSelected;
        private SerializedProperty _onPanelCentering;
        private SerializedProperty _onPanelCentered;

        private CustomScrollSnap _customScrollSnap;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            _customScrollSnap = target as CustomScrollSnap;

            // Movement and Layout Settings.
            _movementType = serializedObject.FindProperty("movementType");
            _movementAxis = serializedObject.FindProperty("movementAxis");
            _useAutomaticLayout = serializedObject.FindProperty("useAutomaticLayout");
            _sizeControl = serializedObject.FindProperty("sizeControl");
            _size = serializedObject.FindProperty("size");
            _automaticLayoutSpacing = serializedObject.FindProperty("automaticLayoutSpacing");
            _automaticLayoutMargins = serializedObject.FindProperty("automaticLayoutMargins");
            _useInfiniteScrolling = serializedObject.FindProperty("useInfiniteScrolling");
            _infiniteScrollingSpacing = serializedObject.FindProperty("infiniteScrollingSpacing");
            _useOcclusionCulling = serializedObject.FindProperty("useOcclusionCulling");
            _startingPanel = serializedObject.FindProperty("startingPanel");

            // Navigation Settings.
            _useSwipeGestures = serializedObject.FindProperty("useSwipeGestures");
            _minimumSwipeSpeed = serializedObject.FindProperty("minimumSwipeSpeed");
            _previousButton = serializedObject.FindProperty("previousButton");
            _nextButton = serializedObject.FindProperty("nextButton");
            _pagination = serializedObject.FindProperty("pagination");
            _useToggleNavigation = serializedObject.FindProperty("useToggleNavigation");

            // Snap Settings.
            _snapTarget = serializedObject.FindProperty("snapTarget");
            _snapSpeed = serializedObject.FindProperty("snapSpeed");
            _thresholdSpeedToSnap = serializedObject.FindProperty("thresholdSpeedToSnap");
            _useHardSnapping = serializedObject.FindProperty("useHardSnapping");
            _useUnscaledTime = serializedObject.FindProperty("useUnscaledTime");

            // ShowEvents.
            _onTransitionEffects = serializedObject.FindProperty("onTransitionEffects");
            _onPanelSelecting = serializedObject.FindProperty("onPanelSelecting");
            _onPanelSelected = serializedObject.FindProperty("onPanelSelected");
            _onPanelCentering = serializedObject.FindProperty("onPanelCentering");
            _onPanelCentered = serializedObject.FindProperty("onPanelCentered");
        }

        #endregion API Methods

        #region Class Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ShowMovementAndLayoutSettings();
            ShowNavigationSettings();
            ShowSnapSettings();
            ShowEvents();

            serializedObject.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(_customScrollSnap);
        }

        private void ShowMovementAndLayoutSettings()
        {
            EditorGUILayout.Space();

            EditorLayoutUtility.Header(ref _showMovementAndLayoutSettings, new GUIContent("Movement and Layout Settings"));
            if (_showMovementAndLayoutSettings)
            {
                ShowStartingPanel();
                ShowMovementType();
            }
            EditorGUILayout.Space();
        }

        private void ShowStartingPanel()
        {
            EditorGUILayout.IntSlider(_startingPanel, 0, _customScrollSnap.NumberOfPanels - 1, new GUIContent("Starting Panel", "The number of the panel that will be displayed first, based on a 0-indexed array."));
        }

        private void ShowMovementType()
        {
            EditorGUILayout.PropertyField(_movementType, new GUIContent("Movement Type", "Determines how users will be able to move between panels within the ScrollRect."));
            if (_customScrollSnap.MovementType == MovementType.Fixed)
            {
                EditorGUI.indentLevel++;

                ShowMovementAxis();
                ShowUseAutomaticLayout();
                ShowUseInfiniteScrolling();
                ShowUseOcclusionCulling();

                EditorGUI.indentLevel--;
            }
        }

        private void ShowMovementAxis()
        {
            EditorGUILayout.PropertyField(_movementAxis, new GUIContent("Movement Axis", "Determines the axis the user's movement will be restricted to."));
        }

        private void ShowUseAutomaticLayout()
        {
            EditorGUILayout.PropertyField(_useAutomaticLayout, new GUIContent("Use Automatic Layout", "Should panels be automatically positioned and scaled according to the specified movement axis, spacing, margins and size?"));
            if (_customScrollSnap.UseAutomaticLayout)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_sizeControl, new GUIContent("Size Control", "Determines how the panels' size should be controlled."));
                if (_customScrollSnap.SizeControl == SizeControl.Manual)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(_size, new GUIContent("Size", "The size (in pixels) that panels will be when automatically laid out."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.Slider(_automaticLayoutSpacing, 0, 1, new GUIContent("Spacing", "The spacing between panels, calculated using a fraction of the panel’s width (if the movement axis is horizontal) or height (if the movement axis is vertical)."));
                EditorGUILayout.PropertyField(_automaticLayoutMargins, new GUIContent("Margins"));
                EditorGUI.indentLevel--;
            }
        }

        private void ShowUseInfiniteScrolling()
        {
            EditorGUILayout.PropertyField(_useInfiniteScrolling, new GUIContent("Use Infinite Scrolling", "Should panels wrap around to the opposite end once passed, giving the illusion of an infinite list of elements?"));
            if (_customScrollSnap.UseInfiniteScrolling)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(_infiniteScrollingSpacing, 0, 1, new GUIContent("End Spacing", "The spacing maintained between panels once wrapped around to the opposite end."));
                EditorGUI.indentLevel--;
            }
        }

        private void ShowUseOcclusionCulling()
        {
            EditorGUILayout.PropertyField(_useOcclusionCulling, new GUIContent("Use Occlusion Culling", "Should panels not visible in the viewport be disabled?"));
        }

        private void ShowNavigationSettings()
        {
            EditorLayoutUtility.Header(ref _showNavigationSettings, new GUIContent("Navigation Settings"));
            if (_showNavigationSettings)
            {
                ShowUseSwipeGestures();
                ShowPreviousButton();
                ShowNextButton();
                ShowPagination();
            }
            EditorGUILayout.Space();
        }

        private void ShowUseSwipeGestures()
        {
            EditorGUILayout.PropertyField(_useSwipeGestures, new GUIContent("Use Swipe Gestures", "Should users are able to use swipe gestures to navigate between panels?"));
            if (_customScrollSnap.UseSwipeGestures)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_minimumSwipeSpeed, new GUIContent("Minimum Swipe Speed", "The speed at which the user must be swiping in order for a transition to occur to another panel."));
                EditorGUI.indentLevel--;
            }
        }

        private void ShowPreviousButton()
        {
            EditorGUILayout.ObjectField(_previousButton, typeof(Button), new GUIContent("Previous Button", "(Optional) Button used to transition to the previous panel."));
        }

        private void ShowNextButton()
        {
            EditorGUILayout.ObjectField(_nextButton, typeof(Button), new GUIContent("Next Button", "(Optional) Button used to transition to the next panel."));
        }

        private void ShowPagination()
        {
            EditorGUILayout.ObjectField(_pagination, typeof(ToggleGroup), new GUIContent("Pagination", "(Optional) ToggleGroup containing Toggles that shows the current position of the user and can be used to transition to a selected panel."));
            if (_customScrollSnap.Pagination != null)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_useToggleNavigation, new GUIContent("Toggle Navigation", "Should users be able to transition to panels by clicking on their respective toggle."));
                int numberOfToggles = _customScrollSnap.Pagination.transform.childCount;
                if (numberOfToggles != _customScrollSnap.NumberOfPanels)
                {
                    EditorGUILayout.HelpBox("The number of toggles should be equivalent to the number of panels. There are currently " + numberOfToggles + " toggles and " + _customScrollSnap.NumberOfPanels + " panels.", MessageType.Warning);
                }
                EditorGUI.indentLevel--;
            }
        }

        private void ShowSnapSettings()
        {
            EditorLayoutUtility.Header(ref _showSnapSettings, new GUIContent("Snap Settings"));
            if (_showSnapSettings)
            {
                ShowSnapTarget();
                ShowSnapSpeed();
                ShowThresholdSpeedToSnap();
                ShowUseHardSnapping();
                ShowUseUnscaledTime();
            }
            EditorGUILayout.Space();
        }

        private void ShowSnapTarget()
        {
            using (new EditorGUI.DisabledScope(_customScrollSnap.MovementType == MovementType.Free))
            {
                EditorGUILayout.PropertyField(_snapTarget, new GUIContent("Snap Target", "Determines what panel should be targeted and snapped to once the threshold snapping speed has been reached."));
            }
            if (_customScrollSnap.MovementType == MovementType.Free)
            {
                _customScrollSnap.SnapTarget = SnapTarget.Nearest;
            }
        }

        private void ShowSnapSpeed()
        {
            EditorGUILayout.PropertyField(_snapSpeed, new GUIContent("Snap Speed", "The speed at which the targeted panel snaps into position."));
        }

        private void ShowThresholdSpeedToSnap()
        {
            EditorGUILayout.PropertyField(_thresholdSpeedToSnap, new GUIContent("Threshold Speed To Snap", "The speed at which the ScrollRect will stop scrolling and begin snapping to the targeted panel (where -1 is used as infinity)."));
        }

        private void ShowUseHardSnapping()
        {
            EditorGUILayout.PropertyField(_useHardSnapping, new GUIContent("Use Hard Snapping", "Should the inertia of the ScrollRect be disabled once a panel has been selected? If enabled, the ScrollRect will not overshoot the targeted panel when snapping into position and instead Lerp precisely towards the targeted panel."));
        }

        private void ShowUseUnscaledTime()
        {
            EditorGUILayout.PropertyField(_useUnscaledTime, new GUIContent("Use Unscaled Time", "Should the scroll-snap update irrespective of the time scale?"));
        }

        private void ShowEvents()
        {
            EditorLayoutUtility.Header(ref _showEvents, new GUIContent("Events"));
            if (_showEvents)
            {
                EditorGUILayout.PropertyField(_onTransitionEffects);
                EditorGUILayout.PropertyField(_onPanelSelecting);
                EditorGUILayout.PropertyField(_onPanelSelected);
                EditorGUILayout.PropertyField(_onPanelCentering);
                EditorGUILayout.PropertyField(_onPanelCentered);
            }
        }

        #endregion Class Methods
    }
}