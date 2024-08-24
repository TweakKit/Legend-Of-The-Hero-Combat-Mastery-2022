using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvisibleGlovesEquipmentSystem : EquipmentSystem<InvisibleGlovesEquipmentSystemModel>, IDamageModifier
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

        public override void Disable()
        {
            base.Disable();
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (ownerModel.CanIncreaseDamageTowardEnemy)
            {
                if (damageInfo.targetModel.EntityType.IsZombie())
                {
                    var zombieModel = damageInfo.targetModel as CharacterModel;
                    var currentHealthPercent = zombieModel.CurrentHp / zombieModel.MaxHp;
                    if (currentHealthPercent <= ownerModel.TriggeredInstantKillHealthPercent)
                    {
                        var random = Random.Range(0, 1f);
                        if(random < ownerModel.InstantKillRate)
                        {
                            damageInfo.damage = zombieModel.MaxHp;
                            damageInfo.damageProperty = DamageProperty.InstantKill;
                            return damageInfo;
                        }
                    }
                }

                if (damageInfo.targetModel.EntityType.IsEnemy())
                {
                    var zombieModel = damageInfo.targetModel as CharacterModel;
                    var currentHealthPercent = zombieModel.CurrentHp / zombieModel.MaxHp;
                    if (currentHealthPercent >= ownerModel.TriggeredIncreaseDamageHealthPercent)
                    {
                        damageInfo.damage = damageInfo.damage * (1 + ownerModel.IncreasedDamagePercentToEnemy);
                    }
                }
            }

            return damageInfo;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier) => prepareDamageModifier;

        #endregion Class Methods
    }
}