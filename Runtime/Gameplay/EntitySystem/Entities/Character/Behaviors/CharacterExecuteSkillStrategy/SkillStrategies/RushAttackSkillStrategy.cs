using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class RushAttackSkillStrategy : SkillStrategy<RushAttackSkillModel>
    {
        #region Class Methods

        protected override void InitActions(RushAttackSkillModel skillModel)
        {
            CheckCastRangeSkillAction checkCastRangeSkillAction = new CheckCastRangeSkillAction();
            PrecastSkillAction precastSkillAction = new PrecastSkillAction();
            RushAttackSkillAction rushAttackSkillAction = new RushAttackSkillAction();
            BackswingSkillAction backSwingSkillAction = new BackswingSkillAction();

            skillActions.Add(checkCastRangeSkillAction);
            skillActions.Add(precastSkillAction);
            skillActions.Add(rushAttackSkillAction);
            skillActions.Add(backSwingSkillAction);

            checkCastRangeSkillAction.Init(creatorModel,
                                           creatorTransform,
                                           skillModel.SkillType,
                                           skillModel.TargetType, 
                                           SkillActionPhase.Precast,
                                           skillModel.RushRange,
                                           default);

            precastSkillAction.Init(creatorModel,
                                    creatorTransform,
                                    skillModel.SkillType,
                                    skillModel.TargetType,
                                    SkillActionPhase.Precast);

            rushAttackSkillAction.Init(creatorModel,
                                       creatorTransform,
                                       skillModel.SkillType,
                                       skillModel.TargetType,
                                       SkillActionPhase.Cast,
                                       skillModel.RushDuration,
                                       skillModel.RushRange,
                                       skillModel.NumberOfRushTimes,
                                       skillModel.StopRushingAfterHitTarget,
                                       skillModel.RushDamageBonus,
                                       skillModel.RushDamageFactors,
                                       skillModel.RushDamageModifierModels);

            backSwingSkillAction.Init(creatorModel,
                                      creatorTransform,
                                      skillModel.SkillType,
                                      skillModel.TargetType,
                                      SkillActionPhase.Backswing);
        }

        #endregion Class Methods
    }
}