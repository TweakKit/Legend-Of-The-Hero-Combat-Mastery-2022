using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IEntity
    {
        #region Properties

        public uint EntityUId { get; }

        #endregion Properties

        #region Interface Methods

        void Build(EntityModel model, Vector3 position);
        T GetBehavior<T>(bool includeProxy = false) where T : class;

        #endregion Interface Methods
    }
}