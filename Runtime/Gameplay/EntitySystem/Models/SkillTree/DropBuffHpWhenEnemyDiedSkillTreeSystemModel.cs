using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class DropBuffHpWhenEnemyDiedSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.DropBuffHpWhenEnemyDied;
        public float RateDrop { get; private set; }
        public float BuffHealthValue { get; private set; }
        public float LifeTime { get; private set; }

        #endregion Properties

        #region Members

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as DropBuffHpWhenEnemyDiedSkillTreeDataConfigItem;
            RateDrop = dataConfig.rateDrop;
            BuffHealthValue = dataConfig.buffHealthValue;
            LifeTime = dataConfig.lifeTime;
        }

        #endregion Members
    }

}