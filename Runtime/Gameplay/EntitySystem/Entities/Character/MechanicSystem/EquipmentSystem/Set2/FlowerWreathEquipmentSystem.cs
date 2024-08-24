using Runtime.Definition;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class FlowerWreathEquipmentSystem : EquipmentSystem<FlowerWreathEquipmentSystemModel>, IDamageCreatedModifier, IUpdateHealthModifier
    {
        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddUpdateHealthModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddUpdateHealthModifier(this);
        }

        public float CreateDamage(float damage, EntityModel receiver)
        {
            var randomValue = Random.Range(0, 1f);
            if(randomValue < ownerModel.RateConvertDamageToHealth)
            {
                creatorModel.BuffHp(damage * ownerModel.DamageToHealthPercent);
#if DEBUGGING
                Debug.Log($"weapon_flower_wreath_passive2 || target : {receiver.EntityId} | heal: {damage * ownerModel.DamageToHealthPercent} ");
#endif
            }   
            return damage;
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty)
        {
            var randomValue = Random.Range(0, 1f);
            if (randomValue < ownerModel.HealCritPercent)
            {
                damageProperty = DamageProperty.Crit;
                var critDamage = creatorModel.GetTotalStatValue(StatType.CritDamage);
                var originAddedHP = value;
                value *= (1 + critDamage);
#if DEBUGGING
                Debug.Log($"weapon_flower_wreath_passive4 || origin heal: {originAddedHP} | critDamage: {critDamage} | heal: {value}");
#endif
            }

            return (value, damageProperty);
        }

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel) => value;

        #endregion Class Methods
    }
}