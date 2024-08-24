using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class BossModel : CharacterModel, IEntitySkillSequenceData, IEntityDeathData
    {
        #region Properties

        public override EntityType EntityType => EntityType.Boss;
        public List<float> NextSkillDelays { get; private set; }
        public DeathDataIdentity DeathDataIdentity { get; private set; }

        #endregion Properties

        #region Class Methods

        public BossModel(uint spawnedWaveIndex, uint bossUId, uint bossId, BossLevelModel bossLevelModel)
            : base(spawnedWaveIndex, bossUId, bossId, bossLevelModel, bossLevelModel.SkillModels)
        {
            NextSkillDelays = bossLevelModel.NextSkillDelays;
            DeathDataIdentity = bossLevelModel.DeathDataIdentity;
        }

        #endregion Class Methods
    }

    public class BossLevelModel : CharacterLevelModel
    {
        #region Members

        private List<SkillModel> _skillModels;
        private List<float> _nextSkillDelays;
        private DeathDataIdentity _deathDataIdentity;

        #endregion Members

        #region Properties

        public List<SkillModel> SkillModels => _skillModels;
        public List<float> NextSkillDelays => _nextSkillDelays;
        public DeathDataIdentity DeathDataIdentity => _deathDataIdentity;

        #endregion Properties

        #region Class Methods

        public BossLevelModel(uint level, int detectedPriority, CharacterStatsInfo characterStatsInfo,
                              List<SkillModel> skillModels, List<float> nextSkillDelays,
                              DeathDataIdentity deathDataIdentity)
            : base(level, detectedPriority, characterStatsInfo)
        {
            _skillModels = skillModels;
            _nextSkillDelays = nextSkillDelays;
            _deathDataIdentity = deathDataIdentity;
        }

        #endregion Class Methods
    }
}