using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class CompleteStageWithHpAboveValueQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.CompleteStageWithHpAboveValue;
        public float ValuePercentThreshold { get; private set; }

        #endregion Properties

        #region Class Methods

        public CompleteStageWithHpAboveValueQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as CompleteStageWithHpAboveValueQuestDataConfigItem;
            ValuePercentThreshold = dataConfigItem.valuePercentThreshold;
        }

        public override string GetLocalizedInfo()
        {
            string info = string.Format(Description, Mathf.FloorToInt(ValuePercentThreshold * 100.0f));
            return info;
        }

        #endregion Class Methods
    }
}