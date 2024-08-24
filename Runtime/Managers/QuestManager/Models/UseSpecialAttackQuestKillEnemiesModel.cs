using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class UseSpecialAttackKillEnemiesQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.UseSpecialAttackKillEnemies;
        public uint EnemiesKilledCount { get; private set; }

        #endregion Properties

        #region Class Methods

        public UseSpecialAttackKillEnemiesQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as UseSpecialAttackKillEnemiesQuestDataConfigItem;
            EnemiesKilledCount = dataConfigItem.enemiesKilledCount;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, EnemiesKilledCount);
            return info;
        }

        #endregion Class Methods
    }
}