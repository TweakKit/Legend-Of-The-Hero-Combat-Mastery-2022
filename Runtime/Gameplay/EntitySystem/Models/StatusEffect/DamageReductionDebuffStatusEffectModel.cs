using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class DamageReductionDebuffStatusEffectModel : DurationStatusEffectModel
    {
        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.DamageReductionDebuff;

        public override bool IsStackable => false;
        public float DebuffPercent { get; private set; }

        #endregion Properties

        #region Class Methods
        public DamageReductionDebuffStatusEffectModel(float debuffPercent, float duration, float chance = 1) : base(duration, chance)
        {
            DebuffPercent = debuffPercent;
        }

        public override StatusEffectModel Clone() => MemberwiseClone() as DamageReductionDebuffStatusEffectModel;

        #endregion Class Methods
    }
}