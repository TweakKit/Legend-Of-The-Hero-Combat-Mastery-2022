#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Events;

namespace GameEditor.Packages
{
    public static class ImportRequiredPackages
    {
        #region Members

        private static AddRequest s_request;

        #endregion Members

        #region Properties

        private static UnityAction<string> UpdateMethodAction { get; set; }

        #endregion Properties

        #region Class Methods

        public static void ImportPackage(string packageToImport, UnityAction<string> UpdateMethod)
        {
            UpdateMethodAction = UpdateMethod;
            Debug.Log("Installation started. Please wait");
            s_request = UnityEditor.PackageManager.Client.Add(packageToImport);
            EditorApplication.update += Progress;
        }

        private static void Progress()
        {
            UpdateMethodAction(s_request.Status.ToString());
            if (s_request.IsCompleted)
            {
                if (s_request.Status == UnityEditor.PackageManager.StatusCode.Success)
                {
                    UpdateMethodAction("Installed: " + s_request.Result.packageId);
                }
                else
                {
                    if (s_request.Status >= UnityEditor.PackageManager.StatusCode.Failure)
                    {
                        Debug.Log(s_request.Error.message);
                        UpdateMethodAction(s_request.Error.message);
                    }
                }
                EditorApplication.update -= Progress;
            }
        }

        #endregion Class Methods
    }
}

#endif