using UnityEngine;
using TMPro;

namespace Runtime.UI
{
    public class ToastDurationType
    {
        #region Members

        public const float SHORT = 0.5f;
        public const float MEDIUM = 1.2f;
        public const float LONG = 2f;

        #endregion Members
    }

    public struct ToastActivityData
    {
        #region Members

        public string content;

        #endregion Members

        #region Struct Methods

        public ToastActivityData(string content) => this.content = content;

        #endregion Struct Methods
    }

    public class ToastActivity : Activity<ToastActivityData>
    {
        #region Members

        [SerializeField]
        private TextMeshProUGUI _txtMessage;

        #endregion Members

        #region Class Methods

        public override void Init(ToastActivityData activityData) => _txtMessage.text = activityData.content;

        #endregion Class Methods
    }
}