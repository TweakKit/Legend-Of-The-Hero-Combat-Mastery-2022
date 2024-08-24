using UnityEngine;

namespace Runtime.UI
{
    public class RotateY : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float angle)
        {
            rectTransform.localRotation = Quaternion.Euler(new Vector3(rectTransform.localEulerAngles.x, angle, rectTransform.localEulerAngles.z));
        }

        #endregion Class Methods
    }
}