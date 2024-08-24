using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;

namespace Runtime.Gameplay.EntitySystem
{
    public class LeatherJacketEquipmentSystem : EquipmentSystem<LeatherJacketEquipmentSystemModel>
    {
        #region Members

        private bool _isCooldown;
        private bool _isBuffedLifeSteal;
        private float _previousBuffShieldValue;
        private CancellationTokenSource _runCooldownCancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _previousBuffShieldValue = 0;
            _isCooldown = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
            creatorModel.ShieldChangedEvent += OnShieldChanged;
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _previousBuffShieldValue = 0;
            _isCooldown = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
            creatorModel.ShieldChangedEvent += OnShieldChanged;
        }

        public override void Disable()
        {
            base.Disable();
            _runCooldownCancellationTokenSource?.Cancel();
        }

        private void OnShieldChanged(float value, DamageProperty damageProperty)
        {
            if(creatorModel.CurrentDefense <= 0)
            {
                if(_isBuffedLifeSteal)
                {
                    _isBuffedLifeSteal = false;
                    creatorModel.DebuffStat(StatType.LifeSteal, ownerModel.BuffedLifeStealPercent, StatModifyType.BaseBonus);
                }
            }
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (value < 0)
            {
                if (ownerModel.CanBuffShield && !_isCooldown)
                {
                    creatorModel.DebuffStat(StatType.Shield, _previousBuffShieldValue, StatModifyType.BaseBonus);
                    _previousBuffShieldValue = ownerModel.HealthPercentCreateShield * creatorModel.MaxHp;
                    creatorModel.BuffStat(StatType.Shield, _previousBuffShieldValue, StatModifyType.BaseBonus);
                    if(ownerModel.CanBuffLifeSteal)
                    {
                        if (!_isBuffedLifeSteal)
                        {
                            _isBuffedLifeSteal = true;
                            creatorModel.BuffStat(StatType.LifeSteal, ownerModel.BuffedLifeStealPercent, StatModifyType.BaseBonus);
                        }
                    }

                    _runCooldownCancellationTokenSource = new CancellationTokenSource();
                    StartCooldownAsync().Forget();
                }
            }
        }

        private async UniTaskVoid StartCooldownAsync()
        {
            _isCooldown = true;
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.Cooldown), cancellationToken: _runCooldownCancellationTokenSource.Token);
            _isCooldown = false;
        }

        #endregion Class Methods
    }
}