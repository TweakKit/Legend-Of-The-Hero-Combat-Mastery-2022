using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class HasteStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _increasedMoveSpeed;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Haste;
        public override bool IsStackable => false;
        public float IncreasedMoveSpeed => _increasedMoveSpeed;

        #endregion Properties

        #region Class Methods

        public HasteStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            HasteModifierDataConfigItem hasteStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as HasteModifierDataConfigItem;
            duration = hasteStatusEffectDataConfigItem.duration;
            _increasedMoveSpeed = hasteStatusEffectDataConfigItem.speedIncrease;
        }

        public HasteStatusEffectModel(float increasedMoveSpeed, float duration, float chance = 1.0f) : base(duration, chance)
            => _increasedMoveSpeed = increasedMoveSpeed;

        public override StatusEffectModel Clone()
            => MemberwiseClone() as HasteStatusEffectModel;

        #endregion Class Methods
    }
}