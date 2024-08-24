using UnityEngine;

namespace Runtime.Gameplay.Tools.FPS
{
    public class FPSImprover : MonoBehaviour
    {
        #region API Methods

        private void Awake()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        #endregion API Methods
    }
}