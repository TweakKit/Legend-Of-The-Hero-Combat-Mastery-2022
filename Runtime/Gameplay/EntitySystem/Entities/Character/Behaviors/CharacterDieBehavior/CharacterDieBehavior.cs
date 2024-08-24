using System;
using System.Threading;
using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.Manager;
using Runtime.ConfigModel;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This behavior makes the character disappear from the scene after death.<br/>
    /// Its game object will be returned into the pool right away.
    /// </summary>
    public sealed class CharacterDieBehavior : CharacterBehavior, IDisable
    {
        #region Members

        private DeathDataIdentity _deathDataIdentity;
        private CancellationTokenSource _cancellationTokenSource;

        #endregion Members

        #region Class Methods

        public override bool InitModel(EntityModel model, Transform transform)
        {
            base.InitModel(model, transform);

            var entitySpawnEntityData = model.GetEntityDistinctData<IEntityDeathData>();
            if (entitySpawnEntityData != null && entitySpawnEntityData.DeathDataIdentity.deathDataType != DeathDataType.None)
                _deathDataIdentity = entitySpawnEntityData.DeathDataIdentity;
            else
                _deathDataIdentity = DeathDataIdentity.None;

            ownerModel.DeathEvent += OnDeath;
            return true;
        }

        private void OnDeath(DamageSource damageSource)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            StartLayingOnGroundAsync(_cancellationTokenSource.Token).Forget();
        }

        private async UniTaskVoid StartLayingOnGroundAsync(CancellationToken cancellationToken)
        {
            if (!ownerModel.EntityType.IsHero())
                ownerTransform.position = -Vector2.one * 999999;
            Messenger.Publisher().Publish(new CharacterDiedMessage(ownerModel, _deathDataIdentity));
            await UniTask.Delay(TimeSpan.FromSeconds(ownerModel.EntityType.GetLayOnGroundTime()), cancellationToken: cancellationToken, ignoreTimeScale: true);
            PoolManager.Instance.Remove(ownerTransform.gameObject);
        }

        public void Disable() => _cancellationTokenSource?.Cancel();

        #endregion Class Methdos
    }
}