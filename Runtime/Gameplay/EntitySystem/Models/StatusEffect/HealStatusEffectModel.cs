using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class HealStatusEffectModel : StatusEffectModel
    {
        #region Members

        private float _healAmount;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Heal;
        public override bool IsStackable => false;
        public override bool IsOneShot => true;
        public float HealAmount => _healAmount;
        public DamageSource DamageSource { get; private set; }

        #endregion Properties

        #region Class Methods

        public HealStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            HealModifierDataConfigItem healStatusEffectDataConfigItem =  statusEffectData.statusEffectDataConfigItem as HealModifierDataConfigItem;
            _healAmount = healStatusEffectDataConfigItem.healAmount;
        }

        public HealStatusEffectModel(float healAmount, DamageSource damageSource = DamageSource.FromOther, float chance = 1.0f) : base(chance)
        {
            _healAmount = healAmount;
            DamageSource = damageSource;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as HealStatusEffectModel;

        #endregion Class Methods
    }
}