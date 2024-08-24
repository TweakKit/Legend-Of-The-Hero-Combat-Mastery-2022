using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class ToughStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _increasedAttackDamage;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Tough;
        public override bool IsStackable => false;
        public float IncreasedAttackDamage => _increasedAttackDamage;

        #endregion Properties

        #region Class Methods

        public ToughStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            ToughModifierDataConfigItem toughStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as ToughModifierDataConfigItem;
            duration = toughStatusEffectDataConfigItem.duration;
            _increasedAttackDamage = toughStatusEffectDataConfigItem.attackIncrease;
        }

        public ToughStatusEffectModel(float increasedAttackDamage, float duration, float chance = 1.0f) : base(duration, chance)
            => _increasedAttackDamage = increasedAttackDamage;

        public override StatusEffectModel Clone()
            => MemberwiseClone() as ToughStatusEffectModel;

        #endregion Class Methods
    }
}