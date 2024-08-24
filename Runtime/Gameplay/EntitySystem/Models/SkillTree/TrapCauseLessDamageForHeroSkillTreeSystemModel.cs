using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class TrapCauseLessDamageForHeroSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Class Methods

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.TrapCauseLessDamageForHero;
        public float DecreaseDamagePercent { get; private set; }

        #endregion Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as TrapCauseLessDamageForHeroSkillTreeDataConfigItem;
            DecreaseDamagePercent = dataConfig.decreaseDamagePercent;
        }
    }
}