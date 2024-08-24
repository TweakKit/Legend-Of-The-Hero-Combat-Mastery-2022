using System;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class PullStatusEffect : PerFrameDurationStatusEffect<PullStatusEffectModel>
    {
        #region Members

        private float _pullDuration;
        private float _currentPullDuration;
        private Vector2 _pullDirection;
        private Vector2 _originPosition;
        private bool _hasToStopPull;
        private float _pullDistance;

        #endregion Members

        #region Properties

        protected override float Duration => _pullDuration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);

            if (ownerModel.PullType == PullType.ToDamager)
            {
                _pullDirection = -statusEffectMetaData.statusEffectDirection;
                _pullDuration = ownerModel.PullDistance / ownerModel.PullSpeed;
                _pullDistance = ownerModel.PullDistance;
            }
            else
            {
                _pullDirection = (statusEffectMetaData.statusEffectAttractedPoint - receiverModel.Position).normalized;
                _pullDistance = (statusEffectMetaData.statusEffectAttractedPoint - receiverModel.Position).magnitude;
                _pullDuration = _pullDistance / ownerModel.PullSpeed;
            }
            _currentPullDuration = 0.0f;
            _originPosition = receiverModel.Position;

            currentAffectDuration = Duration;
            HasFinished = false;
        }

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingPull();
        }

        protected override void AffectPerFrame(CharacterModel receiverModel)
        {
            base.AffectPerFrame(receiverModel);
            _currentPullDuration += Time.deltaTime;
            float interpolationValue = Mathf.Lerp(0, _pullDistance, Mathf.Clamp01(_currentPullDuration / _pullDuration));
            Vector2 pullPosition = _originPosition + _pullDirection * interpolationValue;
            if (CanBePulledAtThisPosition(pullPosition))
                receiverModel.GettingPull(pullPosition);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingPull();
        }

        protected override bool CheckStopAffect(CharacterModel receiverModel)
            => base.CheckStopAffect(receiverModel) || _hasToStopPull;

        private bool CanBePulledAtThisPosition(Vector2 position)
        {
            bool isWalkable = MapManager.Instance.IsWalkable(position);
            _hasToStopPull = !isWalkable;
            return isWalkable;
        }

        #endregion Class methods
    }
}
