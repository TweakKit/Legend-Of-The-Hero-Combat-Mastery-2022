using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// Base behavior for all character's behaviors.<br/>
    /// Each behavior plays a role in the character entity.<br/>
    /// </summary>
    public abstract class CharacterBehavior : EntityBehavior
    {
        #region Members

        protected CharacterModel ownerModel;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);
            ownerModel = model as CharacterModel;
            return true;
        }

        #endregion Class Methods
    }
}