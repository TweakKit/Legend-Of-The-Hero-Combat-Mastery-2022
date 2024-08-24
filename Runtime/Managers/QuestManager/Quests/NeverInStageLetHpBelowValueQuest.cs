using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class NeverInStageLetHpBelowValueQuest : Quest<NeverInStageLetHpBelowValueQuestModel>
    {
        #region Members

        private float _requiredHpValueThreshold;
        private HeroModel _heroModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            HasCompleted = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            _heroSpawnedRegistry.Dispose();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _heroModel = message.HeroModel;
            _heroModel.HealthChangedEvent += OnHealthChanged;
            _requiredHpValueThreshold = _heroModel.MaxHp * ownerModel.ValuePercentThreshold;
        }

        private void OnHealthChanged(float deltaHp, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (HasCompleted)
            {
                if (_heroModel.CurrentHp < _requiredHpValueThreshold)
                    HasCompleted = false;
            }
        }

        #endregion Class Methods
    }
}