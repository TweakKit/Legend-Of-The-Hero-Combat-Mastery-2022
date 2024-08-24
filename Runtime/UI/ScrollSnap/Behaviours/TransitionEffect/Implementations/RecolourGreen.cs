using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class RecolourGreen : TransitionEffectBase<Graphic>
    {
        #region Class Methods

        public override void OnTransition(Graphic graphic, float green)
        {
            graphic.color = new Color(graphic.color.r, green, graphic.color.b, graphic.color.a);
        }

        #endregion Class Methods
    }
}