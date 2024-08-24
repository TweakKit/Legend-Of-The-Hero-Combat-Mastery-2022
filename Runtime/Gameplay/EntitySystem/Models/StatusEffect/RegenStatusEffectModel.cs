using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class RegenStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _healInterval;
        private float _healAmountPerInterval;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Regen;
        public override bool IsStackable => false;
        public float HealInterval => _healInterval;
        public float HealAmountPerInterval => _healAmountPerInterval;

        #endregion Properties

        #region Class Methods

        public RegenStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            RegenModifierDataConfigItem regenStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as RegenModifierDataConfigItem;
            duration = regenStatusEffectDataConfigItem.duration;
            _healInterval = regenStatusEffectDataConfigItem.interval;
            _healAmountPerInterval = regenStatusEffectDataConfigItem.healAmountPerInterval;
        }

        public RegenStatusEffectModel(float healInterval, float healAmountPerInterval, float duration, float chance = 1.0f) : base(duration, chance)
        {
            _healInterval = healInterval;
            _healAmountPerInterval = healAmountPerInterval;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as RegenStatusEffectModel;

        #endregion Class Methods
    }
}