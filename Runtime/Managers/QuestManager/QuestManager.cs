using System.Collections.Generic;
using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Gameplay.Manager;

namespace Runtime.Gameplay.Quest
{
    public class QuestManager : MonoBehaviour
    {
        #region Members

        private List<IQuest> _quests = new List<IQuest>();

        #endregion Members

        #region Properties

        public List<IQuest> Quests => _quests;

        #endregion Properties

        #region API Methods

        private void Update()
        {
            foreach (var quest in _quests)
                quest.Update(Time.deltaTime);
        }

        public void OnDisable()
        {
            if (_quests != null)
            {
                foreach (var quest in _quests)
                    quest.Dispose();
            }
        }

        #endregion API Methods

        #region Class Methods

        public void LoadQuests(QuestIdentity[] questIdentities)
            => InitQuests(questIdentities);

        public void InitQuests(QuestIdentity[] questIdentities)
        {
            _quests = new List<IQuest>();
            if (questIdentities != null)
            {
                foreach (var questIdentity in questIdentities)
                {
                    var questDataConfigItem = GameplayDataManager.Instance.QuestDataConfigItems[(questIdentity.questType, questIdentity.questDataId)];
                    var questData = new QuestData(questDataConfigItem);
                    var questModel = QuestModelFactory.GetQuestModel(questIdentity.questType, questData);
                    var quest = QuestFactory.GetQuest(questIdentity.questType);
                    if (quest != null)
                    {
                        quest.Init(questModel);
                        _quests.Add(quest);
                    }
                }
            }
        }

        public int GetAchievedQuestsCount()
        {
            int achievedQuestsCount = 0;
            if (_quests != null)
            {
                foreach (var quest in _quests)
                    if (quest.HasCompleted)
                        achievedQuestsCount++;
            }
            return achievedQuestsCount;
        }

        #endregion Class Methods
    }
}