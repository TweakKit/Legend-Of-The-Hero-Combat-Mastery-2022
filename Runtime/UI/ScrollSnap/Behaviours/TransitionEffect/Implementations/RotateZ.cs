using UnityEngine;

namespace Runtime.UI
{
    public class RotateZ : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float angle)
        {
            rectTransform.localRotation = Quaternion.Euler(new Vector3(rectTransform.localEulerAngles.x, rectTransform.localEulerAngles.y, angle));
        }

        #endregion Class Methods
    }
}