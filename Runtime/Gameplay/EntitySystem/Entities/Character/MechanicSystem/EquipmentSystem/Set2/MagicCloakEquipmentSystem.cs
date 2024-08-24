using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class MagicCloakEquipmentSystem : EquipmentSystem<MagicCloakEquipmentSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private bool _isTriggered;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddUpdateHealthModifier(this);
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _isTriggered = false;
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddUpdateHealthModifier(this);
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _isTriggered = false;
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (ownerModel.CanApplyRateBonusConvert)
            {
                if(creatorModel.CurrentHp / creatorModel.MaxHp < ownerModel.TriggerHealthPercent)
                {
                    if (!_isTriggered)
                        _isTriggered = true;
                }
                else
                {
                    if (_isTriggered)
                        _isTriggered = false;
                }
            }
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            var randomValue = Random.Range(0, 1f);

            if (_isTriggered)
            {
                if (randomValue < ownerModel.RateConvertDamageReceivedToHealth + ownerModel.RateBonusConvert)
                {
                    creatorModel.BuffHp(value * ownerModel.DamageReceivedToHealthPercent);
#if DEBUGGING
                    Debug.Log($"weapon_magic_cloak || damage_received: {value} || percent_damage_to_heal: {ownerModel.DamageReceivedToHealthPercent} || heal: {value * ownerModel.DamageReceivedToHealthPercent} ");
#endif
                }
            }
            else
            {
                if (randomValue < ownerModel.RateConvertDamageReceivedToHealth)
                {
                    creatorModel.BuffHp(value * ownerModel.DamageReceivedToHealthPercent);
#if DEBUGGING
                    Debug.Log($"weapon_magic_cloak || damage_received: {value} || percent_damage_to_heal: {ownerModel.DamageReceivedToHealthPercent} || heal: {value * ownerModel.DamageReceivedToHealthPercent} ");
#endif
                }
            }

            return value;
        }

        #endregion Class Methods
    }
}