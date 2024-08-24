using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class UseSpecialAttackQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.UseSpecialAttack;
        public uint MinSpecialAttackTimes { get; private set; }

        #endregion Properties

        #region Class Methods

        public UseSpecialAttackQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as UseSpecialAttackQuestDataConfigItem;
            MinSpecialAttackTimes = dataConfigItem.minSpecialAttackTimes;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, MinSpecialAttackTimes);
            return info;
        }

        #endregion Class Methods
    }
}