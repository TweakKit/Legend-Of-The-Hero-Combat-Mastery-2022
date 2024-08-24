using UnityEngine;
using Runtime.Extensions;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior displays character's information, such as level, health,...
    /// </summary>
    public sealed class CharacterDisplayHUDBehavior : CharacterBehavior
    {
        #region Members

        private const string HUD_NAME = "character_hud";
        private CharacterHUD _characterHUD;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public override void Validate(Transform ownerTransform)
        {
            var characterHUDGameObject = ownerTransform.FindChildTransform(HUD_NAME);
            if (characterHUDGameObject == null)
            {
                Debug.LogError("Character HUD name is not mapped!");
                return;
            }

            _characterHUD = characterHUDGameObject.GetComponent<CharacterHUD>();
            if (_characterHUD == null)
            {
                Debug.LogError("Require a Character HUD component!");
                return;
            }
        }
#endif

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            _characterHUD = transform.FindChildGameObject(HUD_NAME).GetComponent<CharacterHUD>();
            ownerModel.HealthChangedEvent += OnHealthChanged;
            ownerModel.ShieldChangedEvent += OnShieldChanged;
            ownerModel.StatChangedEvent += OnStatChanged;
            ownerModel.DeathEvent += OnDeath;
            InitHUD();
            return true;
        }

        private void InitHUD()
            => _characterHUD.Init(ownerModel.CurrentDefense, ownerModel.MaxDefense);

        private void OnStatChanged(StatType statType, float value)
        {
            if(statType == StatType.Health)
                _characterHUD.UpdateHealthBar(ownerModel.CurrentHp, ownerModel.MaxHp);
            else if (statType == StatType.Shield)
                _characterHUD.UpdateShieldBar(ownerModel.CurrentDefense, ownerModel.MaxDefense);
        }

        private void OnShieldChanged(float deltaDefense, DamageProperty damageProperty)
            => _characterHUD.UpdateShieldBar(ownerModel.CurrentDefense, ownerModel.MaxDefense);

        private void OnHealthChanged(float deltaHp, DamageProperty damageProperty, DamageSource damageSource)
            => _characterHUD.UpdateHealthBar(ownerModel.CurrentHp, ownerModel.MaxHp);

        private void OnDeath(DamageSource damageSource)
            => _characterHUD.SetVisibility(false);

        #endregion Class Methdos
    }
}