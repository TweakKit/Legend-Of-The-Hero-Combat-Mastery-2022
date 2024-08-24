using System;
using System.Threading;
using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.Manager;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class SpikeVestEquipmentSystem : EquipmentSystem<SpikeVestEquipmentSystemModel>, IUpdateHealthModifier
    {
        #region Members

        private const string PROTECTIVE_CIRCLE_EFFECT_PREFAB_NAME = "equipment_130004_protective_circle_effect";
        private GameObject _protectiveCircleEffect;
        private bool _isProtectiveCircleActivated;
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
            if (ownerModel.CanCreateProtectiveCircle)
            {
                _isProtectiveCircleActivated = false;
                _registryHeroSpawned = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
                creatorModel.AddUpdateHealthModifier(this);
            }
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            creatorModel.AddUpdateHealthModifier(this);
        }

        public override void Disable()
        {
            base.Disable();
            if (ownerModel.CanCreateProtectiveCircle)
            {
                _registryHeroSpawned.Dispose();
                _cancellationTokenSource?.Cancel();
                if (_protectiveCircleEffect)
                    PoolManager.Instance.Remove(_protectiveCircleEffect);
            }
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            if (_protectiveCircleEffect)
                PoolManager.Instance.Remove(_protectiveCircleEffect);
            CreateProtectiveCircleEffectAsync(message.HeroTransform).Forget();
        }

        private async UniTaskVoid CreateProtectiveCircleEffectAsync(Transform parent)
        {
            _protectiveCircleEffect = await PoolManager.Instance.Get(PROTECTIVE_CIRCLE_EFFECT_PREFAB_NAME, _cancellationTokenSource.Token, false);
            _protectiveCircleEffect.transform.SetParent(parent);
            _protectiveCircleEffect.transform.localPosition = Vector2.zero;
            StartOperationOnProtectiveCircleAsync().Forget();
        }

        private async UniTaskVoid StartOperationOnProtectiveCircleAsync()
        {
            while (true)
            {
                RunActivateProtectiveCircleAsync(_cancellationTokenSource.Token).Forget();
                await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.DelayToCreateProtectiveCircle), cancellationToken: _cancellationTokenSource.Token);
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
            }
        }

        private async UniTaskVoid RunActivateProtectiveCircleAsync(CancellationToken cancellationToken)
        {
            _isProtectiveCircleActivated = true;
            _protectiveCircleEffect.SetActive(true);
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.ProtectiveCircleExistTime), cancellationToken: cancellationToken);
            _isProtectiveCircleActivated = false;
            _protectiveCircleEffect.SetActive(false);
        }

        public (float, DamageProperty) ModifyBuffHp(float value, DamageSource damageSource, DamageProperty damageProperty)
            => (value, damageProperty);

        public float ModifyDebuffHp(float value, DamageSource damageSource, DamageProperty damageProperty, EntityModel damageCreatorModel)
        {
            if (_isProtectiveCircleActivated)
            {
                if (damageCreatorModel.EntityType.IsEnemy())
                {
                    var characters = EntitiesManager.Instance.GetEntitiesOfType<Character>();
                    foreach (var character in characters)
                    {
                        if (character.EntityUId == damageCreatorModel.EntityUId)
                        {
                            var interactable = character.GetBehavior<IInteractable>(false);
                            var affectedStatusEffectModels = ownerModel.ProtectiveCircleCounterModifierIdentities;
                            var affectedStatusEffectInfo = new AffectedStatusEffectInfo(affectedStatusEffectModels, null, creatorModel);
                            var affectedStatusEffectDirection = (interactable.Model.Position - creatorModel.Position).normalized;
                            interactable.GetAffected(affectedStatusEffectInfo, new StatusEffectMetaData(affectedStatusEffectDirection, creatorModel.Position));
                            break;
                        }
                    }
                }
            }
            return value;
        }

        #endregion Class Methods
    }
}