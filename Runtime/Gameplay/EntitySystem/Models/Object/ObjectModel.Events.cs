using System;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ObjectModel : EntityModel
    {
        #region Properties

        public Action<bool> TargetedEvent { get; set; }
        public Action DestroyEvent { get; set; }

        #endregion Properties

        #region Class Methods

        protected partial void InitEvents(ObjectLevelConfigItem objectConfigItem)
        {
            TargetedEvent = _ => { };
            DestroyEvent = () => { };
        }

        #endregion Class Methods
    }
}