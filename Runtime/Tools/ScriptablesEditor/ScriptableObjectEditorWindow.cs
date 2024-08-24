#if UNITY_EDITOR

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace Runtime.Gameplay.Tools.ScriptablesEditor
{
    public class ScriptableObjectEditorWindow : EditorWindow
    {
        #region Members

        private int _selectedIndex;
        private string[] _selectionNames;
        private Type[] _types;

        #endregion Members

        #region Properties

        public Type[] Types
        {
            get => _types;
            set
            {
                _types = value;
                _selectionNames = _types.Select(t => t.Name).ToArray();
            }
        }

        #endregion Properties

        #region API Methods

        public void OnGUI()
        {
            _selectedIndex = EditorGUILayout.Popup(_selectedIndex, _selectionNames);

            if (GUILayout.Button("Create"))
            {
                var asset = CreateInstance(_types[_selectedIndex]);
                ProjectWindowUtil.StartNameEditingIfProjectWindowExists
                (
                    asset.GetInstanceID(),
                    CreateInstance<CreateAssetAfterNameEdited>(),
                    string.Format("{0}.asset", _selectionNames[_selectedIndex]),
                    AssetPreview.GetMiniThumbnail(asset),
                    null
                );

                Close();
            }
        }

        #endregion API Methods

        #region Private Classes

        private class CreateAssetAfterNameEdited : EndNameEditAction
        {
            #region Class Methods

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
            }

            #endregion Class Methods
        }

        #endregion Private Classes
    }
}

#endif