using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class IncreaseAttackSpeedWhenKillEnemySkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.IncreaseAttackSpeedWhenKillEnemy;
        public float RateBuff { get; private set; }
        public float IncreaseAttackSpeedPercent { get; private set; }
        public float TimeBuff { get; private set; }

        #endregion Properties

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as IncreaseAttackSpeedWhenKillEnemySkillTreeDataConfigItem;
            RateBuff = dataConfig.rateBuff;
            IncreaseAttackSpeedPercent = dataConfig.increaseAttackSpeedPercent;
            TimeBuff = dataConfig.timeBuff;
        }
    }
}
