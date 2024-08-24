using System;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    [Serializable]
    public class TauntStatusEffect : PerFrameDurationStatusEffect<TauntStatusEffectModel>
    {
        #region Members

        private static readonly float s_moveDistanceThreshold = 1.25f;
        private EntityModel _chasedTargetModel;

        #endregion Members

        #region Properties

        protected override float Duration => ownerModel.Duration;

        #endregion Properties

        #region Class methods

        public override void Init(StatusEffectModel statusEffectModel, EntityModel senderModel, CharacterModel receiverModel, StatusEffectMetaData statusEffectMetaData)
        {
            base.Init(statusEffectModel, senderModel, receiverModel, statusEffectMetaData);
            _chasedTargetModel = senderModel;
        }

        protected override void StartAffect(CharacterModel receiverModel)
        {
            base.StartAffect(receiverModel);
            receiverModel.StartGettingTaunt();
        }

        protected override void AffectPerFrame(CharacterModel receiverModel)
        {
            base.AffectPerFrame(receiverModel);

            if (receiverModel.IsInHardCCPriority2Status || receiverModel.IsInHardCCPriority3Status || receiverModel.isPlayingSkill)
                return;

            receiverModel.GettingTaunt();

            if (!receiverModel.IsMoving && Vector2.SqrMagnitude(_chasedTargetModel.Position - receiverModel.Position) <= s_moveDistanceThreshold * s_moveDistanceThreshold)
                return;

            var speed = receiverModel.GetTotalStatValue(StatType.MoveSpeed);
            var direction = (_chasedTargetModel.Position - receiverModel.Position).normalized;
            var nextPosition =  receiverModel.Position + direction * speed * Time.deltaTime;

            if (MapManager.Instance.IsWalkable(nextPosition) && Vector2.SqrMagnitude(_chasedTargetModel.Position - receiverModel.Position) > Vector2.SqrMagnitude(nextPosition - receiverModel.Position))
                receiverModel.SetMoveDirection(direction);
            else
                receiverModel.SetMoveDirection(Vector2.zero);
        }

        protected override void FinishAffect(CharacterModel receiverModel)
        {
            base.FinishAffect(receiverModel);
            receiverModel.StopGettingTaunt();
        }

        #endregion Class methods
    }
}