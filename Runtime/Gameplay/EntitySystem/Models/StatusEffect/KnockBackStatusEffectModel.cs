using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class KnockBackStatusEffectModel : StatusEffectModel
    {
        #region Members

        private float _knockbackVelocity;
        private float _knockbackDistance;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.KnockBack;
        public override bool IsStackable => false;
        public override bool IsOneShot => true;
        public float KnockbackVelocity => _knockbackVelocity;
        public float KnockbackDistance => _knockbackDistance;

        #endregion Properties

        #region Class Methods

        public KnockBackStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            KnockBackModifierDataConfigItem knockBackStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as KnockBackModifierDataConfigItem;
            _knockbackVelocity = knockBackStatusEffectDataConfigItem.knockbackVelocity;
            _knockbackDistance = knockBackStatusEffectDataConfigItem.knockbackDistance;
        }

        public KnockBackStatusEffectModel(float knockbackVelocity, float knockbackDistance, float chance = 1.0f) : base(chance)
        {
            _knockbackVelocity = knockbackVelocity;
            _knockbackDistance = knockbackDistance;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as KnockBackStatusEffectModel;

        #endregion Class Methods
    }
}