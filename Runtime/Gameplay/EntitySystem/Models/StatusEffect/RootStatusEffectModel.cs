using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class RootStatusEffectModel : DurationStatusEffectModel
    {
        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Root;
        public override bool IsStackable => false;
        public override bool IsOneShot => Duration == 0.0f;

        #endregion Properties

        #region Class Methods

        public RootStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            RootModifierDataConfigItem pullAndRootStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as RootModifierDataConfigItem;
            duration = pullAndRootStatusEffectDataConfigItem.rootDuration;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as RootStatusEffectModel;

        #endregion Class Methods
    }
}