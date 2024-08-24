using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract class ObjectBehavior : EntityBehavior
    {
        #region Members

        protected ObjectModel ownerModel;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel = model as ObjectModel;
            return true;
        }

        #endregion Class Methods
    }
}