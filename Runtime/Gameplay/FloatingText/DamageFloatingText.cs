using TMPro;
using UnityEngine;

namespace Runtime.Gameplay
{
    public class DamageFloatingText : FloatingText
    {
        #region Members

        [SerializeField]
        private TextMeshPro _damageText;

        #endregion Members

        #region Properties

        public int Index { get; set; }

        #endregion Properties

        #region Class Methods

        public void Init(float value, Vector2 spawnPosition)
        {
            transform.localPosition = spawnPosition;
            _damageText.text = value > 0 ? $"+{Mathf.Floor(value)}" : Mathf.Floor(value).ToString();
        }

        protected override void OnComplete()
        {
            DamageFloatingTextController.Instance.Despawn(this);
        }

        #endregion Class Methods
    }
}