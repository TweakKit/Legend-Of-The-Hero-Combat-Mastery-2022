using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class ZombieModel : CharacterModel, IEntityDeathData
    {
        #region Properties

        public override EntityType EntityType => EntityType.Zombie;
        public DeathDataIdentity DeathDataIdentity { get; private set; }

        #endregion Properties

        #region Class Methods

        public ZombieModel(uint spawnedWaveIndex, uint zombieUId, uint zombieId, ZombieLevelModel zombieLevelModel)
            : base(spawnedWaveIndex, zombieUId, zombieId, zombieLevelModel, zombieLevelModel.SkillModels)
            => DeathDataIdentity = zombieLevelModel.DeathDataIdentity;

        #endregion Class Methods
    }

    public class ZombieLevelModel : CharacterLevelModel
    {
        #region Members

        private List<SkillModel> _skillModels;
        private DeathDataIdentity _deathDataIdentity;

        #endregion Members

        #region Properties

        public List<SkillModel> SkillModels => _skillModels;
        public DeathDataIdentity DeathDataIdentity => _deathDataIdentity;

        #endregion Properties

        #region Class Methods

        public ZombieLevelModel(uint level, int detectedPriority, CharacterStatsInfo characterStatsInfo,
                                List<SkillModel> skillModels, DeathDataIdentity deathDataIdentity)
            : base(level, detectedPriority, characterStatsInfo)
        {
            _skillModels = skillModels;
            _deathDataIdentity = deathDataIdentity;
        }

        #endregion Class Methods
    }
}