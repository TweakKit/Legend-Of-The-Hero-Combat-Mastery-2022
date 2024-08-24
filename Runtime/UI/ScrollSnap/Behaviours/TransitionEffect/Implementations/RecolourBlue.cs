using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class RecolourBlue : TransitionEffectBase<Graphic>
    {
        #region Class Methods

        public override void OnTransition(Graphic graphic, float blue)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, blue, graphic.color.a);
        }

        #endregion Class Methods
    }
}