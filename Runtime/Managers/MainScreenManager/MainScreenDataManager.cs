using System.Threading;
using Runtime.Core.Singleton;
using Runtime.Manager.Data;
using Runtime.SceneLoading;
using Runtime.Message;
using Cysharp.Threading.Tasks;
using Core.Foundation.PubSub;

namespace Runtime.Gameplay.Manager
{
    public class MainScreenDataManager : MonoSingleton<MainScreenDataManager>
    {
        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            SceneManager.RegisterCompleteTaskBeforeNewSceneAppeared(LoadConfig);
            SceneManager.RegisterBeforeNewSceneAppeared(FinishedLoading);
        }

        #endregion API Methods

        #region Class Methods

        private async UniTask LoadConfig(CancellationToken cancellationToken)
        {
            await DataManager.Config.LoadIAPConfig(cancellationToken);
            await DataManager.Config.LoadIAPFirstTimeRewardConfig(cancellationToken);
            await DataManager.Config.LoadStructureInfo(cancellationToken);
            await DataManager.Config.LoadHeroExpInfo(cancellationToken);
            await DataManager.Config.LoadHeroInfo(cancellationToken);
            await DataManager.Config.LoadFactoryProductionInfo(cancellationToken);
            await DataManager.Config.LoadStructureInfo(cancellationToken);
            await DataManager.Config.LoadSkipTimeTicketInfo(cancellationToken);
            await DataManager.Config.LoadSkillTreeDataInfo();
            await DataManager.Config.LoadMainQuestDataConfig(cancellationToken);
        }

        private void FinishedLoading()
            => Messenger.Publisher().Publish(new SceneDataLoadedMessage());

        #endregion Class Methods
    }
}