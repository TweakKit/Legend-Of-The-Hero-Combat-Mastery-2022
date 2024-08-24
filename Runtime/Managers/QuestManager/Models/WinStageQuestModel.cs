using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class WinStageQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.WinStage;

        #endregion Properties

        #region Class Methods

        public WinStageQuestModel(QuestData questData) : base(questData) { }

        #endregion Class Methods
    }
}