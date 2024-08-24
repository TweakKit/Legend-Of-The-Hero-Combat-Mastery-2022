using UnityEngine;

namespace Runtime.Extensions
{
    public static class CanvasGroupExtensions
    {
        #region Class Methods

        public static void SetActive(this CanvasGroup canvas, bool isActive)
        {
            canvas.alpha = isActive ? 1 : 0;
            canvas.interactable = isActive;
            canvas.blocksRaycasts = isActive;
        }

        public static void SetActiveWithoutAlpha(this CanvasGroup canvas, bool isActive)
        {
            canvas.interactable = isActive;
            canvas.blocksRaycasts = isActive;
        }

        #endregion Class Methods
    }
}