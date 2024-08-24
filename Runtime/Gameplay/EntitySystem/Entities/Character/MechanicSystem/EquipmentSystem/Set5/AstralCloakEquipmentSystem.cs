using System.Threading;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralCloakEquipmentSystem : EquipmentSystem<AstralCloakEquipmentSystemModel>
    {
        #region Members

        private const string RECOVER_EFFECT_PREFAB_NAME = "equipment_130005_recover_effect";
        private bool _isRecovered;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isRecovered = false;
            _cancellationTokenSource = new CancellationTokenSource();
            creatorModel.HealthChangedEvent += OnHealthChanged;
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            if (_isRecovered && ownerModel.CanBuffWhenRecovered)
            {
                creatorModel.BuffStat(StatType.AttackDamage, ownerModel.AttackIncreasePercent, StatModifyType.BaseMultiply);
                creatorModel.BuffStat(StatType.AttackSpeed, ownerModel.AttackSpeedIncreasePercent, StatModifyType.BaseMultiply);
            }
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (creatorModel.IsDead && !_isRecovered && ownerModel.CanRecoverHealth)
            {
                _isRecovered = true;
                var maxHealth = creatorModel.MaxHp;
                CreateRecoverEffectAsync(creatorModel.Position).Forget();
                creatorModel.BuffHp(maxHealth * ownerModel.RecoverHealthPercent);

                if (ownerModel.CanBuffWhenRecovered)
                {
                    creatorModel.BuffStat(StatType.AttackDamage, ownerModel.AttackIncreasePercent, StatModifyType.BaseMultiply);
                    creatorModel.BuffStat(StatType.AttackSpeed, ownerModel.AttackSpeedIncreasePercent, StatModifyType.BaseMultiply);
                }
            }
        }

        private async UniTask CreateRecoverEffectAsync(Vector2 spawnPosition)
        {
            var recoverEffect = await PoolManager.Instance.Get(RECOVER_EFFECT_PREFAB_NAME, cancellationToken: _cancellationTokenSource.Token);
            recoverEffect.transform.position = spawnPosition;
        }

        #endregion Class Methods
    }
}