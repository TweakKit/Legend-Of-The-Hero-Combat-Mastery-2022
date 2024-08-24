using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class FluffyBootsEquipmentSystem : EquipmentSystem<FluffyBootsEquipmentSystemModel>, IDamageModifier
    {
        #region Members

        private const string BUFF_DAMAGE_VFX = "160003_dmg_buff";
        private bool _isBuffedIncreaseDamage;
        private GameObject _buffVFX;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _buffSpeedCancellationTokenSource;
        private CancellationTokenSource _triggerIncreaseDamageCancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => 1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageModifier(this);
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _buffSpeedCancellationTokenSource?.Cancel();
            _isBuffedIncreaseDamage = false;

            if (ownerModel.CanBuffDamage)
            {
                _triggerIncreaseDamageCancellationTokenSource?.Cancel();
                _triggerIncreaseDamageCancellationTokenSource = new CancellationTokenSource();
                StartCountTimeTriggerDamageAsync().Forget();
            }

            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageModifier(this);
            _isBuffedIncreaseDamage = false;
            _buffSpeedCancellationTokenSource?.Cancel();
            _triggerIncreaseDamageCancellationTokenSource?.Cancel();
            creatorModel.HealthChangedEvent += OnHealthChanged;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_buffVFX)
                PoolManager.Instance.Remove(_buffVFX);

            LoadVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid LoadVFX(Transform parent, CancellationToken token)
        {
            _buffVFX = await PoolManager.Instance.Get(BUFF_DAMAGE_VFX, token, false);
            _buffVFX.transform.SetParent(parent);
            _buffVFX.transform.localPosition = Vector2.zero;

            if (_isBuffedIncreaseDamage)
                _buffVFX.SetActive(true);
        }

        public override void Disable()
        {
            base.Disable();
            _registryHeroSpawned.Dispose();
            _cancellationTokenSource?.Cancel();
            _triggerIncreaseDamageCancellationTokenSource?.Cancel();
            _buffSpeedCancellationTokenSource?.Cancel();
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (ownerModel.CanBuffDamage)
            {
                if (value < 0)
                {
                    if (_isBuffedIncreaseDamage)
                    {
                        _isBuffedIncreaseDamage = false;
                        // End buff damge and Start buff speed
                        _buffVFX?.SetActive(false);
                        if (ownerModel.CanBuffSpeed)
                        {
                            _buffSpeedCancellationTokenSource = new CancellationTokenSource();
                            StartBuffSpeedAsync().Forget();
                        }
                    }

                    _triggerIncreaseDamageCancellationTokenSource?.Cancel();
                    _triggerIncreaseDamageCancellationTokenSource = new CancellationTokenSource();
                    StartCountTimeTriggerDamageAsync().Forget();
                }
            }
        }

        private async UniTaskVoid StartCountTimeTriggerDamageAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.CountTimeTriggerIncreaseDamage), cancellationToken: _triggerIncreaseDamageCancellationTokenSource.Token);
            if (!_isBuffedIncreaseDamage)
            {
                _isBuffedIncreaseDamage = true;
                _buffVFX?.SetActive(true);
            }
        }

        private async UniTaskVoid StartBuffSpeedAsync()
        {
            creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
            creatorModel.BuffStat(Definition.StatType.MoveSpeed, ownerModel.IncreaseMoveSpeedValue, Definition.StatModifyType.BaseBonus);
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.LifeTimeIncreasedMoveSpeed), cancellationToken: _buffSpeedCancellationTokenSource.Token);
            creatorModel.DebuffStat(Definition.StatType.MoveSpeed, ownerModel.IncreaseMoveSpeedValue, Definition.StatModifyType.BaseBonus);
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
            => prepareDamageModifier;

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (_isBuffedIncreaseDamage)
                damageInfo.damage = damageInfo.damage * (1 + ownerModel.IncreasedDamagePercent);

            return damageInfo;
        }

        #endregion Class Methods
    }
}