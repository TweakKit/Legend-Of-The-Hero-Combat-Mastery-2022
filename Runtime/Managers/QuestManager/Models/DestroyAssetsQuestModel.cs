using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class DestroyAssetsQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.DestroyAssets;
        public uint RequiredAssetsCount { get; private set; }

        #endregion Properties

        #region Class Methods

        public DestroyAssetsQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as DestroyAssetsQuestDataConfigItem;
            RequiredAssetsCount = dataConfigItem.requiredAssetsCount;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, RequiredAssetsCount);
            return info;
        }

        #endregion Class Methods
    }
}