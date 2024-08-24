using UnityEngine;

namespace Runtime.UI
{
    public class ScaleY : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = new Vector3(rectTransform.localScale.x, scale, rectTransform.localScale.z);
        }

        #endregion Class Methods
    }
}