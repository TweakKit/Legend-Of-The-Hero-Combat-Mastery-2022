using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class UseSpecialAttackKillEnemiesQuest : Quest<UseSpecialAttackKillEnemiesQuestModel>
    {
        #region Members

        private int _currentEnemiesKilledBySpecialAttackTimesCount;
        private Registry<EntitySpawnedMessage> _entitySpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _currentEnemiesKilledBySpecialAttackTimesCount = 0;
            _entitySpawnedRegistry = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
        }

        public override void Dispose()
        {
            base.Dispose();
            _entitySpawnedRegistry.Dispose();
        }

        private void OnEntitySpawned(EntitySpawnedMessage entitySpawnedMessage)
        {
            if (entitySpawnedMessage.IsEnemySpawned)
            {
                var enemyModel = entitySpawnedMessage.EntityModel as CharacterModel;
                enemyModel.DeathEvent += OnDeath;
            }
        }

        private void OnDeath(DamageSource damageSource)
        {
            if (!HasCompleted)
            {
                if (damageSource == DamageSource.FromSpecialAttack)
                {
                    _currentEnemiesKilledBySpecialAttackTimesCount++;
                    if (_currentEnemiesKilledBySpecialAttackTimesCount >= ownerModel.EnemiesKilledCount)
                        Complete();
                }
            }
        }

        #endregion Class Methods
    }
}