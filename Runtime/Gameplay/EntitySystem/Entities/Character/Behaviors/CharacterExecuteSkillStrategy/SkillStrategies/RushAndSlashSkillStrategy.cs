using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class RushAndSlashSkillStrategy : SkillStrategy<RushAndSlashSkillModel>
    {
        protected override void InitActions(RushAndSlashSkillModel skillModel)
        {
            var checkTargetSkillAction = new CheckTargetSkillAction();
            var rushAttackSkillAction = new RushAttackSkillAction();
            var spawnDamageBoxSkillAction = new SpawnDamageBoxSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(rushAttackSkillAction);
            skillActions.Add(spawnDamageBoxSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            rushAttackSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Cast,
                                        skillModel.RushDuration,
                                        skillModel.RushRange,
                                        skillModel.NumberOfRushTime,
                                        skillModel.StopRushingAfterhitTarget,
                                        skillModel.RushDamageBonus,
                                        skillModel.RushDamageFactors,
                                        null);

            spawnDamageBoxSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.SecondCast,
                                        skillModel.SlashVfx,
                                        skillModel.SlashWidth,
                                        skillModel.SlashHeight,
                                        skillModel.SlashDamageBonus,
                                        skillModel.SlashDamageFactors,
                                        null);
        }
    }
}