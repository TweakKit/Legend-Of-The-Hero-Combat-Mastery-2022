using UnityEngine;
using UnityEditor;

namespace GameEditor.PlayerPref
{
    public class DeletePrefs
    {
        #region Class Methods

        [MenuItem("Window/Delete Editor Prefs", false, 1)]
        public static void DeleteEditorPrefs()
        {
            EditorPrefs.DeleteAll();
            Debug.Log("Delete all editor prefs");
        }

        [MenuItem("Window/Delete Player Prefs", false, 2)]
        public static void DeletePlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Delete all player prefs");
        }

        #endregion Class Methods
    }
}