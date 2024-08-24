using UnityEngine;
using Runtime.Message;
using Core.Foundation.PubSub;
using Cysharp.Threading.Tasks;

namespace Runtime.Gameplay.Manager
{
    public sealed class GameplayFlowProcessor : MonoBehaviour
    {
        #region Members

        private Registry<SceneDataLoadedMessage> _sceneDataLoadedRegistry;
        private Registry<CharacterDiedMessage> _characterDiedRegistry;
        private Registry<RewardsDroppedMessage> _rewardsDroppedRegistry;
        private EntitiesManager _entitiesManager;
        private GameplayManager _gameplayManager;

        #endregion Members

        #region API Methods

        private void Awake()
        {
            _sceneDataLoadedRegistry = Messenger.Subscriber().Subscribe<SceneDataLoadedMessage>(OnSceneDataLoaded);
            _characterDiedRegistry = Messenger.Subscriber().Subscribe<CharacterDiedMessage>(OnCharacterDied);
            _rewardsDroppedRegistry = Messenger.Subscriber().Subscribe<RewardsDroppedMessage>(OnRewardsDropped);
        }

        private void Start()
        {
            _gameplayManager = GameplayManager.Instance;
            _entitiesManager = EntitiesManager.Instance;
        }

        public void OnDestroy()
        {
            _sceneDataLoadedRegistry.Dispose();
            _characterDiedRegistry.Dispose();
            _rewardsDroppedRegistry.Dispose();
        }

        #endregion API Methods

        #region Class Methods

        private void OnSceneDataLoaded(SceneDataLoadedMessage SceneDataLoadedMessages)
            => _gameplayManager.StartGameplay();

        private void OnCharacterDied(CharacterDiedMessage characterDiedMessage)
            => OnCharacterDiedAsync(characterDiedMessage).Forget();

        private async UniTaskVoid OnCharacterDiedAsync(CharacterDiedMessage characterDiedMessage)
        {
            var handleCharacterDiedResult = await _entitiesManager.HandleCharacterDied(characterDiedMessage);
            Messenger.Publisher().Publish(new CharacterDiedHandleCompletedMessage(characterDiedMessage.CharacterModel));

            if (handleCharacterDiedResult == HandleCharacterDiedResultType.ClearWave)
                _gameplayManager.ClearWave();
            else if (handleCharacterDiedResult == HandleCharacterDiedResultType.HeroDied)
                _gameplayManager.KillHero();
        }

        private void OnRewardsDropped(RewardsDroppedMessage rewardsDroppedMessage)
        {
            _gameplayManager.GetDroppableRewards(rewardsDroppedMessage.RewardData);
            _entitiesManager.CreateDroppableRewardsAsync(rewardsDroppedMessage.RewardData, rewardsDroppedMessage.SpawnPosition).Forget();
        }

        #endregion Class Methods
    }
}