using UnityEngine;

namespace Runtime.UI
{
    public class ScaleX : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = new Vector3(scale, rectTransform.localScale.y, rectTransform.localScale.z);
        }

        #endregion Class Methods
    }
}