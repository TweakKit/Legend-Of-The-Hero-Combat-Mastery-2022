using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class DropBuffHpWhenEnemyDiedSkillTreeSystem : SkillTreeSystem<DropBuffHpWhenEnemyDiedSkillTreeSystemModel>
    {
        #region Members

        private const string HEAL_BUFF_ASSET_PREFAB = "heal_bottle";
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _cancellationTokenSource = new CancellationTokenSource();
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource.Cancel();
            _characterDiedHandleCompletedRegistry.Dispose();
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (characterDiedHandleCompletedMessage.IsEnemyDied)
            {
                var rate = Random.Range(0, 1f);
                if(rate < ownerModel.RateDrop)
                {
                    SpawnHealAssetAsync(characterDiedHandleCompletedMessage.CharacterModel.Position).Forget();
                }
            }
        }

        private async UniTaskVoid SpawnHealAssetAsync(Vector2 spawnPoint)
        {
            var healAsset = await PoolManager.Instance.Get(HEAL_BUFF_ASSET_PREFAB, cancellationToken: _cancellationTokenSource.Token);
            healAsset.transform.position = spawnPoint;
            healAsset.GetComponent<CauseActionDroppable>().Init(ownerModel.LifeTime, Buff);
        }

        private void Buff(IInteractable interactable)
        {
            if (!interactable.Model.IsDead)
            {
                var healStatusEffect = new HealStatusEffectModel(ownerModel.BuffHealthValue, DamageSource.FromDroppable);
                interactable.GetAffected(new AffectedStatusEffectInfo(new[] { healStatusEffect }, default, null), default);
            }
        }

        #endregion Class Methods
    }
}
