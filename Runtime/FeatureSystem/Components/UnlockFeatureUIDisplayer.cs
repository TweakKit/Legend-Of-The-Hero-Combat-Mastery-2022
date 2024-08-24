using UnityEngine;
using Coffee.UIEffects;

namespace Runtime.FeatureSystem
{
    public class UnlockFeatureUIDisplayer : UnlockFeature
    {
        #region Members

        [SerializeField]
        private UIEffect _disableEffect;
        [SerializeField]
        private GameObject _lockGameObject;
        [SerializeField]
        private GameObject[] _disableObjectsWhenLock;
        [SerializeField]
        private bool _disable;

        #endregion Members

        #region Class Methods

        public override void UpdateStatus(bool isUnlocked)
        {
            base.UpdateStatus(isUnlocked);
            _disableEffect.enabled = !isUnlocked;
            _lockGameObject.SetActive(!isUnlocked);
            if (_disable)
                gameObject.SetActive(isUnlocked);
            foreach (var disableObjectWhenLock in _disableObjectsWhenLock)
                disableObjectWhenLock.SetActive(isUnlocked);
        }

        #endregion Class Methods
    }
}