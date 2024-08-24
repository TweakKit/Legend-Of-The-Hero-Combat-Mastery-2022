using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.SceneLoading
{
    [Serializable]
    public class SceneLoadingInfo
    {
        #region Members

        public string sceneStartNameFormat = "Scene Name";
        [TextArea(2, 4)]
        public string description = "";
        public float sceneLoadingMinTime;
        public float fadeInDelay;
        public float fadeInSpeed;
        public float fadeOutDelay;
        public float fadeOutSpeed;
        public List<string> sceneLoadingTips;
        public List<Sprite> backgrounds;

        #endregion Members

        #region Properties

        public string LoadedSceneName { get; private set; }

        #endregion Properties

        #region Class Methods

        public void UpdateLoadedSceneName(string loadedSceneName)
            => LoadedSceneName = loadedSceneName;

        #endregion Class Methods
    }
}