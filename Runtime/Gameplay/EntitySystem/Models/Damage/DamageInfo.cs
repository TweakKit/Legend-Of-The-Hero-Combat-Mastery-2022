using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class DamageInfo
    {
        #region Members

        public DamageSource damageSource;
        public float damage;
        public float armorPenetration;
        public StatusEffectModel[] damageStatusEffectModels;
        public EntityModel creatorModel;
        public EntityModel targetModel;
        public DamageProperty damageProperty;
        public float critDamage;

        #endregion Members

        #region Class Methods

        public DamageInfo(DamageSource damageSource, float damage, float armorPenetration, 
                          StatusEffectModel[] damageStatusEffectModels,EntityModel creatorModel, 
                          EntityModel targetModel, DamageProperty damageProperty)
        {
            this.damageSource = damageSource;
            this.damage = damage;
            this.damageStatusEffectModels = damageStatusEffectModels;
            this.armorPenetration = armorPenetration;
            this.creatorModel = creatorModel;
            this.targetModel = targetModel;
            this.damageProperty = damageProperty;
        }

        public DamageInfo(DamageSource damageSource, float damage, StatusEffectModel[] damageStatusEffectModels, 
                          EntityModel creatorModel, EntityModel targetModel)
        {
            this.damageSource = damageSource;
            this.damage = damage;
            this.damageStatusEffectModels = damageStatusEffectModels;
            this.creatorModel = creatorModel;
            this.targetModel = targetModel;
            armorPenetration = 0.0f;
        }

        public DamageInfo(DamageSource damageSource, float damage, EntityModel creatorModel, EntityModel targetModel)
        {
            this.damageSource = damageSource;
            this.damage = damage;
            this.creatorModel = creatorModel;
            this.targetModel = targetModel;
            damageStatusEffectModels = null;
            armorPenetration = 0.0f;
        }

        #endregion Class Methods
    }

    public enum DamageSource
    {
        FromNormalAttack,
        FromSpecialAttack,
        FromSkill,
        FromTrap,
        FromOther,
        FromCollide,
        FromDroppable
    }

    public enum DamageProperty
    {
        None,
        Crit,
        Poison,
        InstantKill,
    }

    public static class DamagePropertyExtensions
    {
        public static bool IsCrit(this DamageProperty damageProperty)
        {
            return damageProperty == DamageProperty.Crit;
        }
    }

    public static class DamageFactorExtensions
    {
        public static DamageFactor[] Add(this DamageFactor[] a, DamageFactor[] b)
        {
            if (a == null)
                return b;

            if (b == null)
                return a;

            var listC = new List<DamageFactor>();
            var listB = b.ToList();

            if (a.Length > 0)
            {
                for (int i = 0; i < a.Length; i++)
                {
                    var sameTypeFactor = b.FirstOrDefault(x => x.damageFactorStatType == a[i].damageFactorStatType);
                    var damageFactorValue = a[i].damageFactorValue + sameTypeFactor.damageFactorValue;
                    listB.Remove(sameTypeFactor);
                    listC.Add(new DamageFactor(a[i].damageFactorStatType, damageFactorValue));
                }
            }

            foreach (var factor in listB)
                listC.Add(factor);

            return listC.ToArray();
        }

        public static DamageFactor[] Add(this DamageFactor[] a, float damagePercent)
        {
            if (a == null)
                return a;

            var listC = new List<DamageFactor>();
            for (int i = 0; i < a.Length; i++)
            {
                var damageFactorValue = a[i].damageFactorValue + damagePercent;
                listC.Add(new DamageFactor(a[i].damageFactorStatType, damageFactorValue));
            }

            return listC.ToArray();
        }

        public static DamageFactor[] Multiply(this DamageFactor[] a, float factor)
        {
            if (a == null)
                return a;

            var listC = new List<DamageFactor>();
            for (int i = 0; i < a.Length; i++)
            {
                var damageFactorValue = a[i].damageFactorValue * factor;
                listC.Add(new DamageFactor(a[i].damageFactorStatType, damageFactorValue));
            }

            return listC.ToArray();
        }
    }

    [Serializable]
    public struct DamageFactor
    {
        #region Members

        public StatType damageFactorStatType;
        public float damageFactorValue;

        #endregion Members

        #region Struct Methods

        public DamageFactor(StatType damageFactorStatType, float damageFactorValue)
        {
            this.damageFactorStatType = damageFactorStatType;
            this.damageFactorValue = damageFactorValue;
        }

        #endregion Struct Methods
    }

    /// <summary>
    /// This struct holds data that relates to a damage.<br/>
    /// when a damage occurs, some data needs to be sent in order to execute some other functions such as adding
    /// status effect on targets, doing something rarely needed such as recording those data in server, or whatever else can be.<br/>
    /// The meta data includes:
    ///     + Damage direction: The direction from the damage source to the damaged target.
    ///     + Attracted point: The point indicates the damaged target may move over or do something later.
    /// </summary>
    public struct DamageMetaData
    {
        #region Members

        public Vector2 damageDirection;
        public Vector2 attractedPoint;

        #endregion Members

        #region Struct Methods

        public DamageMetaData(Vector2 damageDirection, Vector2 attractedPoint)
        {
            this.damageDirection = damageDirection;
            this.attractedPoint = attractedPoint;
        }

        #endregion Struct Methods
    }
}