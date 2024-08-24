using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class TrapCauseMoreDamageForEnemySkillTreeSystemModel : SkillTreeSystemModel
    {
        #region Properties
        
        public override SkillTreeSystemType SkillTreeType => SkillTreeSystemType.TrapCauseMoreDamageForEnemy;
        public float IncreaseDamagePercent { get; private set; }

        #endregion Properties

        #region Class Methods

        public override void Init(SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            var dataConfig = skillTreeDataConfigItem as TrapCauseMoreDamageForEnemySkillTreeDataConfigItem;
            IncreaseDamagePercent = dataConfig.increaseDamagePercent;
        }

        #endregion Class Methods
    }

}