using UnityEngine;
using UnityEngine.UI;

namespace Runtime.UI
{
    public class RecolourRed : TransitionEffectBase<Graphic>
    {
        #region Class Methods

        public override void OnTransition(Graphic graphic, float red)
        {
            graphic.color = new Color(red, graphic.color.g, graphic.color.b, graphic.color.a);
        }

        #endregion Class Methods
    }
}