using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class TwinNecklaceEquipmentSystem : EquipmentSystem<TwinNecklaceEquipmentSystemModel>, IDamageModifier
    {
        #region Members

        private const string BUFF_DAMAGE_VFX = "140003_dmg_buff";
        private bool _isBuffedIncreaseDamage;
        private bool _isBuffedMoveSpeed;
        private GameObject _buffVFX;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageModifier(this);
            if(ownerModel.CanBuffDamage)
                _isBuffedIncreaseDamage = true;
            else
                _isBuffedIncreaseDamage = false;
            _isBuffedMoveSpeed = false;
            creatorModel.HealthChangedEvent += OnHealthChanged;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageModifier(this);
            if (ownerModel.CanBuffDamage)
                _isBuffedIncreaseDamage = true;
            else
                _isBuffedIncreaseDamage = false;
            _isBuffedMoveSpeed = false;
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
            _cancellationTokenSource?.Cancel();
            _registryHeroSpawned.Dispose();
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (ownerModel.CanBuffDamage)
            {
                var currentHealthPercent = creatorModel.CurrentHp / creatorModel.MaxHp;

                if (currentHealthPercent <= ownerModel.TriggeredHealthPercent)
                {
                    if (_isBuffedIncreaseDamage)
                    {
                        _isBuffedIncreaseDamage = false;
                        _buffVFX?.SetActive(false);
                    }

                    if (ownerModel.CanBuffSpeed && !_isBuffedMoveSpeed)
                    {
                        _isBuffedMoveSpeed = true;
                        creatorModel.ReactionChangedEvent.Invoke(CharacterReactionType.JustBuffSpeed);
                        creatorModel.BuffStat(StatType.MoveSpeed, ownerModel.IncreaseMoveSpeed, StatModifyType.BaseBonus);
                    }
                }
                else
                {
                    if (!_isBuffedIncreaseDamage)
                    {
                        _isBuffedIncreaseDamage = true;
                        _buffVFX?.SetActive(true);
                    }

                    if (ownerModel.CanBuffSpeed && _isBuffedMoveSpeed)
                    {
                        _isBuffedMoveSpeed = false;
                        creatorModel.DebuffStat(StatType.MoveSpeed, ownerModel.IncreaseMoveSpeed, StatModifyType.BaseBonus);
                    }
                }
            }
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier) => prepareDamageModifier;

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (_isBuffedIncreaseDamage)
                damageInfo.damage = damageInfo.damage * (1 + ownerModel.IncreaseDamagePercent);


            return damageInfo;
        }

        #endregion Class Methods
    }
}