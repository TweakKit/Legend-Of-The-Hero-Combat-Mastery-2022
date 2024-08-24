using UnityEngine;

namespace Runtime.Extensions
{
    public static class TransformExtensions
    {
        #region Members

        private static Vector3[] m_Corners = new Vector3[4];

        #endregion Members

        #region Class Methods

        public static Transform FindChildTransform(this Transform transform, string name)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i);
                if (string.Equals(childTransform.name, name))
                    return childTransform;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform childTransform = transform.GetChild(i).FindChildTransform(name);
                if (childTransform != null)
                    return childTransform;
            }

            return null;
        }

        public static GameObject FindChildGameObject(this Transform transform, string name)
        {
            Transform childTransform = transform.FindChildTransform(name);
            if (childTransform != null)
                return childTransform.gameObject;
            else
                return null;
        }

        /// <summary>
        /// Calculate rect transform bounds in local space.
        /// </summary>
        public static Bounds CalculateRelativeLocalBounds(this Transform transform)
        {
            var component = transform as RectTransform;
            if (component == null)
                return new Bounds(Vector3.zero, Vector3.zero);

            var v1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var v2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            component.GetLocalCorners(m_Corners);
            for (int i = 0; i < m_Corners.Length; ++i)
            {
                v1 = Vector3.Min(m_Corners[i], v1);
                v2 = Vector3.Max(m_Corners[i], v2);
            }
            var bounds = new Bounds(v1, Vector3.zero);
            bounds.Encapsulate(v2);
            return bounds;
        }

        #endregion Class Methods
    }
}