using System.Collections.Generic;
using Runtime.Definition;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public partial class HeroModel
    {
        #region Members

        private List<IDamageModifier> _damageModifiers;
        private List<IDamageCreatedModifier> _damageCreatedModifiers;

        #endregion Members

        #region Class Methods

        public void AddDamageModifier(IDamageModifier damageModifier)
        {
            if (_damageModifiers == null)
                _damageModifiers = new();

            _damageModifiers.Add(damageModifier);
        }
        public void AddDamageCreatedModifier(IDamageCreatedModifier damageCreatedModifier)
        {
            if (_damageCreatedModifiers == null)
                _damageCreatedModifiers = new();
            _damageCreatedModifiers.Add(damageCreatedModifier);
        }

        public override void DebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            base.DebuffHp(value, damageSource, damageProperty, damageCreatorModel);
            Messenger.Publisher().Publish(new HeroGotHitMessage(value, damageSource));
        }

        public override void CreateDamage(float createdDamage, EntityModel entityModel, DamageInfo damageInfo)
        {
            if (_damageCreatedModifiers != null)
            {
                foreach (var item in _damageCreatedModifiers)
                    createdDamage = item.CreateDamage(createdDamage, entityModel);
            }

            base.CreateDamage(createdDamage, entityModel, damageInfo);
        }

        public override DamageInfo GetDamageInfo(DamageSource damageSource, float damageBonus, DamageFactor[] damageFactors, StatusEffectModel[] damageModifierModels, EntityModel targetModel)
        {
            var critChance = damageSource == DamageSource.FromNormalAttack ? GetTotalStatValue(StatType.CritChance) : 0;
            var prepareDamageModifier = new PrepareDamageModifier(damageBonus, damageFactors, critChance, damageModifierModels);
            prepareDamageModifier = PreCalculateDamageInfo(targetModel, damageSource, prepareDamageModifier);
            var damageInfo = GetDamageInfoWithCritChance(damageSource, prepareDamageModifier.damageBonus, prepareDamageModifier.damageFactors, prepareDamageModifier.damageModifierModels, targetModel, prepareDamageModifier.critChance);
            return PostCalculateDamageInfo(damageInfo, damageSource);
        }

        private PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier) 
        {
            if(_damageModifiers != null)
            {
                foreach (var item in _damageModifiers)
                    prepareDamageModifier = item.PreCalculateDamageInfo(targetModel, damageSource, prepareDamageModifier);
            }

            return prepareDamageModifier;
        }

        private DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (_damageModifiers != null)
            {
                foreach (var item in _damageModifiers)
                    damageInfo = item.PostCalculateDamageInfo(damageInfo, damageSource);
            }
            return damageInfo;
        }

        #endregion Class Methods
    }
}
