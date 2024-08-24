using System.Collections.Generic;

namespace Runtime.Gameplay.EntitySystem
{
    public interface IEntitySkillSequenceData
    {
        #region Properties

        List<SkillModel> SkillModels { get; }
        List<float> NextSkillDelays { get; }

        #endregion Properties
    }
}