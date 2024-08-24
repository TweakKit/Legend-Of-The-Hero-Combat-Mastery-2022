using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleAfterReviveSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties

        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.InvincibleAfterRevive;
        public float TimeInvincible { get; private set; }

        #endregion Properties

        #region Class Methods


        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as InvincibleAfterReviveSkillTreeDataConfigItem;
            TimeInvincible = dataConfig.timeInvincible;
        }

        #endregion Class Methods
    }
}
