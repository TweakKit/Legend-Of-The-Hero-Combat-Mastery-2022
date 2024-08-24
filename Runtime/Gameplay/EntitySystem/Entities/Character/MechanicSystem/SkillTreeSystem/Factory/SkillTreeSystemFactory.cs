using Runtime.ConfigModel;

namespace Runtime.Gameplay.EntitySystem
{
    public class SkillTreeSystemFactory
    {
        public static ISkillTreeMechanicSystem GetSkillTreeSystem(SkillTreeSystemType skillTreeSystemType)
        {
            switch (skillTreeSystemType)
            {
                case SkillTreeSystemType.InvincibleAfterRevive:
                    return new InvincibleAfterReviveSkillTreeSystem();
                case SkillTreeSystemType.DropBuffHpWhenEnemyDied:
                    return new DropBuffHpWhenEnemyDiedSkillTreeSystem();
                case SkillTreeSystemType.BuffStatWhenStartByAds:
                    return new BuffStatWhenStartByAdsSkillTreeSystem();
                case SkillTreeSystemType.InvincibleWhenGetFinishDamage:
                    return new InvincibleWhenGetFinishDamageSkillTreeSystem();
                case SkillTreeSystemType.TrapCauseLessDamageForHero:
                    return new TrapCauseLessDamageForHeroSkillTreeSystem();
                case SkillTreeSystemType.DropBuffHpWhenAssetDestroyed:
                    return new DropBuffHpWhenAssetDestroyedSkillTreeSystem();
                case SkillTreeSystemType.BuffDamageTowardBoss:
                    return new BuffDamageTowardBossSkillTreeSystem();
                case SkillTreeSystemType.DropHpIncreaseValue:
                    return new DropHpIncreaseValueSkillTreeSystem();
                case SkillTreeSystemType.InvincibleWithFirstBossAttacks:
                    return new InvincibleWithFirstBossAttackSkillTreeSystem();
                case SkillTreeSystemType.UseDropHpIncreaseDamage:
                    return new UseDropHpIncreaseDamageSkillTreeSystem();
                case SkillTreeSystemType.TrapCauseMoreDamageForEnemy:
                    return new TrapCauseMoreDamageForEnemySkillTreeSystem();
                case SkillTreeSystemType.ExplodeDiedEnemy:
                    return new ExplodeDiedEnemySkillTreeSystem();
                case SkillTreeSystemType.InvincibleWhenReceiveDamage:
                    return new InvincibleWhenReceiveDamageSkillTreeSystem();
                case SkillTreeSystemType.IncreaseAttackSpeedWhenKillEnemy:
                    return new IncreaseAttackSpeedWhenKillEnemySkillTreeSystem();
                default:
                    break;
            }

            return null;
        }
    }
}
