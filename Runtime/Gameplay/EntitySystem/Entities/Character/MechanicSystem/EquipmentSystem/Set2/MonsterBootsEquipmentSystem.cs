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
    public class MonsterBootsEquipmentSystem : EquipmentSystem<MonsterBootsEquipmentSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string BUFF_SPEED_EFFECT_NAME = "160001_speed_up";
        private bool _isBuffed;
        private GameObject _buffSpeedEffect;
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;

        #endregion Members

        #region Properties

        public int UpdateHealthPriority => -1;

        #endregion Properties

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddUpdateHealthModifier(this);
            _isBuffed = false;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddUpdateHealthModifier(this);
            _isBuffed = false;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_buffSpeedEffect)
                PoolManager.Instance.Remove(_buffSpeedEffect);

            LoadBuffVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid LoadBuffVFX(Transform parent, CancellationToken token)
        {
            _buffSpeedEffect = await PoolManager.Instance.Get(BUFF_SPEED_EFFECT_NAME, token, false);
            _buffSpeedEffect.transform.SetParent(parent);
            _buffSpeedEffect.transform.localPosition = Vector2.zero;
        }

        public override void Disable()
        {
            base.Disable();
            _registryHeroSpawned.Dispose();
            _cancellationTokenSource?.Cancel();
            if (_buffSpeedEffect)
                PoolManager.Instance.Remove(_buffSpeedEffect);
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty) => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if (ownerModel.CanBuff)
            {
                if (!_isBuffed)
                    StartBuffAsync().Forget();
            }
            return value;
        }

        private async UniTaskVoid StartBuffAsync()
        {
            _isBuffed = true;
            creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
            creatorModel.BuffStat(StatType.MoveSpeed, ownerModel.BuffSpeedValue, StatModifyType.BaseBonus);
            _buffSpeedEffect?.SetActive(true);
            if (ownerModel.CanBuffDodgeChance)
                creatorModel.BuffStat(StatType.DodgeChance, ownerModel.BuffDodgeChance, StatModifyType.BaseBonus);

            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.BuffSpeedDuration), cancellationToken: _cancellationTokenSource.Token);

            _isBuffed = false;
            creatorModel.DebuffStat(StatType.MoveSpeed, ownerModel.BuffSpeedValue, StatModifyType.BaseBonus);
            _buffSpeedEffect?.SetActive(false);
            if (ownerModel.CanBuffDodgeChance)
                creatorModel.DebuffStat(StatType.DodgeChance, ownerModel.BuffDodgeChance, StatModifyType.BaseBonus);
        }

        #endregion Class Methods
    }
}