using System;
using UnityEngine;
using Runtime.Manager;
using Runtime.Extensions;

namespace Runtime.UI
{
    public abstract class PanelData
    {
        #region Members

        public Action<PanelEnterData> switchToPanelAction;

        #endregion Members

        #region Class Methods

        public PanelData(Action<PanelEnterData> switchToPanelAction)
            => this.switchToPanelAction = switchToPanelAction;

        #endregion Class Methods
    }

    public class PanelEnterData
    {
        #region Members

        public string switcher;

        #endregion Members

        #region Class Methods

        public PanelEnterData(string switcher)
            => this.switcher = switcher;

        #endregion Class Methods
    }

    /// <summary>
    /// Panel is contained within a screen/sheet/modal,... Use this as a child object of those containers.
    /// Panel can be hiden, shown, and can be attached actions to in order to switch to another panel when interacting with something there.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Panel<T1> : MonoBehaviour where T1 : PanelData
    {
        #region Members

        protected CanvasGroup ownerCanvasGroup;
        protected Action<PanelEnterData> switchToPanelAction;

        #endregion Members

        #region API Methods

        protected virtual void Awake()
        {
            ownerCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (ownerCanvasGroup == null)
                ownerCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;

            ownerCanvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (ownerCanvasGroup == null)
                ownerCanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
#endif

        #endregion API Methods

        #region Class Methods

        public virtual void Init(T1 panelData)
        {
            this.switchToPanelAction = panelData.switchToPanelAction;
            ownerCanvasGroup.SetActive(true);
        }

        public virtual void Enter(PanelEnterData panelEnterData)
            => Show();

        public virtual void Show()
            => ownerCanvasGroup.SetActive(true);

        public virtual void Hide()
            => ownerCanvasGroup.SetActive(false);

        public virtual void Dispose() { }

        public virtual void ShowToast(string toastMessage)
            => ToastController.Instance.Show(toastMessage);

        protected virtual void SwitchToPanel(PanelEnterData panelEnterData)
            => switchToPanelAction?.Invoke(panelEnterData);

        /// <summary>
        /// Used for parent with sheet type only (used in DidEnter).
        /// </summary>
        public virtual void ParentEntered() { }

        /// <summary>
        /// Used for parent with sheet type only (used in DidExit).
        /// </summary>
        public virtual void ParentExited() { }

        #endregion Class Methods
    }
}