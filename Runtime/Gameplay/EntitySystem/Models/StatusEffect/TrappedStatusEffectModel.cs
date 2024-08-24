using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class TrappedStatusEffectModel : StatusEffectModel
    {
        #region Members

        private float _damage;
        private float _damageInterval;
        private TrapType _trappedSourceType;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType
        {
            get
            {
                switch (_trappedSourceType)
                {
                    case TrapType.Poison:
                        return StatusEffectType.TrappedPoison;

                    case TrapType.Fire:
                    default:
                        return StatusEffectType.TrappedFire;
                }
            }
        }

        public override bool IsStackable => false;
        public override bool IsOneShot => Duration == 0.0f;
        public float Duration => float.MaxValue;
        public float Damage => _damage;
        public float DamageInterval => _damageInterval;
        public TrapType TrappedSourceType => _trappedSourceType;

        #endregion Properties

        #region Class Methods

        public TrappedStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            TrappedModifierDataConfigItem trappedStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as TrappedModifierDataConfigItem;
            _damage = trappedStatusEffectDataConfigItem.damage;
            _damageInterval = trappedStatusEffectDataConfigItem.damageInterval;
            _trappedSourceType = trappedStatusEffectDataConfigItem.trappedSourceType;
        }

        public TrappedStatusEffectModel(float damage, float damageInterval, TrapType trappedSourceType, float chance = 1.0f) : base(chance)
        {
            _damage = damage;
            _damageInterval = damageInterval;
            _trappedSourceType = trappedSourceType;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as TrappedStatusEffectModel;

        #endregion Class Methods
    }
}