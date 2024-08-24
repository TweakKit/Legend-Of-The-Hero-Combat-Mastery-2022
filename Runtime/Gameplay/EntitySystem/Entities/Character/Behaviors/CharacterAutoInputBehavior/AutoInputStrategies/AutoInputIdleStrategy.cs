using Runtime.Gameplay.Manager;
using Runtime.Message;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.EntitySystem
{
    /// <summary>
    /// This auto input strategy doesn't update any movement input data, 
    /// but just checks and updates the skill trigger input data.
    /// </summary>
    public sealed class AutoInputIdleStrategy : IAutoInputStrategy
    {
        #region Members

        private CharacterModel _ownerModel;
        private CharacterModel _targetModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public AutoInputIdleStrategy(CharacterModel ownerModel)
        {
            _ownerModel = ownerModel;
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _targetModel = EntitiesManager.Instance.HeroModel;
            _ownerModel.currentTargetedTarget = _targetModel;
        }

        public void Disable()
            => _heroSpawnedRegistry.Dispose();

        public void Update()
        {
            if (!_targetModel.IsDead)
                _ownerModel.TriggerSkill();
        }

        private void OnHeroSpawned(HeroSpawnedMessage heroSpawnedMessage)
        {
            _targetModel = heroSpawnedMessage.HeroModel;
            _ownerModel.currentTargetedTarget = _targetModel;
        }

        #endregion Class Methods
    }
}