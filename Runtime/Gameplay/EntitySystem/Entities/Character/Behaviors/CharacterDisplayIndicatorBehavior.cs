using UnityEngine;
using Runtime.Extensions;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior displays indicators for the character, such as the selected indicator,...
    /// </summary>
    public sealed class CharacterDisplayIndicatorBehavior : CharacterBehavior
    {
        #region Members

        private const string SELECTED_INDICATOR = "selected_indicator";
        private GameObject _selectedIndicator;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            _selectedIndicator = ownerTransform.FindChildGameObject(SELECTED_INDICATOR);
            if (_selectedIndicator == null)
            {
                Debug.LogError("Selected indicator name is not mapped!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            _selectedIndicator = transform.FindChildGameObject(SELECTED_INDICATOR);
            _selectedIndicator.SetActive(false);
            ownerModel.TargetedEvent += OnTargeted;
            ownerModel.DeathEvent += OnDeath;
            return true;
        }

        private void OnTargeted(bool isTargeted) => _selectedIndicator.SetActive(isTargeted);
        private void OnDeath(DamageSource damageSource) => _selectedIndicator.SetActive(false);

        #endregion Class Methods
    }
}