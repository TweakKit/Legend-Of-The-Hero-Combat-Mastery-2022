using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWhenGetFinishDamageSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Members

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.InvincibleWhenGetFinishDamage;
        public float TimeInvincible { get; private set; }

        #endregion Members

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as InvincibleWhenGetFinishDamageSkillTreeDataConfigItem;
            TimeInvincible = dataConfig.timeInvincible;
        }

        #endregion Class Methods
    }
}
