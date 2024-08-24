using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class BleedStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _damage;
        private float _interval;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Bleed;
        public override bool IsStackable => true;
        public float Damage => _damage;
        public float Interval => _interval;

        #endregion Properties

        #region Class Methods

        public BleedStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            BleedModifierDataConfigItem bleeStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as BleedModifierDataConfigItem;
            duration = bleeStatusEffectDataConfigItem.bleedDuration;
            _damage = bleeStatusEffectDataConfigItem.damageTaken;
            _interval = bleeStatusEffectDataConfigItem.interval;
        }

        public BleedStatusEffectModel(float damage, float interval, float duration, float chance = 1.0f) : base(duration, chance)
        {
            _damage = damage;
            _interval = interval;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as BleedStatusEffectModel;

        public override void Stack(StatusEffectModel stackedStatusEffectModel, bool isMaxStack)
        {
            var stackedBleedStatusEffectModel = stackedStatusEffectModel as BleedStatusEffectModel;
            duration = Mathf.Max(duration, stackedBleedStatusEffectModel.duration);
            if (!isMaxStack)
            {
                bonusDuration += stackedBleedStatusEffectModel.bonusDuration;
                _damage += stackedBleedStatusEffectModel._damage;
                _interval = Mathf.Min(_interval, stackedBleedStatusEffectModel._interval);
            }
        }

        #endregion Class Methods
    }
}