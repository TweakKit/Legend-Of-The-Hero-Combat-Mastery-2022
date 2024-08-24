using Runtime.Message;
using Runtime.Definition;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class CompleteStageWithHpAboveValueQuest : Quest<CompleteStageWithHpAboveValueQuestModel>
    {
        #region Members

        private float _questAchieveRequiredHp;
        private HeroModel _heroModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;
        private Registry<GameStateChangedMessage> _gameStateChangedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
            _gameStateChangedRegistry = Messenger.Subscriber().Subscribe<GameStateChangedMessage>(OnGameStateChanged);
        }

        public override void Dispose()
        {
            base.Dispose();
            _heroSpawnedRegistry.Dispose();
            _gameStateChangedRegistry.Dispose();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            HasCompleted = true;
            _heroModel = message.HeroModel;
            _heroModel.HealthChangedEvent += OnHealthChanged;
            _questAchieveRequiredHp = _heroModel.MaxHp * ownerModel.ValuePercentThreshold;
        }

        private void OnHealthChanged(float value, DamageProperty property, DamageSource source)
        {
            var currentHp = _heroModel.CurrentHp;
            if (currentHp >= _questAchieveRequiredHp)
                Complete();
            else
                HasCompleted = false;
        }

        private void OnGameStateChanged(GameStateChangedMessage gameStateChangedMessage)
        {
            if (gameStateChangedMessage.GameStateType == GameStateEventType.WinStage)
            {
                var currentHp = _heroModel.CurrentHp;
                if (currentHp >= _questAchieveRequiredHp)
                    Complete();
                else
                    HasCompleted = false;
            }
        }

        #endregion Class Methods
    }
}