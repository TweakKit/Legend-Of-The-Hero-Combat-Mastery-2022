using System;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.FeatureSystem
{
    public abstract class UnlockFeature : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private FeatureType _featureType;

        #endregion Members

        #region Propperties

        public FeatureType FeatureType => _featureType;
        public bool IsUnlocked { get; private set; }
        public string UnlockDescription { get; private set; }
        private Action<bool> OnUpdateLockStatusCallbackAction { get; set; }

        #endregion Properties;

        #region Class Methods

        public virtual void Init(bool isUnlocked, string unlockDesription, Action<bool> onUpdateLockStatusCallbackAction = null)
        {
            IsUnlocked = isUnlocked;
            UnlockDescription = unlockDesription;
            OnUpdateLockStatusCallbackAction = onUpdateLockStatusCallbackAction;
            UpdateStatus(isUnlocked);
        }

        public virtual void UpdateStatus(bool isUnlocked)
        {
            IsUnlocked = isUnlocked;
            OnUpdateLockStatusCallbackAction?.Invoke(isUnlocked);
        }

        #endregion Class Methods
    }
}