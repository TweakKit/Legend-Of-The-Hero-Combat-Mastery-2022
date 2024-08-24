using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract partial class CharacterModel : EntityModel
    {
        #region Members

        private List<IUpdateHealthModifier> _updateHealthModifiers;

        #endregion Members

        #region Class Methods

        public void AddUpdateHealthModifier(IUpdateHealthModifier updateHealthModifier)
        {
            if (_updateHealthModifiers == null)
                _updateHealthModifiers = new();
            _updateHealthModifiers.Add(updateHealthModifier);
            _updateHealthModifiers = _updateHealthModifiers.OrderBy(x => x.UpdateHealthPriority).ToList();
        }

        public void BuffShield(float value, DamageProperty damageProperty)
        {
            var buffValue = value;
            if (currentDefense + value >= MaxDefense)
                buffValue = MaxDefense - currentDefense;

            currentDefense += buffValue;
            ShieldChangedEvent.Invoke(buffValue, damageProperty);
        }

        public virtual void BuffHp(float value, DamageSource damageSource = DamageSource.FromOther, DamageProperty damageProperty = DamageProperty.None)
        {
            if (_updateHealthModifiers != null)
            {
                foreach (var item in _updateHealthModifiers)
                    (value, damageProperty) = item.ModifyBuffHp(value, damageSource, damageProperty);
            }

            var buffValue = value;
            if (currentHp + value >= MaxHp)
                buffValue = MaxHp - currentHp;

            currentHp += buffValue;
            HealthChangedEvent.Invoke(buffValue, damageProperty, damageSource);
        }

        public virtual void DebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if (_updateHealthModifiers != null)
            {
                foreach (var item in _updateHealthModifiers)
                    value = item.ModifyDebuffHp(value, damageSource, damageProperty, damageCreatorModel);
            }

            if (currentDefense > 0)
            {
                if (value >= currentDefense)
                {
                    currentDefense = 0;
                    ShieldChangedEvent.Invoke(currentDefense, damageProperty);
                    value -= currentDefense;
                }
                else
                {
                    currentDefense -= value;
                    ShieldChangedEvent.Invoke(-value, damageProperty);
                    value = 0;
                    return;
                }
            }

            var buffValue = value;
            if (currentHp + value <= 0)
                buffValue = currentHp;

#if DEBUGGING
            var log = $"get_damage_log_after_mechanics || target: {EntityId}/{EntityType} | damageTaken: {value} | damageSource: {damageSource} | damageProperty: {damageProperty}";
            Debug.Log($"{log} | finalDamage: {buffValue}");
#endif

            currentHp -= buffValue;
            HealthChangedEvent.Invoke(-buffValue, damageProperty, damageSource);
            if (IsDead)
                DeathEvent.Invoke(damageSource);
        }

        #endregion Class Methods
    }
}