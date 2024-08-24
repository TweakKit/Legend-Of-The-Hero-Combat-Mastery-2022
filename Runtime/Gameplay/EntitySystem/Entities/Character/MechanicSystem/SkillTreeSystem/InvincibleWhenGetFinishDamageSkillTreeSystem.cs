using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleWhenGetFinishDamageSkillTreeSystem : SkillTreeSystem<InvincibleWhenGetFinishDamageSkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string INVINCIBLE_EFFECT_PREFAB = "invincible_effect";
        private bool _isTriggeredInvincible;
        private bool _isInvincible;
        private Transform _heroTransform;
        private GameObject _invincibleEffectGameObject;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => 0;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isTriggeredInvincible = false;
            creatorModel.AddUpdateHealthModifier(this);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Disable()
        {
            base.Disable();
            _heroSpawnedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message) => _heroTransform = message.HeroTransform;

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);
        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if(_isInvincible)
            {
                value = 0;
                return value;
            }

            if (!_isTriggeredInvincible)
            {
                if (value >= creatorModel.CurrentHp)
                {
                    value = creatorModel.CurrentHp - 1;
                    _isTriggeredInvincible = true;
                    _cancellationTokenSource = new CancellationTokenSource();
                    StartCountInvincibleTimeAsync().Forget();
                }
            }

            return value;
        }

        private async UniTaskVoid StartCountInvincibleTimeAsync()
        {
            _isInvincible = true;
            _invincibleEffectGameObject = await PoolManager.Instance.Get(INVINCIBLE_EFFECT_PREFAB, _cancellationTokenSource.Token);
            _invincibleEffectGameObject.transform.SetParent(_heroTransform);
            _invincibleEffectGameObject.transform.localPosition = Vector2.zero;

            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.TimeInvincible), cancellationToken: _cancellationTokenSource.Token);
            PoolManager.Instance.Remove(_invincibleEffectGameObject);
            _isInvincible = false;
        }

        #endregion Class Methods
    }
}
