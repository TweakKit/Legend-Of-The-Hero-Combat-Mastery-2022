using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class TauntStatusEffectModel : DurationStatusEffectModel
    {
        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Taunt;
        public override bool IsStackable => false;

        #endregion Properties

        #region Class Methods

        public TauntStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            TauntModifierDataConfigItem tauntStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as TauntModifierDataConfigItem;
            duration = tauntStatusEffectDataConfigItem.duration;
        }

        public TauntStatusEffectModel(float duration, float chance = 1.0f)
            : base(duration, chance) { }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as TauntStatusEffectModel;

        #endregion Class Methods
    }
}