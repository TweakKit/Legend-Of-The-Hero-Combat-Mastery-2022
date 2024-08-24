using System.Collections;
using System.Collections.Generic;
using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FireRoundBulletSkillStrategy : SkillStrategy<FireRoundBulletSkillModel>
    {
        #region Class Methods

        protected override void InitActions(FireRoundBulletSkillModel skillModel)
        {
            CheckTargetSkillAction checkTargetSkillAction = new CheckTargetSkillAction();
            FireProjectileSkillAction fireProjectileSkillAction = new FireProjectileSkillAction();

            skillActions.Add(checkTargetSkillAction);
            skillActions.Add(fireProjectileSkillAction);

            checkTargetSkillAction.Init(creatorModel,
                                        creatorTransform,
                                        skillModel.SkillType,
                                        skillModel.TargetType,
                                        SkillActionPhase.Precheck);

            FlyRoundProjectileStrategyData flyRoundProjectileStrategyData =  new FlyRoundProjectileStrategyData(
                                                                                DamageSource.FromSkill,
                                                                                skillModel.BulletFlyDuration,
                                                                                skillModel.BulletFlyHeight,
                                                                                skillModel.WarningPrefabName,
                                                                                skillModel.DamageAreaHeight,
                                                                                skillModel.DamageAreaWidth,
                                                                                default,
                                                                                0,
                                                                                null,
                                                                                null
                                                                            );

            SpawnDamageAreaProjectileFinishStrategyData spawnDamageAreaProjectileStrategyData = new SpawnDamageAreaProjectileFinishStrategyData(DamageSource.FromSkill,
                                                                                                                                                skillModel.DamageAreaPrefabName,
                                                                                                                                                skillModel.DamageAreaLifeTime,
                                                                                                                                                skillModel.DamageAreaDamageFactors,
                                                                                                                                                skillModel.DamageAreaWidth,
                                                                                                                                                skillModel.DamageAreaHeight,
                                                                                                                                                skillModel.DamageAreaInterval,
                                                                                                                                                skillModel.DamageAreaDamageModifierModels,
                                                                                                                                                skillModel.FirstInitDamageFactors,
                                                                                                                                                null);

            fireProjectileSkillAction.Init(creatorModel,
                                           creatorTransform,
                                           skillModel.SkillType,
                                           skillModel.TargetType,
                                           SkillActionPhase.Cast,
                                           skillModel.BulletId,
                                           new ProjectileStrategyData[] { flyRoundProjectileStrategyData, spawnDamageAreaProjectileStrategyData },
                                           1,
                                           0,
                                           true,
                                           0);
        }

        #endregion Class Methods
    }
}