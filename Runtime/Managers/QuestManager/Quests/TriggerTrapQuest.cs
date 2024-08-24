using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class TriggerTrapQuest : Quest<TriggerTrapQuestModel>
    {
        #region Members

        private int _triggeredTimes;
        private Registry<EntitySpawnedMessage> _entitySpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _triggeredTimes = 0;
            _entitySpawnedRegistry = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
        }

        public override void Dispose()
        {
            base.Dispose();
            _entitySpawnedRegistry.Dispose();
        }

        private void OnEntitySpawned(EntitySpawnedMessage entitySpawnedMessage)
        {
            if (entitySpawnedMessage.IsCharacterSpawned)
            {
                var characterModel = entitySpawnedMessage.EntityModel as CharacterModel;
                characterModel.HealthChangedEvent += OnTrapTriggered;
            }
        }

        private void OnTrapTriggered(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if(damageSource == DamageSource.FromTrap && value < 0)
            {
                if (!HasCompleted)
                {
                    _triggeredTimes++;
                    if (_triggeredTimes >= ownerModel.TriggerTimes)
                        Complete();
                }
            }
        }

        #endregion Class Methods
    }
}