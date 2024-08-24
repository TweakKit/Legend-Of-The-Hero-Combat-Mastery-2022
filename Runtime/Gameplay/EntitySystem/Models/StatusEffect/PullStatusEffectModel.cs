using System;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class PullStatusEffectModel : StatusEffectModel
    {
        #region Members

        private float _pullSpeed;
        private float _pullDistance;
        private PullType _pullType;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Pull;
        public override bool IsStackable => false;
        public override bool IsOneShot => Duration == 0.0f;
        public float PullSpeed => _pullSpeed;
        public float PullDistance => _pullDistance;
        public PullType PullType => _pullType;
        public float Duration => _pullType == PullType.ToDamager ? _pullDistance / _pullSpeed : -1;

        #endregion Properties

        #region Class Methods

        public PullStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            PullModifierDataConfigItem pullStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as PullModifierDataConfigItem;
            _pullSpeed = pullStatusEffectDataConfigItem.pullSpeed;
            _pullDistance = pullStatusEffectDataConfigItem.pullDistance;
            _pullType = pullStatusEffectDataConfigItem.pullType;
        }

        public override StatusEffectModel Clone()
            => MemberwiseClone() as PullStatusEffectModel;

        #endregion Class Methods
    }
}