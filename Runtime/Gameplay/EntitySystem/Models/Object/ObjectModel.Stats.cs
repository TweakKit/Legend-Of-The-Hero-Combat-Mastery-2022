using System;
using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ObjectModel : EntityModel
    {
        #region Members

        public int currentDestroyHits;
        protected int destroyHits;

        #endregion Members

        #region Properties

        public int DestroyHits => destroyHits;

        #endregion Properties

        protected partial void InitStats(ObjectLevelConfigItem objectConfigItem)
        {
            destroyHits = (int)objectConfigItem.destroyHits;
            currentDestroyHits = destroyHits;
        }
    }
}