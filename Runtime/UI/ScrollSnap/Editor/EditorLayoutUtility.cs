using UnityEditor;
using UnityEngine;

namespace GameEditor.UI
{
    public class EditorLayoutUtility
    {
        #region Class Methods

        public static void Header(ref bool show, GUIContent content)
        {
            GUIStyle style = new GUIStyle(EditorStyles.foldout)
            {
                fontStyle = FontStyle.Bold
            };
            show = EditorGUILayout.Foldout(show, content, true, style);
        }

        #endregion Class Methods
    }
}