using UnityEngine;

namespace Runtime.UI
{
    public class RotateX : TransitionEffectBase<RectTransform>
    {
        #region Class Methods

        public override void OnTransition(RectTransform rectTransform, float angle)
        {
            rectTransform.localRotation = Quaternion.Euler(new Vector3(angle, rectTransform.localEulerAngles.y, rectTransform.localEulerAngles.z));
        }

        #endregion Class Methods
    }
}