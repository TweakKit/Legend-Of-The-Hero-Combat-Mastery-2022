using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using UnityEngine;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class BoxingGlovesEquipmentSystem : EquipmentSystem<BoxingGlovesEquipmentSystemModel>
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;

        protected override void Initialize()
        {
            base.Initialize();
            _cancellationTokenSource = new CancellationTokenSource();
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        public override void Disable()
        {
            base.Disable();
            _cancellationTokenSource?.Cancel();
            _characterDiedHandleCompletedRegistry.Dispose();
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (characterDiedHandleCompletedMessage.IsEnemyDied && ownerModel.CanCreatedExplode)
                CreateDamageBoxAsync(characterDiedHandleCompletedMessage.CharacterModel,
                                        0, new[] { new DamageFactor(StatType.AttackDamage, ownerModel.CreatedDamagePercent) },
                                        ownerModel.CreatedDamageModifierModels, creatorModel, 
                                        characterDiedHandleCompletedMessage.CharacterModel.Position).Forget();
        }

        private async UniTask CreateDamageBoxAsync(EntityModel target, float damageBonus, DamageFactor[] damageFactors, StatusEffectModel[] statusEffectModels, CharacterModel creator, Vector2 spawnPoint)
        {
            var damageBoxGameObject = await PoolManager.Instance.Get(ownerModel.PrefabExplodeName, _cancellationTokenSource.Token);
            var damageBox = damageBoxGameObject.GetComponent<SpriteAnimatorDamageBox>();
            damageBox.Init(creator, DamageSource.FromOther, false, damageBonus, damageFactors, statusEffectModels, avoidEntityUids: new List<uint>() { target.EntityUId });
            damageBox.transform.position = spawnPoint;
        }
    }
}
