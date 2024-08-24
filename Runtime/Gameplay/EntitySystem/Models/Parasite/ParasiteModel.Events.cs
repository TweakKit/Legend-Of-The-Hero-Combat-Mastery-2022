using System;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ParasiteModel : EntityModel
    {
        #region Properties

        public Action DestroyWithHostEvent { get; set; }

        #endregion Properties

        #region Class Methods

        protected partial void InitEvents()
            => DestroyWithHostEvent = () => { };

        #endregion Class Methods
    }
}