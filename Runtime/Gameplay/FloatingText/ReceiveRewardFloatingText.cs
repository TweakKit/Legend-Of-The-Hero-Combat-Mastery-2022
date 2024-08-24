using TMPro;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class ReceiveRewardFloatingText : FloatingText
    {
        #region Members

        private static readonly float s_heightOffset = 1.5f;

        [SerializeField]
        private SpriteRenderer _image;
        [SerializeField]
        private TextMeshPro _number;

        #endregion Members

        #region Class Methods

        public void Init(Vector2 spawnPosition,  Sprite sprite, long number)
        {
            transform.position = spawnPosition + new Vector2(0, s_heightOffset);
            _number.text = $"+{number}";
            _image.sprite = sprite;
        }

        #endregion Class Methods
    }
}
