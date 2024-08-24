using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using Core.Foundation.PubSub;
using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This auto input strategy updates the movement input data
    /// that would make the character always move towards the hero.<br/>
    /// It also checks and updates the skill trigger input data once the hero gets in their skill cast range.
    /// </summary>
    public sealed class AutoInputTowardsHeroStrategy : AutoInputStrategy
    {
        #region Members

        private static float s_stoppingDistance = 0.5f;
        private float _castRange;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public AutoInputTowardsHeroStrategy(CharacterModel ownerModel, float castRange)
            : base(ownerModel, s_stoppingDistance)
        {
            _castRange = castRange;
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            targetModel = EntitiesManager.Instance.HeroModel;
            OwnerModel.currentTargetedTarget = targetModel;
        }

        public override void Disable()
        {
            base.Disable();
            _heroSpawnedRegistry.Dispose();
        }

        protected override void FindNewPath() => RunFindPathToTarget();

        protected override void MoveOnPath()
        {
            // Stand still if the target is dead.
            if (targetModel.IsDead)
            {
                LockMovement();
                return;
            }

            // If the chased target is now near the character by the skill cast range, then stop chasing and send a trigger skill usage.
            if (Vector2.SqrMagnitude(targetModel.Position - OwnerModel.Position) <= _castRange * _castRange)
                TriggerSkill();

            // If the target has moved far from the destination where the character was supposed to move to, then find another new path.
            if (Vector2.SqrMagnitude(targetModel.Position - moveToPosition) >= refindTargetThresholdSqr && currentRefindTargetTime > RefindTargetMinTime)
            {
                currentRefindTargetTime = 0.0f;
                RefindNewPath();
                return;
            }

            // Make a move.
            Move();
        }

        private void RunFindPathToTarget()
        {
            MapManager.Instance.FindPathWithRandomness(OwnerModel.Position,
                                                       targetModel.Position,
                                                       OnRunFindPathToTargetComplete);
        }

        private void OnRunFindPathToTargetComplete(Path path)
        {
            if (!path.error && path.hasPath)
                PathFoundCompleted(path);
            else
                RunFindPathToAroundTarget();
        }

        private void RunFindPathToAroundTarget()
        {
            MapManager.Instance.FindNeighbourTargetPath(OwnerModel.Position,
                                                        targetModel.Position,
                                                        OnRunFindPathToAroundTargetComplete);
        }

        private void OnRunFindPathToAroundTargetComplete(Path path)
        {
            if (!path.error && path.hasPath)
                PathFoundCompleted(path);
            else
                RunFindPathRandomly();
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
                PathFoundCompleted(path);
            }
            else
            {
                canFindNewPath = true;
                hasFoundAPath = false;
            }
        }

        private void TriggerSkill()
            => OwnerModel.TriggerSkill();

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
        {
            currentRefindTargetTime = 0;
            targetModel = heroSpawnedMessage.HeroModel;
            OwnerModel.currentTargetedTarget = targetModel;
            RefindNewPath();
        }

        #endregion Class Methods
    }
}