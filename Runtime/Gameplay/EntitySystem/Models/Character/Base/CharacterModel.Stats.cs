using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.Gameplay.EntitySystem
{
    public abstract partial class CharacterModel : EntityModel
    {
        #region Members

        protected Dictionary<StatType, EntityStat> statsDictionary;

        #endregion Members

        #region Class Methods

        protected virtual partial void InitStats(CharacterStatsInfo statsInfo)
        {
            statsDictionary = new Dictionary<StatType, EntityStat>();
            foreach (var statType in statsInfo.StatTypes)
            {
                var statTotalValue = statsInfo.GetStatTotalValue(statType);
                statsDictionary.Add(statType, new EntityStat(statTotalValue));
            }
        }

        public bool TryGetStat(StatType statType, out float statValue)
        {
            if (statsDictionary.TryGetValue(statType, out var stat))
            {
                statValue = stat.TotalValue;
                return true;
            }
            else
            {
                statValue = 0.0f;
                return false;
            }
        }

        public float GetTotalStatValue(StatType statType)
        {
            var result = statsDictionary.TryGetValue(statType, out var stat);
            return result ? stat.TotalValue : 0;
        }

        public float GetTotalMultiplyStatValue(StatType statType)
        {
            var result = statsDictionary.TryGetValue(statType, out var stat);
            return result ? stat.TotalMultiply : 0;
        }

        public float GetBaseStatValue(StatType statType)
        {
            var result = statsDictionary.TryGetValue(statType, out var stat);
            return result ? stat.BaseValue : 0;
        }

        public virtual void BuffStat(StatType statType, float value, StatModifyType statModifyType)
        {
            if(statType == StatType.Health)
            {
                BuffMaxHp(value, statModifyType);
                StatChangedEvent.Invoke(statType, MaxHp);
                return;
            }
            else if (statType == StatType.Shield)
            {
                BuffMaxDefense(value, statModifyType);
                StatChangedEvent.Invoke(statType, MaxDefense);
                return;
            }

            if (!statsDictionary.ContainsKey(statType))
            {
                statsDictionary.Add(statType, new EntityStat(0));
            }

            var buffedStat = statsDictionary[statType];
            buffedStat.BuffValue(value, statModifyType);
            statsDictionary[statType] = buffedStat;
            StatChangedEvent.Invoke(statType, statsDictionary[statType].TotalValue);
        }

        public virtual void DebuffStat(StatType statType, float value, StatModifyType statModifyType)
        {
            if (statType == StatType.Health)
            {
                DebuffMaxHp(value, statModifyType);
                StatChangedEvent.Invoke(statType, MaxHp);
                return;
            }
            else if (statType == StatType.Shield)
            {
                DebuffDefense(value, statModifyType);
                StatChangedEvent.Invoke(statType, MaxDefense);
                return;
            }

            if (!statsDictionary.ContainsKey(statType))
            {
                statsDictionary.Add(statType, new EntityStat(0));
            }

            var debuffedStat = statsDictionary[statType];
            debuffedStat.DebuffValue(value, statModifyType);
            statsDictionary[statType] = debuffedStat;
            StatChangedEvent.Invoke(statType, statsDictionary[statType].TotalValue);
        }

        protected virtual void BuffMaxDefense(float value, StatModifyType statModifyType)
        {
            switch (statModifyType)
            {
                case StatModifyType.BaseBonus:
                    baseBonusDefense += value;
                    break;

                case StatModifyType.BaseMultiply:
                    baseMultiplyDefense += value;
                    break;

                case StatModifyType.TotalBonus:
                    totalBonusDefense += value;
                    break;

                case StatModifyType.TotalMultiply:
                    totalMultiplyDefense += value;
                    break;

                default:
                    break;
            }

            currentDefense = MaxDefense;
        }

        protected virtual void DebuffDefense(float value, StatModifyType statModifyType)
        {
            switch (statModifyType)
            {
                case StatModifyType.BaseBonus:
                    baseBonusDefense -= value;
                    break;

                case StatModifyType.BaseMultiply:
                    baseMultiplyDefense -= value;
                    break;

                case StatModifyType.TotalBonus:
                    totalBonusDefense -= value;
                    break;

                case StatModifyType.TotalMultiply:
                    totalMultiplyDefense -= value;
                    break;

                default:
                    break;
            }

            currentDefense = Mathf.Min(currentDefense, MaxDefense);
        }

        protected virtual void BuffMaxHp(float value, StatModifyType statModifyType)
        {
            switch (statModifyType)
            {
                case StatModifyType.BaseBonus:
                    baseBonusHp += value;
                    break;

                case StatModifyType.BaseMultiply:
                    baseMultiplyHp += value;
                    break;

                case StatModifyType.TotalBonus:
                    totalBonusHp += value;
                    break;

                case StatModifyType.TotalMultiply:
                    totalMultiplyHp += value;
                    break;

                default:
                    break;
            }

            currentHp = MaxHp;
        }

        protected virtual void DebuffMaxHp(float value, StatModifyType statModifyType)
        {
            switch (statModifyType)
            {
                case StatModifyType.BaseBonus:
                    baseBonusHp -= value;
                    break;

                case StatModifyType.BaseMultiply:
                    baseMultiplyHp -= value;
                    break;

                case StatModifyType.TotalBonus:
                    totalBonusHp -= value;
                    break;

                case StatModifyType.TotalMultiply:
                    totalMultiplyHp -= value;
                    break;

                default:
                    break;
            }

            currentHp = Mathf.Min(currentHp, MaxHp);
        }

        public virtual void CreateDamage(float createdDamage, EntityModel entityModel, DamageInfo damageInfo)
        {
            var lifesteal = 0.0f;
            if (statsDictionary.ContainsKey(StatType.LifeSteal))
            {
                var lifestealStat = statsDictionary[StatType.LifeSteal];
                lifesteal = lifestealStat.TotalValue;
            }
            var addedHp = createdDamage * lifesteal;
            BuffHp(addedHp);

#if DEBUGGING
            if (addedHp > 0)
                Debug.Log($"lifesteal_log || owner: {EntityId}/{EntityType} | lifesteal: {lifesteal} | damageCreated: {createdDamage} | finalTake: {currentHp}");
#endif
        }

        public virtual DamageInfo GetDamageInfo(DamageSource damageSource, float damageBonus, DamageFactor[] damageFactors, StatusEffectModel[] damageModifierModels, EntityModel targetModel)
        {
            var critChance = GetTotalStatValue(StatType.CritChance);
            if (damageSource != DamageSource.FromNormalAttack)
                critChance = 0;
            return GetDamageInfoWithCritChance(damageSource, damageBonus, damageFactors, damageModifierModels, targetModel, critChance);
        }

        public DamageInfo GetDamageInfoWithCritChance(DamageSource damageSource, float damageBonus, DamageFactor[] damageFactors, StatusEffectModel[] damageModifierModels, EntityModel targetModel, float critChance)
        {
            var attackDamage = GetTotalStatValue(StatType.AttackDamage);
            var isCrit = Random.Range(0, 1f) < critChance;
            var damageValue = attackDamage;
            if (damageFactors != null && damageFactors.Length > 0)
            {
                float damageConfig = 0;
                foreach (var damageFactor in damageFactors)
                    damageConfig += (GetTotalStatValue(damageFactor.damageFactorStatType) * damageFactor.damageFactorValue);
                damageValue = damageConfig;
            }
            else
            {
                damageValue = 0;
            }
            damageValue += damageBonus;

            float critDamage = 0;
            if (isCrit)
            {
                critDamage = GetTotalStatValue(StatType.CritDamage);
                damageValue = damageValue * (1 + critDamage);
            }

            var armorPenetration = GetTotalStatValue(StatType.ArmorPenetration);
#if DEBUGGING
            Debug.Log($"damage_log|| owner: {EntityId}/{EntityType} | damage source: {damageSource}| attack damage: {attackDamage} | isCrit: {isCrit} | critDamage: {critDamage}" +
                      $" | armorPenet: {armorPenetration} | damageFactor: {(damageFactors != null && damageFactors.Length > 0 ? GetTextFactorLog(damageFactors) : "1")} | damageBonus: {damageBonus}");
#endif

            return new DamageInfo(damageSource, damageValue, armorPenetration, damageModifierModels, this, targetModel, isCrit ? DamageProperty.Crit : DamageProperty.None);
        }

        private string GetTextFactorLog(DamageFactor[] damageFactors)
        {
            var textFactor = "";
            foreach (var damageFactor in damageFactors)
                textFactor += $"{damageFactor.damageFactorStatType} - {damageFactor.damageFactorValue}";
            return textFactor;
        }

        #endregion Class Methods
    }
}