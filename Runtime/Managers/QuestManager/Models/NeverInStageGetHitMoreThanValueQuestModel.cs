using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class NeverInStageGetHitMoreThanValueQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.NeverInStageGetHitMoreThanValue;
        public uint HitTimesCountTheshold { get; private set; }

        #endregion Properties

        #region Class Methods

        public NeverInStageGetHitMoreThanValueQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as NeverInStageGetHitMoreThanValueQuestDataConfigItem;
            HitTimesCountTheshold = dataConfigItem.hitTimesCountTheshold;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, HitTimesCountTheshold);
            return info;
        }

        #endregion Class Methods
    }
}