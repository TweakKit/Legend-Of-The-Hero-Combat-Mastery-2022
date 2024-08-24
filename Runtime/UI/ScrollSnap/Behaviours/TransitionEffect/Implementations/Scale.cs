using UnityEngine;

namespace Runtime.UI
{
    public class Scale : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = Vector3.one * scale;
        }

        #endregion Class Methods
    }
}