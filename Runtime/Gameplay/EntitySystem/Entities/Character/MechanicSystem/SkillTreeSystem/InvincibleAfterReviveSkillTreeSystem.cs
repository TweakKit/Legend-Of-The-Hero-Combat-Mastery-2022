using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class InvincibleAfterReviveSkillTreeSystem : SkillTreeSystem<InvincibleAfterReviveSkillTreeSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string INVINCIBLE_EFFECT_PREFAB = "invincible_effect";
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private bool _isRevived;
        private bool _isInvincible;
        private GameObject _invincibleEffectGameObject;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isRevived = false;
            _isInvincible = false;
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddUpdateHealthModifier(this);
            _isInvincible = false;
        }

        public override void Disable()
        {
            base.Disable();
            _heroSpawnedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            if (!_isRevived)
            {
                _isRevived = true;
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();
                StartCountInvincibleTimeAsync(message.HeroTransform).Forget();
            }
        }

        private async UniTaskVoid StartCountInvincibleTimeAsync(Transform heroTransform)
        {
            _isInvincible = true;
            _invincibleEffectGameObject = await PoolManager.Instance.Get(INVINCIBLE_EFFECT_PREFAB, _cancellationTokenSource.Token);
            _invincibleEffectGameObject.transform.SetParent(heroTransform);
            _invincibleEffectGameObject.transform.localPosition = Vector2.zero;

            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.TimeInvincible), cancellationToken: _cancellationTokenSource.Token);

            PoolManager.Instance.Remove(_invincibleEffectGameObject);
            _isInvincible = false;
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if (_isInvincible)
                return 0;
            else
                return value;
        }

        #endregion Class Methods
    }

}