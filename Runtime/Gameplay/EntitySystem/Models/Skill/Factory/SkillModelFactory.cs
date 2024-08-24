using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public static class SkillModelFactory
    {
        #region Class Methods

        public static SkillModel GetSkillModel(SkillType skillType, SkillData skillData)
        {
            switch (skillType)
            {
                case SkillType.RushAttack:
                    return new RushAttackSkillModel(skillData);

                case SkillType.ForwardAttack:
                    return new ForwardAttackSkillModel(skillData);

                case SkillType.SpawnFlail:
                    return new SpawnFlailSkillModel(skillData);

                case SkillType.FireProjectile:
                    return new FireProjectileSkillModel(skillData);

                case SkillType.FireRotatedScaledProjectile:
                    return new FireRotatedScaledProjectileSkillModel(skillData);

                case SkillType.SpawnForwardScaledImpacts:
                    return new SpawnForwardScaledImpactsSkillModel(skillData);

                case SkillType.SpawnRayHit:
                    return new SpawnRayHitSkillModel(skillData);

                case SkillType.Summon:
                    return new SummonSkillModel(skillData);

                case SkillType.SummonRayPillarsToFire:
                    return new SummonRayPillarsToFireSkillModel(skillData);

                case SkillType.FireRoundBullet:
                    return new FireRoundBulletSkillModel(skillData);

                case SkillType.MoveUnderGround:
                    return new MoveUnderGroundSkillModel(skillData);

                case SkillType.JumpAttack:
                    return new JumpAttackSkillModel(skillData);

                case SkillType.RushAndSlash:
                    return new RushAndSlashSkillModel(skillData);

                case SkillType.ThrowBodyProjectile:
                    return new ThrowBodyProjectileSkillModel(skillData);
            }

            return null;
        }

        #endregion Class Methods
    }
}