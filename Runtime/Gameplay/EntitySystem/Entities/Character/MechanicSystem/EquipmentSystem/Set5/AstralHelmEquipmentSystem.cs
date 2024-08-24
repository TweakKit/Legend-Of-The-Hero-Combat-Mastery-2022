using System.Collections.Generic;
using Runtime.Definition;
using Runtime.Gameplay.Manager;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    public class AstralHelmEquipmentSystem : EquipmentSystem<AstralHelmEquipmentSystemModel>
    {
        #region Members

        private bool _isTriggeredIncrease;
        private List<CharacterModel> _enemyModels;
        private Registry<EntitySpawnedMessage> _entitySpawnedRegistry;
        private Registry<CharacterDiedHandleCompletedMessage> _characterDiedHandleCompletedRegistry;

        #endregion Members

        #region Class Methods

        protected override void Initialize()
        {
            base.Initialize();
            _enemyModels = EntitiesManager.Instance.EnemyModels;
            _entitySpawnedRegistry = Messenger.Subscriber().Subscribe<EntitySpawnedMessage>(OnEntitySpawned);
            _characterDiedHandleCompletedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedHandleCompletedMessage>(OnCharacterDiedHandleCompleted);
        }

        private void OnEntitySpawned(EntitySpawnedMessage entitySpawnedMessage)
        {
            if (entitySpawnedMessage.IsEnemySpawned)
                CheckEnemyCount();
        }

        private void OnCharacterDiedHandleCompleted(CharacterDiedHandleCompletedMessage characterDiedHandleCompletedMessage)
        {
            if (characterDiedHandleCompletedMessage.IsEnemyDied)
                CheckEnemyCount();
        }

        private void CheckEnemyCount()
        {
            if (_enemyModels.Count >= ownerModel.NumberOfEnemyTriggerIncreaseDodgeChance)
            {
                if (!_isTriggeredIncrease)
                {
                    _isTriggeredIncrease = true;
                    creatorModel.BuffStat(StatType.DodgeChance, ownerModel.DodgeChanceIncreasePercent, StatModifyType.BaseBonus);
                }
            }
            else
            {
                if (_isTriggeredIncrease)
                {
                    _isTriggeredIncrease = false;
                    creatorModel.DebuffStat(StatType.DodgeChance, ownerModel.DodgeChanceIncreasePercent, StatModifyType.BaseBonus);
                }
            }
        }

        public override void Disable()
        {
            base.Disable();
            _entitySpawnedRegistry.Dispose();
            _characterDiedHandleCompletedRegistry.Dispose();
        }

        #endregion Class Methods
    }
}