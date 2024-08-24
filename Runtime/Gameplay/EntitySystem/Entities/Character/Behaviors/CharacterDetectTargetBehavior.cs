using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.CollisionDetection;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior helps the character detect targets within a specified radius.<br/>
    /// </summary>
    public class CharacterDetectTargetBehavior : CharacterBehavior, ICollisionBody, IUpdatable, IDisable
    {
        #region Members

        protected List<EntityModel> detectedTargets;
        protected bool notifyTargetDetection;
        protected ICollisionShape collisionShape;
        protected Collider2D collider;
        protected CollisionSearchTargetType[] collisionSearchTargetTypes;

        #endregion Members

        #region Properties

        public int RefId { get; set; }
        public ICollisionShape CollisionShape => collisionShape;
        public Vector2 CollisionSystemPosition => ownerModel.Position;
        public Collider2D Collider => null;
        public CollisionBodyType CollisionBodyType => CollisionBodyType.TargetDetect;

        public CollisionSearchTargetType[] CollisionSearchTargetTypes => collisionSearchTargetTypes;

        #endregion Properties

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            collisionSearchTargetTypes = ownerModel.EntityType.GetCollisionBodySearchTypes();
            detectedTargets = new List<EntityModel>();
            notifyTargetDetection = Constants.CanNotifyTargetDetection(ownerModel.EntityType);
            var tryGetDetectRange = 0.0f;
            if (ownerModel.TryGetStat(StatType.DetectRange, out tryGetDetectRange))
            {
                var detectRange = tryGetDetectRange;
                collisionShape = this.CreateCollisionShape(detectRange);
                CollisionSystem.Instance.AddBody(this);
                ownerModel.StatChangedEvent += OnChangedStat;
                return true;
            }
#if DEBUGGING
            else
            {
                Debug.LogError($"Require {StatType.DetectRange} for this behavior to work!");
                return false;
            }
#else
            else return false;
#endif
        }

        public void Update()
        {
            if (ownerModel.CanDetectTarget)
                UpdateClosestTarget();
        }

        private void OnChangedStat(StatType statType, float value)
        {
            if (statType == StatType.DetectRange)
            {
                detectedTargets.Clear();
                ownerModel.currentTargetedTarget = null;
                CollisionSystem.Instance.RemoveBody(this);
                var detectRange = ownerModel.GetTotalStatValue(StatType.DetectRange);
                collisionShape = this.CreateCollisionShape(detectRange);
                CollisionSystem.Instance.AddBody(this);
            }
        }

        protected virtual void UpdateClosestTarget()
        {
            var closestTarget = GetClosestTarget();
            if (closestTarget != null)
            {
                if (ownerModel.currentTargetedTarget != closestTarget)
                {
                    if (ownerModel.currentTargetedTarget != null && notifyTargetDetection)
                        NotifyTargetExitDetection(ownerModel.currentTargetedTarget);

                    ownerModel.currentTargetedTarget = closestTarget;

                    if (notifyTargetDetection)
                        NotifyTargetDetected(ownerModel.currentTargetedTarget);
                }
            }
            else
            {
                if (ownerModel.currentTargetedTarget != null)
                {
                    NotifyTargetExitDetection(ownerModel.currentTargetedTarget);
                    ownerModel.currentTargetedTarget = null;
                }
            }
        }

        protected virtual EntityModel GetClosestTarget()
        {
            int highestPriority = int.MinValue;
            foreach (var detectedTarget in detectedTargets)
            {
                if (highestPriority < detectedTarget.DetectedPriority)
                    highestPriority = detectedTarget.DetectedPriority;
            }

            float closestSqrDistance = float.MaxValue;
            EntityModel closestTarget = null;

            foreach (var detectedTarget in detectedTargets)
            {
                if (detectedTarget.DetectedPriority == highestPriority)
                {
                    var direction = (ownerModel.Position - detectedTarget.Position).normalized;
                    var sqrDistanceBetween = (ownerModel.Position - detectedTarget.GetEdgePoint(direction)).sqrMagnitude;
                    var currentCheckForwardDistance = MapManager.Instance.SlotHalfSize;
                    var hasAnyColliderBetween = false;
                    while (sqrDistanceBetween > currentCheckForwardDistance * currentCheckForwardDistance)
                    {
                        var checkForwardPosition = detectedTarget.Position + direction * currentCheckForwardDistance;
                        currentCheckForwardDistance += MapManager.Instance.SlotHalfSize;
                        if (!MapManager.Instance.IsWalkable(checkForwardPosition))
                        {
                            hasAnyColliderBetween = true;
                            break;
                        }
                    }
                    if (!hasAnyColliderBetween && closestSqrDistance > sqrDistanceBetween)
                    {
                        closestSqrDistance = sqrDistanceBetween;
                        closestTarget = detectedTarget;
                    }
                }
            }

            if (closestTarget == null)
            {
                float fallbackClosestSqrDistance = float.MaxValue;
                EntityModel fallbackClosestTarget = null;
                foreach (var detectedTarget in detectedTargets)
                {
                    if (detectedTarget.DetectedPriority == highestPriority)
                    {
                        var direction = (ownerModel.Position - detectedTarget.Position).normalized;
                        var sqrDistanceBetween = (ownerModel.Position - detectedTarget.GetEdgePoint(direction)).sqrMagnitude;
                        if (fallbackClosestSqrDistance > sqrDistanceBetween)
                        {
                            fallbackClosestSqrDistance = sqrDistanceBetween;
                            fallbackClosestTarget = detectedTarget;
                        }
                    }
                }
                return fallbackClosestTarget;
            }

            return closestTarget;
        }

        protected virtual void NotifyTargetDetected(EntityModel target) => target.IsBeingTargeted = true;
        protected virtual void NotifyTargetExitDetection(EntityModel target) => target.IsBeingTargeted = false;

        public void OnCollision(CollisionResult result, ICollisionBody other)
        {
            if (other.Collider != null)
            {
                if (result.collisionType == CollisionType.Enter)
                {
                    var interactable = GetInteractable(other.Collider);
                    if (interactable != null)
                        detectedTargets.Add(interactable.Model);
                }
                else if (result.collisionType == CollisionType.Exit)
                {
                    var interactable = GetInteractable(other.Collider);
                    if (interactable != null)
                    {
                        detectedTargets.Remove(interactable.Model);
                    }
                }
            }
        }

        private IInteractable GetInteractable(Collider2D collider)
        {
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>();
                return interactable;
            }
            return null;
        }

        public virtual void Disable()
        {
            if (notifyTargetDetection && ownerModel.currentTargetedTarget != null)
                NotifyTargetExitDetection(ownerModel.currentTargetedTarget);
            CollisionSystem.Instance.RemoveBody(this);
        }

        #endregion Class Methods
    }
}