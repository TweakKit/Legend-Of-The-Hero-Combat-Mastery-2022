using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralGaunletEquipmentSystem : EquipmentSystem<AstralGauntletEquipmentSystemModel>
    {
        #region Members

        private const string IMPACT_SPECIAL_RANGE_EFFECT = "equipment_150005_range_effect";
        private float _currentCooldown;
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _currentCooldown = 0;
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
            _cancellationTokenSource = new CancellationTokenSource();
            if (ownerModel.CanApplyEffect)
            {
                StartCountTimeAsync().Forget();
            }
        }

        public override void Reset(HeroModel heroModel)
        {
            base.Reset(heroModel);
            _currentCooldown = 0;
        }

        public override void Disable()
        {
            base.Disable();
            _characterDiedHandleCompletedRegistry.Dispose();
            _cancellationTokenSource?.Cancel();
        }

        private async UniTaskVoid StartCountTimeAsync()
        {
            while (true)
            {
                await UniTask.Yield(cancellationToken: _cancellationTokenSource.Token);
                if (_currentCooldown <= 0)
                {
                    if (creatorModel.currentTargetedTarget != null && !creatorModel.currentTargetedTarget.IsDead)
                    {
                        var entities = EntitiesManager.Instance.GetEntitiesOfType<Entity>();
                        foreach (var entity in entities)
                        {
                            var interactable = entity.GetBehavior<IInteractable>();
                            if (interactable != null && interactable.Model.EntityUId == creatorModel.currentTargetedTarget.EntityUId)
                            {
                                _currentCooldown = ownerModel.Cooldown * (1 - creatorModel.GetTotalStatValue(StatType.CooldownReduction));
                                var effectedStatusInfo = new AffectedStatusEffectInfo(new []{new DamageReductionDebuffStatusEffectModel(ownerModel.IncreaseDamageReceivePercent, ownerModel.EffectDuration)}, null, creatorModel);
                                interactable.GetAffected(effectedStatusInfo, default);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _currentCooldown -= Time.deltaTime;
                }
            }
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (ownerModel.CanApplyArroundAfterFinished && characterDiedHandleCompletedMessage.IsEnemyDied &&
               characterDiedHandleCompletedMessage.CharacterModel.CheckContainStatusEffectInStack(new[] { StatusEffectType.DamageReductionDebuff }))
            {
                SpawnEffectRangeVFXAsync(characterDiedHandleCompletedMessage.CharacterModel.Position, ownerModel.RangeEffectAfterEnemyDie).Forget();
                var colliders = Physics2D.OverlapCircleAll(characterDiedHandleCompletedMessage.CharacterModel.Position, ownerModel.RangeEffectAfterEnemyDie);
                foreach (var collider in colliders)
                {
                    var entity = collider.GetComponent<IEntity>();
                    if (entity != null)
                    {
                        var interactable = entity.GetBehavior<IInteractable>();
                        if (interactable != null && !interactable.Model.IsDead && interactable.Model != creatorModel)
                        {
                            if (interactable.Model.EntityType.IsCharacter())
                            {
                                var effectedStatusInfo = new AffectedStatusEffectInfo(new []{new DamageReductionDebuffStatusEffectModel(ownerModel.IncreaseDamageReceivePercent, ownerModel.EffectDuration)}, null, creatorModel);
                                interactable.GetAffected(effectedStatusInfo, default);
                            }
                        }
                    }
                }
            }
        }

        private async UniTaskVoid SpawnEffectRangeVFXAsync(Vector2 spawnPoint, float range)
        {
            var vfx = await PoolManager.Instance.Get(IMPACT_SPECIAL_RANGE_EFFECT, _cancellationTokenSource.Token);
            vfx.transform.position = spawnPoint;
            vfx.transform.localScale = new Vector2(range, range);
        }

        #endregion Class Methods
    }
}