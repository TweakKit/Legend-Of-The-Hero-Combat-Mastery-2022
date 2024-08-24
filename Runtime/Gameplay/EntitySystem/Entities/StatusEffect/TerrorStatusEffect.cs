using System;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class TerrorStatusEffect : PerFrameDurationStatusEffect<TerrorStatusEffectModel>
    {
        #region Members

        private Vector2 _terrorDirection;

        #endregion Members

        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            _terrorDirection = statusEffectMetaData.statusEffectDirection.normalized;
        }

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingTerror();
        }

        protected override void AffectPerFrame(CharacterModel receiverModel)
        {
            base.AffectPerFrame(receiverModel);

            if (receiverModel.IsInHardCCPriority2Status || receiverModel.IsInHardCCPriority3Status)
                return;

            var speed = receiverModel.GetTotalStatValue(StatType.MoveSpeed);
            var nextPosition = receiverModel.Position + _terrorDirection * speed * Time.deltaTime;
            if (MapManager.Instance.IsWalkable(nextPosition))
                receiverModel.SetMoveDirection(_terrorDirection, false);
            else
                receiverModel.SetMoveDirection(Vector2.zero, false);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingTerror();
        }

        #endregion Class methods
    }
}