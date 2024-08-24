using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class ExplodeDiedEnemySkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.ExplodeDiedEnemy;
        public float RateExplode { get; private set; }
        public float HealthPercentToDamage { get; private set; }

        #endregion Properties

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as ExplodeDiedEnemySkillTreeDataConfigItem;
            RateExplode = dataConfig.rateExplode;
            HealthPercentToDamage = dataConfig.healthPercentToDamage;
        }
    }
}
