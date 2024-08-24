namespace Runtime.Gameplay.EntitySystem
{
    public class AdditionalSkillSequenceEntityTransformationProperty : EntityTransformationProperty
    {
        #region Properties

        public SkillModel SkillModel { get; private set; }
        public float NextSkillDelay { get; private set; }
        public bool UseSimultaneouslyWithOthers { get; private set; }
        public int[] SimultaneousSkillIndexes { get; private set; }

        #endregion Properties

        #region Class Methods

        public AdditionalSkillSequenceEntityTransformationProperty(SkillModel skillModel, float nextSkillDelay,
                                                                   bool useSimultaneouslyWithOthers, int[] simultaneousSkillIndexes)
        {
            SkillModel = skillModel;
            NextSkillDelay = nextSkillDelay;
            UseSimultaneouslyWithOthers = useSimultaneouslyWithOthers;
            SimultaneousSkillIndexes = simultaneousSkillIndexes;
        }

        #endregion Class Methods
    }
}