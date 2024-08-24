using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public static class SkillTreeSystemModelFactory
    {
        #region Class Methods

        public static SkillTreeSystemModel GetSkillTreeSystemModel(SkillTreeSystemType skillTreeSystemType, SkillTreeDataConfigItem skillTreeDataConfigItem)
        {
            SkillTreeSystemModel skillTreeSystemModel = null;

            switch (skillTreeSystemType)
            {
                case SkillTreeSystemType.InvincibleAfterRevive:
                    skillTreeSystemModel = new InvincibleAfterReviveSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.DropBuffHpWhenEnemyDied:
                    skillTreeSystemModel = new DropBuffHpWhenEnemyDiedSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.BuffStatWhenStartByAds:
                    skillTreeSystemModel = new BuffStatWhenStartByAdsSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.InvincibleWhenGetFinishDamage:
                    skillTreeSystemModel = new InvincibleWhenGetFinishDamageSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.TrapCauseLessDamageForHero:
                    skillTreeSystemModel = new TrapCauseLessDamageForHeroSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.DropBuffHpWhenAssetDestroyed:
                    skillTreeSystemModel = new DropBuffHpWhenAssetDestroyedSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.BuffDamageTowardBoss:
                    skillTreeSystemModel = new BuffDamageTowardBossSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.DropHpIncreaseValue:
                    skillTreeSystemModel = new DropHpIncreaseValueSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.InvincibleWithFirstBossAttacks:
                    skillTreeSystemModel = new InvincibleWithFirstBossAttacksSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.UseDropHpIncreaseDamage:
                    skillTreeSystemModel = new UseDropHpIncreaseDamageSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.TrapCauseMoreDamageForEnemy:
                    skillTreeSystemModel = new TrapCauseMoreDamageForEnemySkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.ExplodeDiedEnemy:
                    skillTreeSystemModel = new ExplodeDiedEnemySkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.InvincibleWhenReceiveDamage:
                    skillTreeSystemModel = new InvincibleWhenReceiveDamageSkillTreeSystemModel();
                    break;
                case SkillTreeSystemType.IncreaseAttackSpeedWhenKillEnemy:
                    skillTreeSystemModel = new IncreaseAttackSpeedWhenKillEnemySkillTreeSystemModel();
                    break;
                default:
                    break;
            }


            if (skillTreeSystemModel != null)
            {
                skillTreeSystemModel.Init(skillTreeDataConfigItem);
                return skillTreeSystemModel;
            }
            return null;
        }

        #endregion Class Methods
    }

}
