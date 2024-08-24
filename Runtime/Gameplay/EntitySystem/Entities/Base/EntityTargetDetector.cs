using System;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class EntityTargetDetector : MonoBehaviour
    {
        #region Members

        [SerializeField] private bool _includeProxy;

        #endregion Members

        #region Properties

        private Action<EntityModel> TargetEnteredAction { get; set; } = null;
        private Action<EntityModel> TargetExitedAction { get; set; } = null;
        private Action<Collider2D> ColliderEnteredAction { get; set; } = null;
        private Action<Collider2D> ColliderExitedAction { get; set; } = null;

        #endregion Properties

        #region API Methods

        private void OnTriggerEnter2D(Collider2D collider)
        {
            ColliderEnteredAction?.Invoke(collider);
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(_includeProxy);
                if (interactable != null && !interactable.Model.IsDead)
                {
                    TargetEnteredAction?.Invoke(interactable.Model);
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            ColliderExitedAction?.Invoke(collider);
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(_includeProxy);
                if (interactable != null)
                {
                    TargetExitedAction?.Invoke(interactable.Model);
                }
            }
        }

        #endregion API Methods

        #region Class Methods

        /// <summary>
        /// For the collider whose shape is manually configured, meaning there is no calculation at runtime
        /// taken into account for the shape properties.
        /// <param name="targetEnteredAction">Action callback fired when a target has entered the collider.</param>
        /// <param name="targetExitedAction">Action callback fired when a target has exited the collider.</param>
        /// </summary>
        public void Init(Action<EntityModel> targetEnteredAction = null, Action<EntityModel> targetExitedAction = null,
                         Action<Collider2D> colliderEnteredAction = null, Action<Collider2D> colliderExitedAction = null)
        {
            TargetEnteredAction = targetEnteredAction;
            TargetExitedAction = targetExitedAction;
            ColliderEnteredAction = colliderEnteredAction;
            ColliderExitedAction = colliderExitedAction;
        }

        /// <summary>
        /// For the collider whose shape is circle, pass in the detect range as the detect diameter for the collider.
        /// </summary>
        /// <param name="detectRange">The detect diameter.</param>
        /// <param name="targetEnteredAction">Action callback fired when a target has entered the collider.</param>
        /// <param name="targetExitedAction">Action callback fired when a target has exited the collider.</param>
        /// </summary>
        public void Init(float detectRange, Action<EntityModel> targetEnteredAction, Action<EntityModel> targetExitedAction,
                        Action<Collider2D> colliderEnteredAction = null, Action<Collider2D> colliderExitedAction = null)
        {
            Init(targetEnteredAction, targetExitedAction, colliderEnteredAction, colliderExitedAction);
            var circleCollider = gameObject.GetComponent<CircleCollider2D>();
            if (circleCollider != null)
                circleCollider.radius = detectRange;
        }

        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);

        #endregion Class Methods
    }
}