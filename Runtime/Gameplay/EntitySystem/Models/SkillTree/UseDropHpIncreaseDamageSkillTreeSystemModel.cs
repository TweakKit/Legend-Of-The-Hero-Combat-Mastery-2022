using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class UseDropHpIncreaseDamageSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.UseDropHpIncreaseDamage;
        public float IncreaseDamagePercent { get; private set; }
        public float TimeIncreaseDamage { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as UseDropHpIncreaseDamageSkillTreeDataConfigItem;
            IncreaseDamagePercent = dataConfig.increaseDamagePercent;
            TimeIncreaseDamage = dataConfig.timeIncreaseDamage;
        }

        #endregion Class Methods
    }
}
