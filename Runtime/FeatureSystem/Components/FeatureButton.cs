using UnityEngine;
using Runtime.Core.UI;
using Runtime.Manager;

namespace Runtime.FeatureSystem
{
    [RequireComponent(typeof(UnlockFeature))]
    public class FeatureButton : PerfectButton, IUnlockable
    {
        #region Members

        protected UnlockFeature unlockFeature;

        #endregion Members

        #region Properties

        public bool IsUnlocked => unlockFeature.IsUnlocked;
        public string RequiredUnlockDescription => unlockFeature.UnlockDescription;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            unlockFeature = gameObject.GetComponentInChildren<UnlockFeature>();
        }

        #endregion API Methods

        #region Class Methods

        protected override bool Press()
        {
            if (!IsUnlocked)
            {
                ToastController.Instance.Show(RequiredUnlockDescription);
                return false;
            }
            else return base.Press();
        }

        #endregion Class Methods
    }
}