using UnityEngine;

namespace Runtime.UI
{
    public class Fade : TransitionEffectBase<CanvasGroup>
    {
        #region Class Methods

        public override void OnTransition(CanvasGroup canvasGroup, float alpha)
        {
            canvasGroup.alpha = alpha;
        }

        #endregion Class Methods
    }
}