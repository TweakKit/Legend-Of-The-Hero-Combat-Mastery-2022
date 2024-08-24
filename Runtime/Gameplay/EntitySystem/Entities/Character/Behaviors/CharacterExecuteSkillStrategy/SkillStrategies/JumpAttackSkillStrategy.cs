using System.Collections;
using System.Collections.Generic;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class JumpAttackSkillStrategy : SkillStrategy<JumpAttackSkillModel>
    {
        #region Class Methods

        protected override void InitActions(JumpAttackSkillModel skillModel)
        {
            var checkTargetSkillAction = new CheckTargetSkillAction();
            var jumpUpSkillAction = new JumpUpSkillAction();
            var jumpDownSkillAction = new JumpDownSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(jumpUpSkillAction);
            skillActions.Add(jumpDownSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                       creatorTransform,
                                       skillModel.SkillType,
                                       skillModel.TargetType,
                                       SkillActionPhase.Precheck);

            jumpUpSkillAction.Init(creatorModel,
                                    creatorTransform,
                                    skillModel.SkillType,
                                    skillModel.TargetType,
                                    SkillActionPhase.Cast,
                                    skillModel.JumpHeight,
                                    skillModel.JumpUpDuration);

            jumpDownSkillAction.Init(creatorModel,
                                    creatorTransform,
                                    skillModel.SkillType,
                                    skillModel.TargetType,
                                    SkillActionPhase.SecondCast, 
                                    skillModel.WarninigVfx,
                                    skillModel.DisplayWarningTime,
                                    skillModel.JumpMiddleDuration,
                                    skillModel.JumpHeight,
                                    skillModel.JumpDownDuration,
                                    skillModel.JumpDamageWidth,
                                    skillModel.JumpDamageHeight,
                                    skillModel.JumpDamageBoxVfx,
                                    skillModel.JumpDamageBonus,
                                    skillModel.JumpDamageFactors);
        }

        #endregion Class Methods
    }
}