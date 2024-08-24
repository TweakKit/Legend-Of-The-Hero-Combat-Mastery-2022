using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class HardenedStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _increasedArmor;
        private float _decreasedMoveSpeed;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Hardened;
        public override bool IsStackable => false;
        public float IncreasedArmor => _increasedArmor;
        public float DecreasedMoveSpeed => _decreasedMoveSpeed;

        #endregion Properties

        #region Class Methods

        public HardenedStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            HardenedModifierDataConfigItem hardenedStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as HardenedModifierDataConfigItem;
            duration = hardenedStatusEffectDataConfigItem.duration;
            _increasedArmor = hardenedStatusEffectDataConfigItem.armorIncrease;
            _decreasedMoveSpeed = hardenedStatusEffectDataConfigItem.speedDecrease;
        }

        public HardenedStatusEffectModel(float increasedArmor, float decreasedMoveSpeed, float duration, float chance = 1.0f) : base(duration, chance)
        {
            _increasedArmor = increasedArmor;
            _decreasedMoveSpeed = decreasedMoveSpeed;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as HardenedStatusEffectModel;

        #endregion Class Methods
    }
}