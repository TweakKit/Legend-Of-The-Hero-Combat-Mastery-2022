using System;
using UnityEngine;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class KnockBackStatusEffect : PerFrameDurationStatusEffect<KnockBackStatusEffectModel>
    {
        #region Members

        private float _knockbackDuration;
        private float _currentKnockbackDuration;
        private float _knockbackDistance;
        private Vector2 _knockBackDirection;
        private Vector2 _originPosition;
        private bool _hasToStopKnockBack;

        #endregion Members

        #region Properties

        protected override float Duration => _knockbackDuration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            _knockbackDuration = ownerModel.KnockbackDistance / ownerModel.KnockbackVelocity;
            _knockbackDistance = ownerModel.KnockbackDistance;
            _currentKnockbackDuration = 0.0f;
            _originPosition = receiverModel.Position;
            _knockBackDirection = statusEffectMetaData.statusEffectDirection.normalized;
            if (_knockBackDirection == Vector2.zero)
                _knockBackDirection = -receiverModel.FaceDirection;

            currentAffectDuration = Duration;
            HasFinished = false;
        }

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingKnockback();
        }

        protected override void AffectPerFrame(CharacterModel receiverModel)
        {
            base.AffectPerFrame(receiverModel);
            _currentKnockbackDuration += Time.deltaTime;
            float interpolationValue = Mathf.Lerp(0, _knockbackDistance, Mathf.Clamp01(_currentKnockbackDuration / _knockbackDuration));
            Vector2 knockbackPosition = _originPosition + _knockBackDirection * interpolationValue;
            if (CanBeKnockedBackAtThisPosition(knockbackPosition))
                receiverModel.GettingKnockback(knockbackPosition);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingKnockback();
        }

        protected override bool CheckStopAffect(CharacterModel receiverModel)
            => base.CheckStopAffect(receiverModel) || _hasToStopKnockBack;

        private bool CanBeKnockedBackAtThisPosition(Vector2 position)
        {
            bool isWalkable = MapManager.Instance.IsWalkable(position);
            _hasToStopKnockBack = !isWalkable;
            return isWalkable;
        }

        #endregion Class methods
    }
}