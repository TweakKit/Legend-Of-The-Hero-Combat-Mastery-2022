using System.Collections.Generic;
using UnityEngine;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;
using System.Threading;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class GauntletsOfTheOverlordEquipmentSystem : EquipmentSystem<GauntletsOfTheOverlordEquipmentSystemModel>, IDamageModifier
    {
        #region Members

        private const string EXPLODE_PREFAB = "150001_explode_damage";
        private const string BUFF_DAMAGE_EFFECT = "150001_dmg_buff";
        private GameObject _buffDamageEffect;
        private bool _isBuffedAttack;
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;
        private Registry<EntitySpawnedMessage> _registryEntitySpawned;
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            _isBuffedAttack = false;
            base.Initialize();
            creatorModel.AddDamageModifier(this);
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _registryEntitySpawned = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageModifier(this);
            _isBuffedAttack = false;
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_buffDamageEffect)
                PoolManager.Instance.Remove(_buffDamageEffect);

            LoadBuffVFX(message.HeroTransform, _cancellationTokenSource.Token).Forget();
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

        private void CheckBuffStat()
        {
            if (ownerModel.NumberOfEnemyTriggerIncreaseAttack != 0 && EntitiesManager.Instance.EnemyModels.Count >= ownerModel.NumberOfEnemyTriggerIncreaseAttack)
            {
                if (!_isBuffedAttack)
                {
                    _isBuffedAttack = true;
                    creatorModel.BuffStat(StatType.AttackDamage, ownerModel.AttackIncreasePercent, StatModifyType.BaseMultiply);
                    _buffDamageEffect?.SetActive(true);
                }
            }
            else
            {
                if (_isBuffedAttack)
                {
                    _isBuffedAttack = false;
                    creatorModel.DebuffStat(StatType.AttackDamage, ownerModel.AttackIncreasePercent, StatModifyType.BaseMultiply);
                    _buffDamageEffect?.SetActive(false);
                }
            }
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
            _registryEntitySpawned.Dispose();
            _characterDiedHandleCompletedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
            if (_buffDamageEffect)
                PoolManager.Instance.Remove(_buffDamageEffect);
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource) => damageInfo;

        private async UniTask CreateDamageBoxAsync(EntityModel target, float damageBonus, DamageFactor[] damageFactors, StatusEffectModel[] statusEffectModels, CharacterModel creator, Vector2 spawnPoint)
        {
            var damageBoxGameObject = await PoolManager.Instance.Get(EXPLODE_PREFAB, _cancellationTokenSource.Token);
            var damageBox = damageBoxGameObject.GetComponent<SpriteAnimatorDamageBox>();
            damageBox.Init(creator, DamageSource.FromOther, false, damageBonus, damageFactors, statusEffectModels, avoidEntityUids: new List<uint>() { target.EntityUId });
            damageBox.transform.position = spawnPoint;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
        {
            if (damageSource == DamageSource.FromNormalAttack && ownerModel.NumberOfEnemyTriggerExplodeDamage != 0 &&
                EntitiesManager.Instance.EnemyModels.Count >= ownerModel.NumberOfEnemyTriggerExplodeDamage)
            {
                var damageBonus = prepareDamageModifier.damageBonus * ownerModel.ExplodeDamagePercent;
                var damageFactors = new List<DamageFactor>();
                foreach (var item in prepareDamageModifier.damageFactors)
                {
                    DamageFactor damageFactor = new DamageFactor(item.damageFactorStatType, item.damageFactorValue * ownerModel.ExplodeDamagePercent);
                    damageFactors.Add(damageFactor);
                }

                CreateDamageBoxAsync(targetModel, damageBonus, damageFactors.ToArray(), prepareDamageModifier.damageModifierModels, creatorModel, targetModel.Position).Forget();
            }

            return prepareDamageModifier;
        }

        #endregion Class Methods
    }
}