using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWhenReceiveDamageSkillTreeSystem : SkillTreeSystem<InvincibleWhenReceiveDamageSkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string INVINCIBLE_EFFECT_PREFAB = "invincible_effect";
        private bool _isInvincible;
        private bool _isCooldown;
        private Transform _heroTransform;
        private GameObject _invincibleEffectGameObject;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private CancellationTokenSource _cooldownCancellationTokenSource;
        private CancellationTokenSource _countTimeInvincibleCancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -2;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            _isCooldown = false;
            _isInvincible = false;
            base.Initialize();
            creatorModel.AddUpdateHealthModifier(this);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            _isCooldown = false;
            _isInvincible = false;
            base.Reset(heroModel);

            _countTimeInvincibleCancellationTokenSource?.Cancel();
            if (_invincibleEffectGameObject)
                PoolManager.Instance.Remove(_invincibleEffectGameObject);

            creatorModel.AddUpdateHealthModifier(this);
        }

        public override void Disable()
        {
            base.Disable();
            _cooldownCancellationTokenSource?.Cancel();
            _countTimeInvincibleCancellationTokenSource?.Cancel();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message) => _heroTransform = message.HeroTransform;

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel creatorModel)
        {
            if(value > 0)
            {
                if (_isInvincible)
                    return 0;

                if (!_isCooldown)
                {
                    _countTimeInvincibleCancellationTokenSource?.Cancel();
                    _countTimeInvincibleCancellationTokenSource = new CancellationTokenSource();
                    StartCountTimeInvincibleAsync().Forget();

                    _cooldownCancellationTokenSource?.Cancel();
                    _cooldownCancellationTokenSource = new CancellationTokenSource();
                    StartCooldownAsync().Forget();
                }
            }

            return value;
        }

        private async UniTaskVoid StartCountTimeInvincibleAsync()
        {
            if (!_isInvincible)
            {
                _isInvincible = true;
                _invincibleEffectGameObject = await PoolManager.Instance.Get(INVINCIBLE_EFFECT_PREFAB, _countTimeInvincibleCancellationTokenSource.Token);
                _invincibleEffectGameObject.transform.SetParent(_heroTransform);
                _invincibleEffectGameObject.transform.localPosition = Vector2.zero;
            }

            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.TimeInvincible), cancellationToken: _countTimeInvincibleCancellationTokenSource.Token);
            PoolManager.Instance.Remove(_invincibleEffectGameObject);
            _isInvincible = false;
        }

        private async UniTaskVoid StartCooldownAsync()
        {
            _isCooldown = true;
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.Cooldown * (1 - creatorModel.GetTotalStatValue(StatType.CooldownReduction))), cancellationToken: _cooldownCancellationTokenSource.Token);
            _isCooldown = false;
        }

        #endregion Class Methods
    }
}
