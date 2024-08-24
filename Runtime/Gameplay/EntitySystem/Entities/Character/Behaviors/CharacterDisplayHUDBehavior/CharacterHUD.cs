using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class CharacterHUD : MonoBehaviour
    {
        #region Members

        [SerializeField]
        private Transform _healthBarSliderAnchor;
        [SerializeField]
        private Transform _shieldBarSliderAnchor;
        [SerializeField]
        private GameObject _shieldContainer;

        private Vector2 _currentHealthAnchorLocalScale;
        private Vector2 _currentShieldAnchorLocalScale;

        #endregion Members

        #region Class Methods

        public void Init(float currentDefense, float maxDefense)
        {
            _currentShieldAnchorLocalScale = _shieldBarSliderAnchor.transform.localScale;
            _currentShieldAnchorLocalScale.x = 1;
            _shieldBarSliderAnchor.transform.localScale = _currentShieldAnchorLocalScale;

            _currentHealthAnchorLocalScale = _healthBarSliderAnchor.transform.localScale;
            _currentHealthAnchorLocalScale.x = 1;
            _healthBarSliderAnchor.transform.localScale = _currentHealthAnchorLocalScale;

            UpdateShieldBar(currentDefense, maxDefense);
            SetVisibility(true);
        }

        public void UpdateShieldBar(float currentDefense, float maxDefense)
        {
            if (maxDefense <= 0 || currentDefense <= 0)
            {
                _shieldContainer.SetActive(false);
                return;
            }

            _shieldContainer.SetActive(true);
            _currentShieldAnchorLocalScale.x = Mathf.Max(currentDefense / maxDefense, 0);
            _shieldBarSliderAnchor.transform.localScale = _currentShieldAnchorLocalScale;
        }

        public void UpdateHealthBar(float currentHP, float maxHP)
        {
            _currentHealthAnchorLocalScale.x = Mathf.Max(currentHP / maxHP, 0);
            _healthBarSliderAnchor.transform.localScale = _currentHealthAnchorLocalScale;
        }

        public void SetVisibility(bool isVisible)
            => gameObject.SetActive(isVisible);

        #endregion Class Methods
    }
}