using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class DestroyAssetsQuest : Quest<DestroyAssetsQuestModel>
    {
        #region Members

        private int _assetsDestroyedTimesCount;
        private Registry<EntitySpawnedMessage> _entitySpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _assetsDestroyedTimesCount = 0;
            _entitySpawnedRegistry = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
        }

        public override void Dispose()
        {
            base.Dispose();
            _entitySpawnedRegistry.Dispose();
        }

        private void OnEntitySpawned(EntitySpawnedMessage entitySpawnedMessage)
        {
            if (entitySpawnedMessage.IsObjectSpawned)
            {
                var objectModel = entitySpawnedMessage.EntityModel as ObjectModel;
                objectModel.DestroyEvent += OnDestroy;
            }
        }

        private void OnDestroy()
        {
            if (!HasCompleted)
            {
                _assetsDestroyedTimesCount++;
                if (_assetsDestroyedTimesCount >= ownerModel.RequiredAssetsCount)
                    Complete();
            }
        }

        #endregion Class Methods
    }
}