using UnityEngine;
using Runtime.Gameplay.Manager;
using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This auto input strategy updates the movement input data that would
    /// make the character move towards their allies and keep a distance from them.<br/>
    /// It also checks and updates the skill trigger input data once their allies get in their skill cast range.
    /// </summary>
    public sealed class AutoInputTowardsAllyStrategy : AutoInputStrategy
    {
        #region Class Methods

        public AutoInputTowardsAllyStrategy(CharacterModel ownerModel, float castRange)
            : base(ownerModel, castRange)
            => targetModel = null;

        protected override void FindNewPath()
            => RunFindPath();

        protected override void MoveOnPath()
        {
            if (targetModel != null)
            {
                // If the ally target is not dead, then check their position again their ally target to make some decisions.
                if (!targetModel.IsDead)
                {
                    // If the ally target is now near the character by the skill cast range, then stop chasing and send a trigger skill usage.
                    if (Vector2.SqrMagnitude(targetModel.Position - OwnerModel.Position) <= stopChasingTargetDistanceSqr)
                    {
                        LockMovement();
                        CheckCanUseSkill();
                        return;
                    }

                    // If the ally target has moved far from the destination where the character was supposed to move to, then find another new path.
                    if (Vector2.SqrMagnitude(targetModel.Position - moveToPosition) >= refindTargetThresholdSqr)
                    {
                        RefindNewPath();
                        return;
                    }
                }
                // If the ally target is dead, then find another new path.
                else
                {
                    RefindNewPath();
                    return;
                }
            }

            // Make a move.
            Move();
        }

        private void RunFindPath()
        {
            // Move towards the hero's current target.
            if (!OwnerModel.IsBeingTargeted && (targetModel = FindHeroTargetedAllyModel()) != null)
            {
                OwnerModel.currentTargetedTarget = targetModel;
                MapManager.Instance.FindStraightPath(OwnerModel.Position,
                                                     targetModel.Position,
                                                     OnRunFindPathTowardHeroTargetedAllyComplete);
            }
            // Move towards the closest ally zombie.
            else
            {
                targetModel = FindClosestTargetAllyModel();
                if (targetModel != null)
                {
                    OwnerModel.currentTargetedTarget = targetModel;
                    MapManager.Instance.FindStraightPath(OwnerModel.Position,
                                                         targetModel.Position,
                                                         OnRunFindPathTowardsClosestAllyComplete);
                }
                else RunFindPathRandomly();
            }
        }

        private void OnRunFindPathTowardHeroTargetedAllyComplete(Path path)
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

        private void OnRunFindPathTowardsClosestAllyComplete(Path path)
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

        private CharacterModel FindClosestTargetAllyModel()
        {
            float closestSqrDistance = float.MaxValue;
            CharacterModel closestAllyEnemy = null;

            foreach (var allyEnemyModel in EntitiesManager.Instance.EnemyModels)
            {
                if (OwnerModel != allyEnemyModel)
                {
                    float sqrDistanceBetween = (OwnerModel.Position - allyEnemyModel.Position).sqrMagnitude;
                    if (closestSqrDistance > sqrDistanceBetween)
                    {
                        closestSqrDistance = sqrDistanceBetween;
                        closestAllyEnemy = allyEnemyModel;
                    }
                }
            }

            return closestAllyEnemy;
        }

        private CharacterModel FindHeroTargetedAllyModel()
        {
            CharacterModel heroModel = EntitiesManager.Instance.HeroModel;
            if (heroModel.currentTargetedTarget != null && heroModel.currentTargetedTarget.EntityType.IsEnemy())
                return heroModel.currentTargetedTarget as CharacterModel;
            else
                return null;
        }

        private void CheckCanUseSkill()
            => OwnerModel.TriggerSkill();

        #endregion Class Methods
    }
}