using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class ChillStatusEffectModel : DurationStatusEffectModel
    {
        #region Members

        private float _decreasedMoveSpeedPercent;

        #endregion Members

        #region Properties

        public override StatusEffectType StatusEffectType => StatusEffectType.Chill;
        public override bool IsStackable => true;
        public float DecreasedMoveSpeedPercent => _decreasedMoveSpeedPercent;

        #endregion Properties

        #region Class Methods

        public ChillStatusEffectModel(StatusEffectData statusEffectData) : base(statusEffectData)
        {
            ChillModifierDataConfigItem chillStatusEffectDataConfigItem = statusEffectData.statusEffectDataConfigItem as ChillModifierDataConfigItem;
            duration = chillStatusEffectDataConfigItem.duration;
            _decreasedMoveSpeedPercent = chillStatusEffectDataConfigItem.decreasedMoveSpeedPercent;
        }

        public ChillStatusEffectModel(float decreasedMoveSpeedPercent, float duration, float chance = 1.0f) : base(duration, chance)
            => _decreasedMoveSpeedPercent = decreasedMoveSpeedPercent;

        public override StatusEffectModel Clone()
            => MemberwiseClone() as ChillStatusEffectModel;

        public override void Stack(StatusEffectModel stackedStatusEffectModel, bool isMaxStack)
        {
            var stackedChillStatusEffectModel = stackedStatusEffectModel as ChillStatusEffectModel;
            duration = Mathf.Max(duration, stackedChillStatusEffectModel.duration);

            if (!isMaxStack)
            {
                bonusDuration += stackedChillStatusEffectModel.bonusDuration;
                _decreasedMoveSpeedPercent = Mathf.Min(_decreasedMoveSpeedPercent + stackedChillStatusEffectModel._decreasedMoveSpeedPercent, 1);
            }
        }

        #endregion Class Methods
    }
}