using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class ThrowBodyProjectileSkillStrategy : SkillStrategy<ThrowBodyProjectileSkillModel>
    {
        protected override void InitActions(ThrowBodyProjectileSkillModel skillModel)
        {
            var checkTargetSkillAction = new CheckTargetSkillAction();
            var throwBodySkillAction = new ThrowBodyProjectileSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(throwBodySkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                      creatorTransform,
                                      skillModel.SkillType,
                                      skillModel.TargetType,
                                      SkillActionPhase.Precheck);

            var canThrowBody = !string.IsNullOrEmpty(skillModel.BodyProjectileId) && skillModel.BodyFlySpeed > 0 && skillModel.BodyFlyDuration > 0;
            var canThrowHead = !string.IsNullOrEmpty(skillModel.HeadProjectileId) && skillModel.HeadFlySpeed > 0 && skillModel.HeadFlyDuration > 0;

            throwBodySkillAction.Init(creatorModel,
                                      creatorTransform,
                                      skillModel.SkillType,
                                      skillModel.TargetType,
                                      SkillActionPhase.Cast,
                                      skillModel.BodyProjectileId,
                                      skillModel.BodyFlySpeed,
                                      skillModel.BodyFlyDuration,
                                      skillModel.BodyHits,
                                      skillModel.BodyDamageBonus,
                                      skillModel.BodyDamageFactors,
                                      null,
                                      skillModel.DelayBetweenHeadAndBody,
                                      canThrowBody,
                                      // =====
                                      skillModel.HeadProjectileId,
                                      skillModel.HeadFlySpeed,
                                      skillModel.HeadFlyDuration,
                                      skillModel.HeadDamageBonus,
                                      skillModel.HeadDamageFactors,
                                      null,
                                      canThrowHead
                                      );
        }
    }
}