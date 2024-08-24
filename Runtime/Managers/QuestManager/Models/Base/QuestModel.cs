using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Localization;

namespace Runtime.Gameplay.Quest
{
    public abstract class QuestModel
    {
        #region Properties

        public abstract QuestType QuestType { get; }
        public uint Id { get; protected set; }
        public string Description { get; protected set; }

        #endregion Properties

        #region Class Methods

        public QuestModel(QuestData questData)
        {
            Id = questData.questDataConfigItem.id;
            Description = LocalizationManager.GetLocalize(LocalizeTable.QUEST, LocalizeKeys.GetQuestDescription(QuestType));
        }

        public virtual string GetLocalizedInfo() => Description;

        #endregion Class Methods
    }

    public class QuestData
    {
        #region Members

        public QuestDataConfigItem questDataConfigItem;

        #endregion Members

        #region Class Methods

        public QuestData(QuestDataConfigItem questDataConfigItem)
            => this.questDataConfigItem = questDataConfigItem;

        #endregion Class Methods
    }
}