using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class CompleteStageWithinDurationQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.CompleteStageWithinDuration;
        public long RequiredDuration { get; private set; }

        #endregion Properties

        #region Class Methods

        public CompleteStageWithinDurationQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as CompleteStageWithinDurationQuestDataConfigItem;
            RequiredDuration = dataConfigItem.requiredDuration;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, RequiredDuration);
            return info;
        }

        #endregion Class Methods
    }
}