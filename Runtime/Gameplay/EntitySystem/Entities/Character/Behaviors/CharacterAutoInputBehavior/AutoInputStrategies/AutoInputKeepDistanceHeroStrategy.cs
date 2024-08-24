using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using Core.Foundation.PubSub;
using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This auto input strategy updates the movement input data that would
    /// make the character move towards the hero and keep a distance from that hero.<br/>
    /// It also checks and updates the skill trigger input data once the hero gets in their skill cast range.
    /// </summary>
    public sealed class AutoInputKeepDistanceHeroStrategy : AutoInputStrategy
    {
        #region Members

        private enum MoveState
        {
            MoveTowardsHero,
            MoveAwayFromHero,
            MoveRandomly,
        }

        private static readonly float s_stayBeforeAwayTargetBonusRange = 1.0f;
        private static readonly float s_stayBeforeBackToTargetBonusRange = 1.0f;
        private static readonly int s_awayMoveSearchMinSlotsCount = 3;
        private static readonly int s_awayMoveSearchMaxSlotsCount = 6;
        private static readonly float s_awayMoveAimStrength = 0.2f;
        private float _stayBeforeAwayTargetDistanceSqr;
        private float _stayBeforeBackToTargetDistanceSqr;
        private bool _isMoveAwayFromTarget;
        private int _awayMoveSearchLength;
        private int _awayMoveSearchSpreadLength;
        private MoveState _moveState = MoveState.MoveTowardsHero;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public AutoInputKeepDistanceHeroStrategy(CharacterModel ownerModel, float castRange)
            : base(ownerModel, castRange)
        {
            _isMoveAwayFromTarget = false;
            _stayBeforeAwayTargetDistanceSqr = (castRange - s_stayBeforeAwayTargetBonusRange) * (castRange - s_stayBeforeAwayTargetBonusRange);
            _stayBeforeBackToTargetDistanceSqr = (castRange + s_stayBeforeBackToTargetBonusRange) * (castRange + s_stayBeforeBackToTargetBonusRange);
            _awayMoveSearchLength = Mathf.CeilToInt(s_awayMoveSearchMinSlotsCount * MapManager.Instance.SlotSize);
            _awayMoveSearchSpreadLength = Mathf.CeilToInt(s_awayMoveSearchMaxSlotsCount * MapManager.Instance.SlotSize);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            targetModel = EntitiesManager.Instance.HeroModel;
            OwnerModel.currentTargetedTarget = targetModel;
        }

        public override void Disable()
        {
            base.Disable();
            _heroSpawnedRegistry.Dispose();
        }

        protected override void FindNewPath()
            => RunFindPath();

        protected override void MoveOnPath()
        {
            // Stand still if the target is dead.
            if (targetModel.IsDead)
            {
                LockMovement();
                return;
            }

            if (_moveState == MoveState.MoveTowardsHero)
            {
                // If the chased hero target is near by the character by a specific distance, then make the character move away from the hero target.
                if (Vector2.SqrMagnitude(targetModel.Position - OwnerModel.Position) <= _stayBeforeAwayTargetDistanceSqr)
                {
                    _isMoveAwayFromTarget = true;
                    CheckCanUseSkill();
                    RefindNewPath();
                    return;
                }

                // If the chased hero target has moved far from the destination where the character was supposed to move to, then find another new path.
                if (Vector2.SqrMagnitude(targetModel.Position - moveToPosition) >= refindTargetThresholdSqr)
                {
                    RefindNewPath();
                    return;
                }
            }
            else if (_moveState == MoveState.MoveAwayFromHero)
            {
                // If the chased hero target is now far from the character by a specific distance, then find a new path to chase the hero again.
                if (Vector2.SqrMagnitude(targetModel.Position - OwnerModel.Position) > _stayBeforeBackToTargetDistanceSqr)
                {
                    _isMoveAwayFromTarget = false;
                    RefindNewPath();
                    return;
                }
            }

            // If the chased hero target is now near the character by the skill cast range, then stop chasing, send a trigger skill usage.
            if (Vector2.SqrMagnitude(targetModel.Position - OwnerModel.Position) <= (stopChasingTargetDistanceSqr))
                CheckCanUseSkill();

            // Make a move.
            Move();
        }

        protected override void FinishedMoving()
        {
            if (_moveState == MoveState.MoveAwayFromHero)
                _isMoveAwayFromTarget = false;

            base.FinishedMoving();
        }

        private void RunFindPath()
        {
            if (_isMoveAwayFromTarget)
                RunFindPathAwayTarget();
            else
                RunFindPathTowardsTarget();
        }

        private void RunFindPathAwayTarget()
        {
            MapManager.Instance.FindMoveAwayTargetPath(OwnerModel.Position,
                                                       targetModel.Position,
                                                       _awayMoveSearchLength,
                                                       _awayMoveSearchSpreadLength,
                                                       s_awayMoveAimStrength,
                                                       OnRunFindPathAwayTargetComplete);
        }

        private void OnRunFindPathAwayTargetComplete(Path path)
        {
            if (!path.error && path.hasPath)
            {
                _moveState = MoveState.MoveAwayFromHero;
                PathFoundCompleted(path);
            }
            else RunFindPathRandomly();
        }

        private void RunFindPathTowardsTarget()
        {
            MapManager.Instance.FindStraightPath(OwnerModel.Position,
                                                 targetModel.Position,
                                                 OnRunFindPathTowardsTargetComplete);
        }

        private void OnRunFindPathTowardsTargetComplete(Path path)
        {
            if (!path.error && path.hasPath)
            {
                _moveState = MoveState.MoveTowardsHero;
                PathFoundCompleted(path);
            }
            else RunFindPathRandomly();
        }

        private void RunFindPathRandomly()
        {
            MapManager.Instance.FindRandomPath(OwnerModel.Position,
                                               randomMoveSearchLength,
                                               randomMoveSearchSpreadLength,
                                               OnRunFindPathRandomlyComplete);
        }

        private void OnRunFindPathRandomlyComplete(Path path)
        {
            if (!path.error && path.hasPath)
            {
                _moveState = MoveState.MoveRandomly;
                PathFoundCompleted(path);
            }
            else
            {
                canFindNewPath = true;
                hasFoundAPath = false;
            }
        }

        private void CheckCanUseSkill()
            => OwnerModel.TriggerSkill();

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
        {
            _isMoveAwayFromTarget = false;
            targetModel = heroSpawnedMessage.HeroModel;
            OwnerModel.currentTargetedTarget = targetModel;
            RefindNewPath();
        }

        #endregion Class Methods
    }
}