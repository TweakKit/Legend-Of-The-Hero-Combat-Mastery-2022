using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IEntityBehavior
    {
        #region Interface Methods

        bool InitModel(EntityModel model, Transform transform);

        #endregion Interface Methods
    }
}