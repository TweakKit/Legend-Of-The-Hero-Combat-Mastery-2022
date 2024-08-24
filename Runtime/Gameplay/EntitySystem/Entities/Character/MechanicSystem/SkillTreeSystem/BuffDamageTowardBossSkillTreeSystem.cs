using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class BuffDamageTowardBossSkillTreeSystem : SkillTreeSystem<BuffDamageTowardBossSkillTreeSystemModel>, IDamageModifier
    {
        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageModifier(this);
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (damageInfo.targetModel.EntityType.IsBoss())
                damageInfo.damage = damageInfo.damage * (1 + ownerModel.IncreaseDamagePercent);

            return damageInfo;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier) => prepareDamageModifier;

        #endregion Class Methods
    }
}