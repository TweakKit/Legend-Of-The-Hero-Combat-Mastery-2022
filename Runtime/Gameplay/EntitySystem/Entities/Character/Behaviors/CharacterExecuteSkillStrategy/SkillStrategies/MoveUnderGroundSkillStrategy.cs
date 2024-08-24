namespace Runtime.Gameplay.EntitySystem
{
    public class MoveUnderGroundSkillStrategy : SkillStrategy<MoveUnderGroundSkillModel>
    {
        #region Members

        #endregion Members

        #region Class Methods

        protected override void InitActions(MoveUnderGroundSkillModel skillModel)
        {
            var checkTarget = new CheckTargetSkillAction();
            var precastAction = new PrecastSkillAction();
            var moveUnderGroundAction = new MoveUnderGroundSkillAction();
            var upAttackAction = new UpAttackSkillAction();

            skillActions.Add(checkTarget);
            skillActions.Add(precastAction);
            skillActions.Add(moveUnderGroundAction);
            skillActions.Add(upAttackAction);

            checkTarget.Init(creatorModel,
                             creatorTransform,
                             skillModel.SkillType,
                             skillModel.TargetType,
                             Definition.SkillActionPhase.Precheck);

            precastAction.Init(creatorModel,
                             creatorTransform,
                             skillModel.SkillType,
                             skillModel.TargetType,
                             Definition.SkillActionPhase.Precast);


            moveUnderGroundAction.Init(creatorModel, 
                                        creatorTransform, 
                                        skillModel.SkillType, 
                                        skillModel.TargetType, 
                                        Definition.SkillActionPhase.Cast, 
                                        skillModel.UnderGroundTime, 
                                        skillModel.MoveSpeedScale);

            upAttackAction.Init(creatorModel,
                                creatorTransform,
                                skillModel.SkillType,
                                skillModel.TargetType,
                                Definition.SkillActionPhase.SecondCast,
                                skillModel.UpWarningTime,
                                skillModel.UpWarningPrefabName,
                                skillModel.UpDamageBoxPrefabName,
                                skillModel.UpDamageBoxWidth,
                                skillModel.UpDamageBoxHeight,
                                skillModel.UpDamageBonus,
                                skillModel.UpDamageFactors,
                                skillModel.UpDamageModifierModels);

        }

        #endregion Class Methods
    }
}