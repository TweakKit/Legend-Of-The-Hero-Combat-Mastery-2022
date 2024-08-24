using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class DropHpIncreaseValueSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.DropHpIncreaseValue;
        public float IncreaseBuffPercent { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as DropHpIncreaseValueSkillTreeDataConfigItem;
            IncreaseBuffPercent = dataConfig.increaseBuffPercent;
        }

        #endregion Class Methods
    }

}