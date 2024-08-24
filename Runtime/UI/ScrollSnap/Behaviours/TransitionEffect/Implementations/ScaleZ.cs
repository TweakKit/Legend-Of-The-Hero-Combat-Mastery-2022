using UnityEngine;

namespace Runtime.UI
{
    public class ScaleZ : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float scale)
        {
            rectTransform.localScale = new Vector3(rectTransform.localScale.x, rectTransform.localScale.y, scale);
        }

        #endregion Class Methods
    }
}