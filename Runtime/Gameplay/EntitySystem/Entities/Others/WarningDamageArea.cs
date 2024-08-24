using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class WarningDamageArea : MonoBehaviour
    {
        #region Class Methods

        public void Init(Vector2 spawnPosition, Vector2 scale)
        {
            transform.position = spawnPosition;
            transform.localScale = scale;
        }

        #endregion Class Methods
    }
}