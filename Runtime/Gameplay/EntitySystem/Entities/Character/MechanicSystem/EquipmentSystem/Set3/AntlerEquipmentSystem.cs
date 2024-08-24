using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class AntlerEquipmentSystem : EquipmentSystem<AntlerEquipmentSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string REGEN_VFX = "regen_vfx";
        private bool _isTriggeredRegen;
        private bool _isTriggeredScaleHeal;

        private GameObject _regenEffect;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private CancellationTokenSource _playRegenCancellationTokenSource;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => 0;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isTriggeredRegen = false;
            _isTriggeredScaleHeal = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
            creatorModel.AddUpdateHealthModifier(this);
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _isTriggeredRegen = false;
            _isTriggeredScaleHeal = false;
            creatorModel.AddUpdateHealthModifier(this);
        }


        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
            _playRegenCancellationTokenSource?.Cancel();
            _registryHeroSpawned.Dispose();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_regenEffect)
                PoolManager.Instance.Remove(_regenEffect);

            LoadRegenVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid LoadRegenVFX(Transform parent, CancellationToken token)
        {
            _regenEffect = await PoolManager.Instance.Get(REGEN_VFX, token, false);
            _regenEffect.transform.SetParent(parent);
            _regenEffect.transform.localPosition = Vector2.zero;
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            var currentHealthPercent = creatorModel.CurrentHp / creatorModel.MaxHp;

            if(currentHealthPercent <= ownerModel.TriggeredRegenHealthPercent)
            {
                if (!_isTriggeredRegen)
                {
                    _isTriggeredRegen = true;
                    _regenEffect?.SetActive(true);
                    _playRegenCancellationTokenSource = new CancellationTokenSource();
                    StartRegenAsync().Forget();
                }
            }
            else
            {
                if (_isTriggeredRegen)
                {
                    _isTriggeredRegen = false;
                    _regenEffect?.SetActive(false);
                    _playRegenCancellationTokenSource?.Cancel();
                }
            }

            if(currentHealthPercent <= ownerModel.TriggeredScaleHealHealthPercent)
            {
                if (!_isTriggeredScaleHeal)
                    _isTriggeredScaleHeal = true;
            }
            else
            {
                if (_isTriggeredScaleHeal)
                    _isTriggeredScaleHeal = false;
            }
        }

        private async UniTaskVoid StartRegenAsync()
        {
            while (true)
            {
                creatorModel.BuffHp(ownerModel.RegenHealthPercentPerSecond * creatorModel.MaxHp);
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: _playRegenCancellationTokenSource.Token);
            }
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty)
        {
            if (_isTriggeredScaleHeal)
                value *= ownerModel.ScaleHealEffectValue;

            return (value, damageProperty);
        }

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel) => value;

        #endregion Class Methods
    }
}
