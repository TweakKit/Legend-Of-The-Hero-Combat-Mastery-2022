using System.Collections.Generic;
using UnityEngine;

namespace Runtime.UI
{
    [RequireComponent(typeof(CustomScrollSnap))]
    public abstract class TransitionEffectBase<T> : MonoBehaviour where T : Component
    {
        #region Members

        [SerializeField] 
        protected MinMax minMaxDisplacement = new MinMax(-1000, 1000);
        [SerializeField] 
        protected MinMax minMaxValue = new MinMax(0, 1);
        [SerializeField] 
        protected AnimationCurve function = AnimationCurve.Linear(0, 0, 1, 1);
        private Dictionary<GameObject, T> cachedComponents = new Dictionary<GameObject, T>();

        #endregion Members

        #region Class Methods

        public void OnTransition(GameObject panel, float displacement)
        {
            if (!cachedComponents.ContainsKey(panel))
            {
                cachedComponents.Add(panel, panel.GetComponent<T>());
            }

            float t = Mathf.InverseLerp(minMaxDisplacement.min, minMaxDisplacement.max, displacement);
            float f = function.Evaluate(t);
            float v = Mathf.Lerp(minMaxValue.min, minMaxValue.max, f);
            OnTransition(cachedComponents[panel], v);
        }

        public abstract void OnTransition(T component, float value);

        #endregion Class Methods
    }
}