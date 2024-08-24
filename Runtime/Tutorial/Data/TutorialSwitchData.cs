using System;
using Sirenix.OdinInspector;

namespace Runtime.Tutorial
{
    [Serializable]
    public class TutorialSwitchData
    {
        #region Members

        public SwitchType switchType;
        [ShowIf(nameof(switchType), SwitchType.Index)]
        public int index;
        [ShowIf(nameof(switchType), SwitchType.Name)]
        public string name;

        #endregion Members
    }
}