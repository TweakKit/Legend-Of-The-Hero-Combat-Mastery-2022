using Runtime.Definition;
using Runtime.Extensions;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior displays move effects for the character,...
    /// </summary>
    public class CharacterDisplayMoveEffectBehavior : CharacterBehavior
    {
        #region Members

        private static string s_vfxMoveHolderName = "character_move_effect_container";
        private static string s_moveDustEffectName = "move_effect";
        private static string s_buffSpeedEffectName = "buff_speed_effect";
        private ParticleSystem _moveDustEffect;
        private ParticleSystem _buffSpeedEffect;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);

            var vfxHolder = ownerTransform.FindChildTransform(s_vfxMoveHolderName);
            if (vfxHolder == null)
            {
                return false;
            }

            _moveDustEffect = vfxHolder.Find(s_moveDustEffectName).GetComponentInChildren<ParticleSystem>();
            _buffSpeedEffect = vfxHolder.Find(s_buffSpeedEffectName).GetComponentInChildren<ParticleSystem>();

            _moveDustEffect.Stop();
            _buffSpeedEffect.Stop();

            ownerModel.MovementChangedEvent += OnMovementChanged;
            ownerModel.StatChangedEvent += OnStatChanged;

            return true;
        }

        private void OnMovementChanged()
        {
            if (ownerModel.IsMoving)
                _moveDustEffect.Play();
            else
                _moveDustEffect.Stop();
        }

        private void OnStatChanged(StatType statType, float value)
        {
            if (statType == StatType.MoveSpeed)
            {
                var baseStat = ownerModel.GetBaseStatValue(statType);
                if (value > baseStat)
                    _buffSpeedEffect.Play();
                else
                    _buffSpeedEffect.Stop();
            }
        }

        #endregion Class Methods
    }
}