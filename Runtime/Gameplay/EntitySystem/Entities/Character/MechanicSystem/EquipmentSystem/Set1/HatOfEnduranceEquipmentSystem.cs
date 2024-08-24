using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class HatOfEnduranceEquipmentSystem : EquipmentSystem<HatOfEnduranceEquipmentSystemModel>
    {
        #region Members

        private const string FIRST_TRIGGERED_SHIELD_NAME = "120001_shield";
        private const string SECOND_TRIGGERED_SHIELD_NAME = "120001_shield_2";
        private bool _isFirstTriggered;
        private bool _isSecondTriggered;
        private GameObject _firstTriggeredShield;
        private GameObject _secondTriggeredShield;
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isFirstTriggered = false;
            _isSecondTriggered = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _isFirstTriggered = false;
            _isSecondTriggered = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_firstTriggeredShield)
                PoolManager.Instance.Remove(_firstTriggeredShield);
            if(_secondTriggeredShield)
                PoolManager.Instance.Remove(_secondTriggeredShield);

            LoadFirstShieldAsync(message.HeroTransform, _cancellationTokenSource.Token).Forget();
            LoadSecondShieldAsync(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid LoadFirstShieldAsync(Transform parent, CancellationToken token)
        {
            _firstTriggeredShield = await PoolManager.Instance.Get(FIRST_TRIGGERED_SHIELD_NAME, token, false);
            _firstTriggeredShield.transform.SetParent(parent);
            _firstTriggeredShield.transform.localPosition = Vector2.zero;
        }
        private async UniTaskVoid LoadSecondShieldAsync(Transform parent, CancellationToken token)
        {
            _secondTriggeredShield = await PoolManager.Instance.Get(SECOND_TRIGGERED_SHIELD_NAME, token, false);
            _secondTriggeredShield.transform.SetParent(parent);
            _secondTriggeredShield.transform.localPosition = Vector2.zero;
        }

        public override void Disable()
        {
            base.Disable();
            _registryHeroSpawned.Dispose();
            _cancellationTokenSource?.Cancel();
            if (_firstTriggeredShield)
                PoolManager.Instance.Remove(_firstTriggeredShield);
            if (_secondTriggeredShield)
                PoolManager.Instance.Remove(_secondTriggeredShield);
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource) => CheckBuffStat();

        private void CheckBuffStat()
        {
            var currentHealthPercent = creatorModel.CurrentHp / creatorModel.MaxHp;
            if (currentHealthPercent <= ownerModel.FirstHeatlhPercentTriggerThreshold)
            {
                if (!_isFirstTriggered)
                {
                    _isFirstTriggered = true;
                    creatorModel.BuffStat(StatType.DamageReduction, ownerModel.FirstDamageReductionIncreasePercent, StatModifyType.BaseBonus);
                    _firstTriggeredShield.SetActive(true);
                }
            }
            else
            {
                if (_isFirstTriggered)
                {
                    _isFirstTriggered = false;
                    creatorModel.DebuffStat(StatType.DamageReduction, ownerModel.FirstDamageReductionIncreasePercent, StatModifyType.BaseBonus);
                    _firstTriggeredShield.SetActive(false);
                }
            }

            if (currentHealthPercent <= ownerModel.SecondHealthPercentTriggerThreshold)
            {
                if (!_isSecondTriggered)
                {
                    _isSecondTriggered = true;
                    creatorModel.BuffStat(StatType.DamageReduction, ownerModel.SecondDamageReductionIncreasePercent, StatModifyType.BaseBonus);
                    _firstTriggeredShield.SetActive(false);
                    _secondTriggeredShield.SetActive(true);
                }
            }
            else
            {
                if (_isSecondTriggered)
                {
                    _isSecondTriggered = false;
                    creatorModel.DebuffStat(StatType.DamageReduction, ownerModel.SecondDamageReductionIncreasePercent, StatModifyType.BaseBonus);
                    _secondTriggeredShield.SetActive(false);
                    if (_isFirstTriggered)
                        _firstTriggeredShield.SetActive(true);
                }
            }
        }

        #endregion Class Methods
    }
}