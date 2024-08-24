using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnFlailSkillStrategy : SkillStrategy<SpawnFlailSkillModel>
    {
        #region Class Methods

        protected override void InitActions(SpawnFlailSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            PrecastSkillAction precastSkillAction = new PrecastSkillAction();
            SpawnFlailSkillAction spawnFlailSkillAction = new SpawnFlailSkillAction();
            BackswingSkillAction backSwingSkillAction = new BackswingSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(precastSkillAction);
            skillActions.Add(spawnFlailSkillAction);
            skillActions.Add(backSwingSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            precastSkillAction.Init(creatorModel,
                                    creatorTransform,
                                    skillModel.SkillType,
                                    skillModel.TargetType,
                                    SkillActionPhase.Precast);

            spawnFlailSkillAction.Init(creatorModel,
                                       creatorTransform,
                                       skillModel.SkillType,
                                       skillModel.TargetType,
                                       SkillActionPhase.Cast,
                                       skillModel.NumberOfFlails,
                                       skillModel.DelayBetweenFlails,
                                       skillModel.FlailPrefabNames,
                                       skillModel.FlailDamageBonus,
                                       skillModel.FlailDamageFactors,
                                       skillModel.FlailDamageModifierIdentities);

            backSwingSkillAction.Init(creatorModel,
                                      creatorTransform,
                                      skillModel.SkillType,
                                      skillModel.TargetType,
                                      SkillActionPhase.Backswing);
        }

        #endregion Class Methods
    }
}