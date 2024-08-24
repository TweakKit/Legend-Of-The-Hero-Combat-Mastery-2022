using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class QuickStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _increasedAttackSpeed;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Quick;
        public override bool IsStackable => false;
        public float IncreasedAttackSpeed => _increasedAttackSpeed;

        #endregion Properties

        #region Class Methods

        public QuickStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            QuickModifierDataConfigItem quickStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as QuickModifierDataConfigItem;
            duration = quickStatusEffectDataConfigItem.duration;
            _increasedAttackSpeed = quickStatusEffectDataConfigItem.attackSpeedIncrease;
        }

        public QuickStatusEffectModel(float increasedAttackSpeed, float duration, float chance = 1.0f) : base(duration, chance)
            => _increasedAttackSpeed = increasedAttackSpeed;

        public override StatusEffectModel Clone()
            => MemberwiseClone() as QuickStatusEffectModel;

        #endregion Class Methods
    }
}