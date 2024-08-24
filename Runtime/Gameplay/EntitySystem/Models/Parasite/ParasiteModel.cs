using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ParasiteModel : EntityModel
    {
        #region Members

        private CharacterModel _hostModel;

        #endregion Members

        #region Properties

        public override EntityType EntityType => EntityType.Parasite;
        public Vector3 HostPosition => _hostModel.Position;

        #endregion Properties

        #region Class Methods

        public ParasiteModel(uint spawnedWaveIndex, uint parasiteUId, uint parasiteId, CharacterModel hostModel)
            : base(spawnedWaveIndex, parasiteUId, parasiteId.ToString(), Constants.IGNORE_PRIORITY_DETECT)
        {
            InitEvents();
            _hostModel = hostModel;
            _hostModel.DeathEvent += _ => DestroyWithHostEvent.Invoke();
        }

        protected partial void InitEvents();

        #endregion Class Methods
    }
}