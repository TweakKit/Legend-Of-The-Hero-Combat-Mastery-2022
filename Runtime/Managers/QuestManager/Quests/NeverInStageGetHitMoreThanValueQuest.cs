using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class NeverInStageGetHitMoreThanValueQuest : Quest<NeverInStageGetHitMoreThanValueQuestModel>
    {
        #region Members

        private int _currentHitTimesCount;
        private HeroModel _heroModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _currentHitTimesCount = 0;
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
        }

        private void OnHealthChanged(float value, DamageProperty damageProperty, DamageSource damageSource)
        {
            if (HasCompleted)
            {
                if (value < 0 && damageSource != DamageSource.FromTrap)
                {
                    _currentHitTimesCount++;
                    if (_currentHitTimesCount >= ownerModel.HitTimesCountTheshold)
                        HasCompleted = false;
                }
            }
        }

        #endregion Class Methods
    }
}