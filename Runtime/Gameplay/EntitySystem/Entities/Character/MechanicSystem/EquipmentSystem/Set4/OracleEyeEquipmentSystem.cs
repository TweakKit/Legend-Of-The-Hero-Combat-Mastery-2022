using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class OracleEyeEquipmentSystem : EquipmentSystem<OracleEyeEquipmentSystemModel>, IDamageCreatedModifier, IDamageModifier
    {
        #region Properties

        private const string BUFF_DAMAGE_EFFECT = "140004_dmg_buff";
        private GameObject _buffDamageEffect;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _buffDamageCancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private bool _isBuffDamage;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _isBuffDamage = false;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddDamageModifier(this);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _buffDamageCancellationTokenSource?.Cancel();
            _isBuffDamage = false;
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddDamageModifier(this);
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_buffDamageEffect)
                PoolManager.Instance.Remove(_buffDamageEffect);

            LoadBuffVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid LoadBuffVFX(Transform parent, CancellationToken token)
        {
            _buffDamageEffect = await PoolManager.Instance.Get(BUFF_DAMAGE_EFFECT, token, false);
            _buffDamageEffect.transform.SetParent(parent);
            _buffDamageEffect.transform.localPosition = Vector2.zero;
        }

        public override void Disable()
        {
            base.Disable();
            _registryHeroSpawned.Dispose();
            _cancellationTokenSource?.Cancel();
            _buffDamageCancellationTokenSource?.Cancel();
            if (_buffDamageEffect)
                PoolManager.Instance.Remove(_buffDamageEffect);
        }

        public float CreateDamage(float damage, EntityModel receiver)
        {
            if (ownerModel.CanBuffDamage && receiver.IsDead && receiver.EntityType.IsEnemy())
            {
                _buffDamageCancellationTokenSource?.Cancel();
                _buffDamageCancellationTokenSource = new CancellationTokenSource();
                StartCountTimeBuffDamage(_buffDamageCancellationTokenSource.Token).Forget();
            }

            return damage;
        }

        private async UniTaskVoid StartCountTimeBuffDamage(CancellationToken token)
        {
            _isBuffDamage = true;
            _buffDamageEffect?.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.IncreaseDamagePercentDuration), cancellationToken: token);
            _isBuffDamage = false;
            _buffDamageEffect?.SetActive(false);
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (_isBuffDamage)
            {
                var originDamge = damageInfo.damage;
                damageInfo.damage *= (1 + ownerModel.IncreaseDamagePercent);
#if DEBUGGING
                Debug.Log($"weapon_oracle_eye || origin_damage: {originDamge} | outcome_damage: {damageInfo.damage} ");
#endif
            }
            return damageInfo;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier) => prepareDamageModifier;

        #endregion Class Methods
    }
}