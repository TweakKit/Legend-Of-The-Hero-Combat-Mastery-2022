using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime.FeatureSystem
{
    public struct LockMainQuestDefinition
    {
        public bool IsUnLock()
        {
            var mainQuestConfig = DataManager.Config.GetMainQuestDataConfig();
            var mainQuest = DataManager.Server.MainQuest;
            var questProgresses = mainQuest.MainQuestProgress;

            var foundCurrentQuest = false;
            foreach (var item in mainQuestConfig.items)
            {
                var result = questProgresses.TryGetValue(item.id, out var questProgress);
                if (!result || questProgress.QuestState != UserProcessQuestState.Claimed)
                {
                    foundCurrentQuest = true;
                    break;
                }
            }
            return !foundCurrentQuest;
        }
    }
}
