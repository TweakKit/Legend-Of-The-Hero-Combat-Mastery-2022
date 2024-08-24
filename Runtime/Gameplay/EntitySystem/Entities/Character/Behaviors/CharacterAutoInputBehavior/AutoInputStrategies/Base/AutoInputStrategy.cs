using System.Collections.Generic;
using UnityEngine;
using Runtime.Gameplay.Manager;
using Runtime.Definition;
using Pathfinding;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This base auto input strategy updates the movement input data of the character by a specified movement pattern.
    /// </summary>
    public abstract class AutoInputStrategy : IAutoInputStrategy
    {
        #region Members

        protected static readonly float RefindTargetBonusRange = 3.0f;
        protected static readonly float RefindTargetMinTime = 0.5f;
        protected static readonly int RandomMoveSearchSlotsCount = 3;
        protected static readonly int RandomMoveSearchSpreadSlotsCount = 2;
        protected bool canFindNewPath;
        protected bool hasFoundAPath;
        protected int pathMoveIndex;
        protected float moveSpeed;
        protected float stopChasingTargetDistanceSqr;
        protected float refindTargetThresholdSqr;
        protected float currentRefindTargetTime;
        protected int randomMoveSearchLength;
        protected int randomMoveSearchSpreadLength;
        protected Vector2 pathTargetPosition;
        protected Vector2 moveToPosition;
        protected List<Vector3> pathPositions;
        protected CharacterModel targetModel;

        #endregion Members

        #region Properties

        protected CharacterModel OwnerModel { get; private set; }

        #endregion Properties

        #region Class Methods

        public AutoInputStrategy(CharacterModel ownerModel, float castRange)
        {
            OwnerModel = ownerModel;
            canFindNewPath = true;
            hasFoundAPath = false;
            stopChasingTargetDistanceSqr = castRange * castRange;
            refindTargetThresholdSqr = (castRange + RefindTargetBonusRange) * (castRange + RefindTargetBonusRange);
            randomMoveSearchLength = Mathf.CeilToInt(RandomMoveSearchSlotsCount * MapManager.Instance.SlotSize);
            randomMoveSearchSpreadLength = Mathf.CeilToInt(RandomMoveSearchSpreadSlotsCount * MapManager.Instance.SlotSize);
            currentRefindTargetTime = 0.0f;
            var tryGetMoveSpeed = 0.0f;
            if (ownerModel.TryGetStat(StatType.MoveSpeed, out tryGetMoveSpeed))
            {
                moveSpeed = tryGetMoveSpeed;
                ownerModel.StatChangedEvent += OnStatChanged;
            }
#if DEBUGGING
            else
            {
                Debug.LogError($"Require {StatType.MoveSpeed} for this behavior to work!");
                return;
            }
#endif
        }

        public virtual void Update()
        {
            CheckFindPath();
            if (CheckCanMoveOnPath())
                MoveOnPath();
        }

        public virtual void Disable() { }

        protected virtual void CheckFindPath()
        {
            if (canFindNewPath)
            {
                canFindNewPath = false;
                FindNewPath();
            }
        }

        protected abstract void FindNewPath();
        protected abstract void MoveOnPath();
        protected virtual void FinishedMoving()
        {
            LockMovement();
            RefindNewPath();
        }

        protected virtual bool CheckCanMoveOnPath()
            => hasFoundAPath;

        protected virtual void PathFoundCompleted(Path newPath)
        {
            pathPositions = newPath.vectorPath;
            pathMoveIndex = 0;
            pathTargetPosition = OwnerModel.Position;
            moveToPosition = pathPositions[pathPositions.Count - 1];
            hasFoundAPath = true;
        }

        protected virtual void Move()
        {
            currentRefindTargetTime += Time.deltaTime;
            var nextPosition = Vector2.MoveTowards(OwnerModel.Position, pathTargetPosition, Time.deltaTime * moveSpeed);
            if (Vector2.SqrMagnitude(pathTargetPosition - nextPosition) < (Time.deltaTime * moveSpeed * Time.deltaTime * moveSpeed))
            {
                pathMoveIndex++;
                if (pathMoveIndex == pathPositions.Count)
                {
                    FinishedMoving();
                    return;
                }
                else pathTargetPosition = new Vector2(pathPositions[pathMoveIndex].x, pathPositions[pathMoveIndex].y);
            }
            else
            {
                Vector2 moveDirection = (nextPosition - OwnerModel.Position).normalized;
                OwnerModel.SetMoveDirection(moveDirection);
            }
        }

        protected virtual void RefindNewPath()
        {
            hasFoundAPath = false;
            canFindNewPath = true;
        }

        protected virtual void LockMovement()
            => OwnerModel.SetMoveDirection(Vector2.zero);

        protected virtual void OnStatChanged(StatType statType, float updatedValue)
        {
            if (statType == StatType.MoveSpeed)
                moveSpeed = updatedValue;
        }

        #endregion Class Methods
    }
}