using UnityEngine;

namespace Runtime.UI
{
    public class TranslateZ : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float distance)
        {
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, distance);
        }

        #endregion Class Methods
    }
}