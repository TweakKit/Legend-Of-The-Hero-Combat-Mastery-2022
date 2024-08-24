using System.Collections.Generic;
using Runtime.ConfigModel;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class TransformedBossModel : BossModel, IEntityAdditionalSkillSequenceData
    {
        #region Properties

        public SkillModel AdditionalSkillModel { get; private set; }
        public float AdditionalNextSkillDelay { get; private set; }
        public bool UseSimultaneouslyWithOthers { get; private set; }
        public int[] SimultaneousSkillIndexes { get; private set; }

        #endregion Properties   

        #region Class Methods

        public TransformedBossModel(uint spawnedWaveIndex, uint transformedBossUId, uint transformedBossId, TransformedBossLevelModel transformedBossLevelModel)
            : base(spawnedWaveIndex, transformedBossUId, transformedBossId, transformedBossLevelModel)
        {
            foreach (var entityTransformationProperty in transformedBossLevelModel.EntityTransformationProperties)
            {
                if (entityTransformationProperty is AdditionalSkillSequenceEntityTransformationProperty transformationProperty)
                {
                    AdditionalSkillModel = transformationProperty.SkillModel;
                    AdditionalNextSkillDelay = transformationProperty.NextSkillDelay;
                    UseSimultaneouslyWithOthers = transformationProperty.UseSimultaneouslyWithOthers;
                    SimultaneousSkillIndexes = UseSimultaneouslyWithOthers ? transformationProperty.SimultaneousSkillIndexes : null;
                }
            }
        }

        #endregion Class Methods
    }

    public class TransformedBossLevelModel : BossLevelModel
    {
        #region Members

        private List<EntityTransformationProperty> _entityTransformationProperties;

        #endregion Members

        #region Properties

        public List<EntityTransformationProperty> EntityTransformationProperties => _entityTransformationProperties;

        #endregion Properties

        #region Class Methods

        public TransformedBossLevelModel(uint level, int detectedPriority, CharacterStatsInfo bossStatsInfo,
                                         List<SkillModel> skillModels, List<float> nextSkillDelays,
                                         List<EntityTransformationProperty> entityTransformationProperties)
            : base(level, detectedPriority, bossStatsInfo, skillModels, nextSkillDelays, DeathDataIdentity.None)
            => _entityTransformationProperties = entityTransformationProperties;

        #endregion Class Methods
    }
}