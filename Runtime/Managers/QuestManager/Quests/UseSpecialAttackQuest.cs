using UnityEngine;
using Runtime.Message;
using Runtime.Gameplay.EntitySystem;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Quest
{
    public class UseSpecialAttackQuest : Quest<UseSpecialAttackQuestModel>
    {
        #region Members

        private int _currentSpecialAttackTimesCount;
        private HeroModel _heroModel;
        private Registry<HeroSpawnedMessage> _heroSpawnedRegistry;

        #endregion Members

        #region Class Methods

        public override void Init(QuestModel questModel)
        {
            base.Init(questModel);
            _currentSpecialAttackTimesCount = 0;
            _heroSpawnedRegistry = Messenger.Subscriber().Subscribe<HeroSpawnedMessage>(OnHeroSpawned);
        }

        public override void Dispose()
        {
            base.Dispose();
            _heroSpawnedRegistry.Dispose();
        }

        private void OnHeroSpawned(HeroSpawnedMessage message)
        {
            _heroModel = message.HeroModel;
            _heroModel.ReactionChangedEvent += OnReactionChanged;
        }

        private void OnReactionChanged(CharacterReactionType characterReactionType)
        {
            if (!HasCompleted)
            {
                if (characterReactionType == CharacterReactionType.JustSpecialAttack)
                {
                    _currentSpecialAttackTimesCount++;
                    if (_currentSpecialAttackTimesCount >= ownerModel.MinSpecialAttackTimes)
                        Complete();
                }
            }
        }

        #endregion Class Methods
    }
}