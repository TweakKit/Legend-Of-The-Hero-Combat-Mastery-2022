using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class BuffDamageTowardBossSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.BuffDamageTowardBoss;
        public float IncreaseDamagePercent { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as BuffDamageTowardBossSkillTreeDataConfigItem;
            IncreaseDamagePercent = dataConfig.increaseDamagePercent;
        }

        #endregion class Methods
    }
}