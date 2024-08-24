using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class PuppyGlovesEquipmentSystem : EquipmentSystem<PuppyGlovesEquipmentSystemModel>, IDamageCreatedModifier, IDamageModifier
    {
        #region Members

        private const string BUFF_DAMAGE_EFFECT = "150002_dmg_buff";
        private GameObject _buffDamageEffect;
        private bool _killedByPreviousDamage;
        private bool _triggeredBuffedAction;
        private bool _buffDamage;
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<HeroSpawnedMessage> _registryHeroSpawned;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddDamageModifier(this);
            _killedByPreviousDamage = false;
            _triggeredBuffedAction = false;
            creatorModel.ReactionChangedEvent += OnReactionChanged;
            _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddDamageCreatedModifier(this);
            creatorModel.AddDamageModifier(this);
            _killedByPreviousDamage = false;
            _triggeredBuffedAction = false;
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
            if(_buffDamageEffect)
                PoolManager.Instance.Remove(_buffDamageEffect);
        }

        public float CreateDamage(float damage, EntityModel receiver)
        {
            if (receiver.IsDead)
            {
                if (ownerModel.CanBuffDamage)
                {
                    _buffDamageEffect?.SetActive(true);
                    _killedByPreviousDamage = true;
                    _triggeredBuffedAction = true;
                }
            }

            return damage;
        }

        public DamageInfo PostCalculateDamageInfo(DamageInfo damageInfo, DamageSource damageSource)
        {
            if (_buffDamage && damageInfo.damageSource == DamageSource.FromNormalAttack)
            {
                var originDamge = damageInfo.damage;
                damageInfo.damage *= (1 + ownerModel.NextDamageBuffPercent);
#if DEBUGGING
                Debug.Log($"weapon_puppy_gloves || origin_damage: {originDamge} | outcome_damage: {damageInfo.damage} ");
#endif
            }
            return damageInfo;
        }

        public PrepareDamageModifier PreCalculateDamageInfo(EntityModel targetModel, DamageSource damageSource, PrepareDamageModifier prepareDamageModifier)
        {
            if (_buffDamage && ownerModel.CanApplyDamageCritRate && damageSource == DamageSource.FromNormalAttack)
                prepareDamageModifier.critChance = ownerModel.NextDamageCritRate;
            return prepareDamageModifier;
        }

        private void OnReactionChanged(CharacterReactionType characterReactionType)
        {
            if (characterReactionType == CharacterReactionType.JustNormalAttack)
            {
                if (_killedByPreviousDamage)
                {
                    if (_triggeredBuffedAction)
                    {
                        _triggeredBuffedAction = false;
                        _buffDamage = true;
                    }

                    _buffDamageEffect?.SetActive(false);
                    _killedByPreviousDamage = false;
                }
                else
                {
                    if (!_triggeredBuffedAction)
                    {
                        _buffDamageEffect?.SetActive(false);
                        _buffDamage = false;
                    }
                }
            }
        }

        #endregion Class Methods
    }
}