using UnityEngine;

namespace Runtime.Extensions
{
    public static class GameObjectExtensions
    {
        #region Class Methods

        public static void SetActiveWithCheck(this GameObject gameObject, bool isActive)
        {
            if (isActive && !gameObject.activeSelf)
                gameObject.SetActive(isActive);
            else if (!isActive && gameObject.activeSelf)
                gameObject.SetActive(isActive);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
                return gameObject.AddComponent<T>();
            return component;
        }

        #endregion Class Methods
    }
}