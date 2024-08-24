using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public class NeverInStageLetHpBelowValueQuestModel : QuestModel
    {
        #region Properties

        public override QuestType QuestType => QuestType.NeverInStageLetHpBelowValue;
        public float ValuePercentThreshold { get; private set; }

        #endregion Properties

        #region Class Methods

        public NeverInStageLetHpBelowValueQuestModel(QuestData questData) : base(questData)
        {
            var dataConfigItem = questData.questDataConfigItem as NeverInStageLetHpBelowValueQuestDataConfigItem;
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