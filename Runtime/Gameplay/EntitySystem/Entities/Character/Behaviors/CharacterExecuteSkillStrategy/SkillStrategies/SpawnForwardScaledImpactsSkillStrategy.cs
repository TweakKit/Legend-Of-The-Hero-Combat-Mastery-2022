using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpawnForwardScaledImpactsSkillStrategy : SkillStrategy<SpawnForwardScaledImpactsSkillModel>
    {
        #region Class Methods

        protected override void InitActions(SpawnForwardScaledImpactsSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            PrecastSkillAction precastSkillAction = new PrecastSkillAction();
            SpawnForwardScaledImpactsSkillAction spawnForwardScaledImpactsSkillAction = new SpawnForwardScaledImpactsSkillAction();
            BackswingSkillAction backSwingSkillAction = new BackswingSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(precastSkillAction);
            skillActions.Add(spawnForwardScaledImpactsSkillAction);
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

            spawnForwardScaledImpactsSkillAction.Init(creatorModel,
                                                      creatorTransform,
                                                      skillModel.SkillType,
                                                      skillModel.TargetType,
                                                      SkillActionPhase.Cast,
                                                      skillModel.NumberOfImpacts,
                                                      skillModel.ImpactDamageFactors,
                                                      skillModel.ImpactDamageBonus,
                                                      skillModel.DelayBetweenSpawnImpacts,
                                                      skillModel.DistanceBetweenImpacts,
                                                      skillModel.ImpactScaleMultiplier,
                                                      skillModel.ImpactPrefabName,
                                                      skillModel.ImpactDamageModifierModels);

            backSwingSkillAction.Init(creatorModel,
                                      creatorTransform,
                                      skillModel.SkillType,
                                      skillModel.TargetType,
                                      SkillActionPhase.Backswing);
        }

        #endregion Class Methods
    }
}