using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public static class SkillStrategyFactory
    {
        #region Class Methods

        public static ISkillStrategy GetSkillStrategy(SkillType skillType)
        {
            switch (skillType)
            {
                case SkillType.RushAttack:
                    return new RushAttackSkillStrategy();

                case SkillType.ForwardAttack:
                    return new ForwardAttackSkillStrategy();

                case SkillType.SpawnFlail:
                    return new SpawnFlailSkillStrategy();

                case SkillType.FireProjectile:
                    return new FireProjectileSkillStrategy();

                case SkillType.FireRotatedScaledProjectile:
                    return new FireRotatedScaledProjectileSkillStrategy();

                case SkillType.SpawnForwardScaledImpacts:
                    return new SpawnForwardScaledImpactsSkillStrategy();

                case SkillType.SpawnRayHit:
                    return new SpawnRayHitSkillStrategy();

                case SkillType.Summon:
                    return new SummonSkillStrategy();

                case SkillType.SummonRayPillarsToFire:
                    return new SummonRayPillarsToFireSkillStrategy();

                case SkillType.FireRoundBullet:
                    return new FireRoundBulletSkillStrategy();

                case SkillType.MoveUnderGround:
                    return new MoveUnderGroundSkillStrategy();

                case SkillType.JumpAttack:
                    return new JumpAttackSkillStrategy();

                case SkillType.RushAndSlash:
                    return new RushAndSlashSkillStrategy();

                case SkillType.ThrowBodyProjectile:
                    return new ThrowBodyProjectileSkillStrategy();
            }

            return null;
        }

        #endregion Class Methods
    }
}