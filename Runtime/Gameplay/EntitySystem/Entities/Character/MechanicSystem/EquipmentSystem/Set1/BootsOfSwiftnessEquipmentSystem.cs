using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class BootsOfSwiftnessEquipmentSystem : EquipmentSystem<BootsOfSwiftnessEquipmentSystemModel>
    {
        #region Members

        private const string BUFF_SPEED_EFFECT_NAME = "160001_speed_up";
        private const string BUFF_SPEED_EFFECT_WITH_SCALE_NAME = "160001_speed_up_2";
        private GameObject _buffSpeedEffectWithScale;
        private GameObject _buffSpeedEffect;
        private bool _isBuffedSpeed;
        private float _previousBuffSpeed;
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private Registry<EntitySpawnedMessage> _registryEntitySpawned;
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            _isBuffedSpeed = false;
            _previousBuffSpeed = 0;
            base.Initialize();
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _registryEntitySpawned = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _isBuffedSpeed = false;
            _previousBuffSpeed = 0;
            creatorModel.HealthChangedEvent += OnHealthChanged;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_buffSpeedEffectWithScale)
                PoolManager.Instance.Remove(_buffSpeedEffectWithScale);
            if (_buffSpeedEffect)
                PoolManager.Instance.Remove(_buffSpeedEffect);

            LoadBuffVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
            LoadBuffVFXWithScale(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private void OnEntitySpawned(EntitySpawnedMessage message)
        {
            if (message.IsEnemySpawned)
                CheckBuffStat();
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (characterDiedHandleCompletedMessage.IsEnemyDied)
                CheckBuffStat();
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource) => CheckBuffStat();

        private async UniTaskVoid LoadBuffVFX(Transform parent, CancellationToken token)
        {
            _buffSpeedEffect = await PoolManager.Instance.Get(BUFF_SPEED_EFFECT_NAME, token, false);
            _buffSpeedEffect.transform.SetParent(parent);
            _buffSpeedEffect.transform.localPosition = Vector2.zero;
        }
        private async UniTaskVoid LoadBuffVFXWithScale(Transform parent, CancellationToken token)
        {
            _buffSpeedEffectWithScale = await PoolManager.Instance.Get(BUFF_SPEED_EFFECT_WITH_SCALE_NAME, token, false);
            _buffSpeedEffectWithScale.transform.SetParent(parent);
            _buffSpeedEffectWithScale.transform.localPosition = Vector2.zero;
        }

        public override void Disable()
        {
            base.Disable();
            _registryHeroSpawned.Dispose();
            _registryEntitySpawned.Dispose();
            _characterDiedHandleCompletedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
            if (_buffSpeedEffectWithScale)
                PoolManager.Instance.Remove(_buffSpeedEffectWithScale);
            if (_buffSpeedEffect)
                PoolManager.Instance.Remove(_buffSpeedEffect);
        }

        private void CheckBuffStat()
        {
            var buffSpeedValue = ownerModel.MoveSpeedIncrease;
            var currentHealthPercent = creatorModel.CurrentHp / creatorModel.MaxHp;

            bool buffWithScale = false;
            if (currentHealthPercent <= ownerModel.HealthThresholdTrigger)
            {
                buffWithScale = true;
                buffSpeedValue *= ownerModel.MoveSpeedIncreaseScale;
            }

            if (ownerModel.NumberOfEnemyTrigger != 0 && EntitiesManager.Instance.EnemyModels.Count >= ownerModel.NumberOfEnemyTrigger)
            {
                if (!_isBuffedSpeed)
                {
                    _isBuffedSpeed = true;
                    _previousBuffSpeed = buffSpeedValue;
                    creatorModel.BuffStat(StatType.MoveSpeed, buffSpeedValue, StatModifyType.BaseBonus);
                    if (buffWithScale)
                    {
                        _buffSpeedEffect?.SetActive(false);
                        _buffSpeedEffectWithScale?.SetActive(true);
                        creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
                    }
                    else
                    {
                        _buffSpeedEffect?.SetActive(true);
                        _buffSpeedEffectWithScale?.SetActive(false);
                        creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffBigSpeed);
                    }
                }
                else
                {
                    if (_previousBuffSpeed != buffSpeedValue)
                    {
                        creatorModel.DebuffStat(StatType.MoveSpeed, _previousBuffSpeed, StatModifyType.BaseBonus);
                        creatorModel.BuffStat(StatType.MoveSpeed, buffSpeedValue, StatModifyType.BaseBonus);
                        _previousBuffSpeed = buffSpeedValue;

                        if (buffWithScale)
                        {
                            _buffSpeedEffect?.SetActive(false);
                            _buffSpeedEffectWithScale?.SetActive(true);
                            creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffBigSpeed);
                        }
                        else
                        {
                            _buffSpeedEffect?.SetActive(true);
                            _buffSpeedEffectWithScale?.SetActive(false);
                            creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
                        }
                    }
                }
            }
            else
            {
                if (_isBuffedSpeed)
                {
                    _isBuffedSpeed = false;
                    creatorModel.DebuffStat(StatType.MoveSpeed, _previousBuffSpeed, StatModifyType.BaseBonus);
                    _buffSpeedEffect?.SetActive(false);
                    _buffSpeedEffectWithScale?.SetActive(false);
                }
            }
        }

        #endregion Class Methods
    }
}