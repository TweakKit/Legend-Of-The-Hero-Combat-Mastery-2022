using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This component is designed as a proxy for the entity that the proxy is attached to.
    /// For example: Foot of a character, little zone of/inside a trap.
    /// </summary>
    public class ProxyEntity : MonoBehaviour, IEntity
    {
        #region Members

        [SerializeField]
        private Entity _entity;

        #endregion Members

        #region Properties

        public uint EntityUId => _entity.EntityUId;

        #endregion Properties

        #region Class Methods

        public void Build(EntityModel model, Vector3 position) { }

        public T GetBehavior<T>(bool includeProxy = false) where T : class
        {
            if (includeProxy)
                return _entity.GetBehavior<T>();
            return null;
        }

        #endregion Class Methods
    }
}