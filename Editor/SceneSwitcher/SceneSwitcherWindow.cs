using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace GameEditor.SceneSwitcher
{
    public class SceneSwitcherWindow : EditorWindow
    {
        #region Members

        private const string SCENES_CONTAINER_FOLDER_PATH = "Assets/_Game/Scenes";
        private GUIStyle _guiStyle;
        private GUIContent _guiButtonContent;
        private Vector2 _scrollPosition;
        private int _openMode;
        private List<string> _scenePaths;

        #endregion Members

        #region API Methods

        private void OnEnable()
        {
            _guiStyle = new GUIStyle();
            _guiStyle.alignment = TextAnchor.MiddleCenter;
            _guiStyle.fontSize = 18;
            _guiStyle.fontStyle = FontStyle.Bold;
            _guiButtonContent = new GUIContent();
            _scrollPosition = new Vector2(0, 0);
            GetScenePaths();
        }

        private void OnGUI()
        {
            _openMode = GUILayout.SelectionGrid(_openMode, new string[2] { "Open Scene", "Open Scene Additive" }, 2, EditorStyles.toggle);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            CreateGUIElements();
            EditorGUILayout.EndScrollView();
            Repaint();
        }

        #endregion API Methdos

        #region Class Methods

        [MenuItem("Scene Switcher/Open Scene")]
        public static void ShowSceneSwitcherWindow()
            => GetWindow(typeof(SceneSwitcherWindow), false, "Scene Switcher");

        private void GetScenePaths()
        {
            _scenePaths = new List<string>();
            var allSceneGUIDs = AssetDatabase.FindAssets("t:scene");
            for (int i = 0; i < allSceneGUIDs.Length; i++)
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(allSceneGUIDs[i]);
                if (scenePath.Contains(SCENES_CONTAINER_FOLDER_PATH))
                    _scenePaths.Add(scenePath);
            }
        }

        private void CreateGUIElements()
        {
            char[] separators = {'/','.'};
            foreach (var scenePath in _scenePaths)
            {
                string[] separatedAssetPath = scenePath.Split(separators[0]);
                string sceneName = separatedAssetPath[separatedAssetPath.Length-1].Split(separators[1])[0];
                _guiButtonContent.text = sceneName;
                _guiButtonContent.tooltip = "Scene Path: " + scenePath;
                if (GUILayout.Button(_guiButtonContent))
                {
                    EditorSceneManager.OpenScene(scenePath, (_openMode == 0) ? OpenSceneMode.Single : OpenSceneMode.Additive);
                    Close();
                }
            }
        }

        #endregion Class Methods
    }
}