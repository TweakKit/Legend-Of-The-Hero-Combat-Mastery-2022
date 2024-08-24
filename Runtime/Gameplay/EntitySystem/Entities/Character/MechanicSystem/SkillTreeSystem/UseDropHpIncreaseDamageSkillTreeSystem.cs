using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Runtime.Gameplay.EntitySystem
{
    public class UseDropHpIncreaseDamageSkillTreeSystem : SkillTreeSystem<UseDropHpIncreaseDamageSkillTreeSystemModel>, IUpdateHealthModifier, IDamageModifier
    {
        #region Members

        private bool _isBuffedDamage;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isBuffedDamage = false;
            creatorModel.AddUpdateHealthModifier(this);
            creatorModel.AddDamageModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _isBuffedDamage = false;
            _cancellationTokenSource?.Cancel();
            creatorModel.AddUpdateHealthModifier(this);
            creatorModel.AddDamageModifier(this);
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty)
        {
            if(damageSource == DamageSource.FromDroppable)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                StartBuffCountAsync().Forget();
            }

            return (value, damageProperty);
        }

        private async UniTaskVoid StartBuffCountAsync()
        {
            _isBuffedDamage = true;
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.TimeIncreaseDamage), cancellationToken: _cancellationTokenSource.Token);
            _isBuffedDamage = false;
        }

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel) => value;

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
            => prepareDamageModifier;

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if(_isBuffedDamage)
                damageInfo.damage = damageInfo.damage * (1 + ownerModel.IncreaseDamagePercent);
            return damageInfo;
        }

        #endregion Class Methods
    }
}
