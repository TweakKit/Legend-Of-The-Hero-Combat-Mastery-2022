using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class ForwardAttackSkillStrategy : SkillStrategy<ForwardAttackSkillModel>
    {
        #region Class Methods

        protected override void InitActions(ForwardAttackSkillModel skillModel)
        {
            CheckOwnerDirectionSkillAction checkOwnerDirectionSkillAction = new CheckOwnerDirectionSkillAction();
            ForwardAttackSkillAction forwardAttackSkillAction = new ForwardAttackSkillAction();

            skillActions.Add(checkOwnerDirectionSkillAction);
            skillActions.Add(forwardAttackSkillAction);

            checkOwnerDirectionSkillAction.Init(creatorModel,
                                                creatorTransform,
                                                skillModel.SkillType,
                                                skillModel.TargetType,
                                                SkillActionPhase.Precheck);

            forwardAttackSkillAction.Init(creatorModel,
                                          creatorTransform,
                                          skillModel.SkillType,
                                          skillModel.TargetType,
                                          SkillActionPhase.Cast,
                                          skillModel.CastRange,
                                          skillModel.DamageFactors,
                                          skillModel.DamageBonus,
                                          skillModel.DisplayAttackEffectTime,
                                          skillModel.AttackDelayTime,
                                          skillModel.AttackEffectPrefabName,
                                          skillModel.AttackAffectWidth,
                                          skillModel.DamageModifierModels);
        }

        #endregion Class Methods
    }
}