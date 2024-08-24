namespace Runtime.Gameplay.EntitySystem
{
    public interface IEntityAdditionalSkillSequenceData
    {
        #region Members

        SkillModel AdditionalSkillModel { get; }
        float AdditionalNextSkillDelay { get; }
        bool UseSimultaneouslyWithOthers { get; }
        int[] SimultaneousSkillIndexes { get; }

        #endregion Members
    }
}