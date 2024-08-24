using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWhenReceiveDamageSkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties
        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.InvincibleWhenReceiveDamage;
        public float TimeInvincible { get; private set; }
        public float Cooldown { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as InvincibleWhenReceiveDamageSkillTreeDataConfigItem;
            TimeInvincible = dataConfig.timeInvincible;
            Cooldown = dataConfig.cooldown;
        }

        #endregion Class Methods
    }
}
