using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class TerrorStatusEffectModel : DurationStatusEffectModel
    {
        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Terror;
        public override bool IsStackable => false;

        #endregion Properties

        #region Class Methods

        public TerrorStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            TerrorModifierDataConfigItem terrorStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as TerrorModifierDataConfigItem;
            duration = terrorStatusEffectDataConfigItem.duration;
        }

        public TerrorStatusEffectModel(float duration, float chance = 1.0f)
            : base(duration, chance) { }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as TerrorStatusEffectModel;

        #endregion Class Methods
    }
}