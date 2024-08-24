using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWithFirstBossAttacksSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.InvincibleWithFirstBossAttacks;
        public int NumberOfAttacks { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as InvincibleWithFirstBossAttacksSkillTreeDataConfigItem;
            NumberOfAttacks = dataConfig.numberOfAttacks;
        }

        #endregion Class Methods
    }
}
