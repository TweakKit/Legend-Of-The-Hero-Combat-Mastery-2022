using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class DreadStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _increasedLifesteal;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Dread;
        public override bool IsStackable => false;
        public float IncreasedLifesteal => _increasedLifesteal;

        #endregion Properties

        #region Class Methods

        public DreadStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            DreadModifierDataConfigItem dreadStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as DreadModifierDataConfigItem;
            duration = dreadStatusEffectDataConfigItem.duration;
            _increasedLifesteal = dreadStatusEffectDataConfigItem.lifestealIncrease;
        }

        public DreadStatusEffectModel(float increasedLifesteal, float duration, float chance = 1.0f) : base(duration, chance)
            => _increasedLifesteal = increasedLifesteal;

        public override StatusEffectModel Clone()
            => MemberwiseClone() as DreadStatusEffectModel;

        #endregion Class Methods
    }
}