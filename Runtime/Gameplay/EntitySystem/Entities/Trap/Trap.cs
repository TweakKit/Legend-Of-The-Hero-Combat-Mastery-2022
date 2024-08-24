using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This entity causes damage to other targets when they step in and get trapped.<br/>
    /// It's supposed to be designed as an entity with many behaviors attached, but because it doesn't have
    /// many behaviors and for the sack of simplicity and performance, its behaviors are written in a single class.<br/>
    /// That's why it doesn't drive from the BehavioralEntity class, instead drives from the Entity class.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Trap : Entity
    {
        #region Members

        private TrapModel _ownerModel;

        #endregion Members

        #region API Methods

        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (_ownerModel == null)
                return;
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(true);
                if (interactable != null && !interactable.Model.IsDead)
                    StartTriggering(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (_ownerModel == null)
                return;
            var entity = collider.GetComponent<IEntity>();
            if (entity != null)
            {
                var interactable = entity.GetBehavior<IInteractable>(true);
                if (interactable != null)
                    StopTriggering(interactable);
            }
        }

        #endregion API Methods

        #region Class Methods

        public override void Build(EntityModel model, Vector3 position)
        {
            base.Build(model, position);
            _ownerModel = model as TrapModel;
        }

        public override T GetBehavior<T>(bool includeProxy) where T : class => this as T;

        private void StartTriggering(IInteractable trappedTarget)
        {
            _ownerModel.TriggerStartedEvent.Invoke();
            var damageInfo = _ownerModel.GetDamageInfo(trappedTarget.Model);
            var damageDirection = (trappedTarget.Model.Position - _ownerModel.Position).normalized;
            trappedTarget.GetTrapped(_ownerModel.trapType, damageInfo, new DamageMetaData(damageDirection, _ownerModel.Position));
        }

        private void StopTriggering(IInteractable trappedTarget)
        {
            _ownerModel.TriggerStoppedEvent.Invoke();
            trappedTarget.StopTrapped(_ownerModel.trapType);
        }

        #endregion Class Methods
    }
}