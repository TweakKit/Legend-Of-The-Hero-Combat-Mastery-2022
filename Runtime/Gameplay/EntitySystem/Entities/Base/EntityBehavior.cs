using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class EntityBehavior : IEntityBehavior
    {
        #region Members

        protected Transform ownerTransform;

        #endregion Members

        #region Class Methods

#if UNITY_EDITOR
        public virtual void Validate(Transform ownerTransform) { }
#endif

        public virtual bool InitModel(EntityModel model, Transform transform)
        {
            ownerTransform = transform;
            return true;
        }

        #endregion Class Methods
    }
}