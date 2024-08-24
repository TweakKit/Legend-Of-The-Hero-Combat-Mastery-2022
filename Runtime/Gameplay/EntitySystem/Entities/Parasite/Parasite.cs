using UnityEngine;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This entity represents a parasite. And AFAWK, its exsistence is dependent on other entities.
    /// For example, the ray pillar can be thought of a parasite.<br/>
    /// It's supposed to be designed as an entity with many behaviors attached, but because it doesn't have
    /// many behaviors and for the sack of simplicity and performance, its behaviors are written in a single class.<br/>
    /// That's why it doesn't drive from the BehavioralEntity class, instead drives from the Entity class.
    /// </summary>
    [DisallowMultipleComponent]
    public class Parasite : Entity
    {
        #region Class Methods

        public override void Build(EntityModel model, Vector3 position)
        {
            base.Build(model, position);
            var ownerModel = model as ParasiteModel;
            ownerModel.DestroyWithHostEvent += OnDestroyWithHost;
        }

        public override T GetBehavior<T>(bool includeProxy) where T : class => this as T;

        private void OnDestroyWithHost()
            => PoolManager.Instance.Remove(gameObject);

        #endregion Class Methods
    }
}