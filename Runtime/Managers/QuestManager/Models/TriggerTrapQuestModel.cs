using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class TriggerTrapQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.TriggerTrap;
        public uint TriggerTimes { get; private set; }

        #endregion Properties

        #region Class Methods

        public TriggerTrapQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as TriggerTrapQuestDataConfigItem;
            TriggerTimes = dataConfigItem.triggerTimes;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, TriggerTimes);
            return info;
        }

        #endregion Class Methods
    }
}