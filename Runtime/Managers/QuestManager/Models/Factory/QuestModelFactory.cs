using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public static class QuestModelFactory
    {
        #region Class Methods

        public static QuestModel GetQuestModel(QuestType questType, QuestData questData)
        {
            switch (questType)
            {
                case QuestType.WinStage:
                    return new WinStageQuestModel(questData);

                case QuestType.CompleteStageWithinDuration:
                    return new CompleteStageWithinDurationQuestModel(questData);

                case QuestType.TriggerTrap:
                    return new TriggerTrapQuestModel(questData);

                case QuestType.UseSpecialAttack:
                    return new UseSpecialAttackQuestModel(questData);

                case QuestType.UseSpecialAttackKillEnemies:
                    return new UseSpecialAttackKillEnemiesQuestModel(questData);

                case QuestType.CompleteStageWithHpAboveValue:
                    return new CompleteStageWithHpAboveValueQuestModel(questData);

                case QuestType.NeverInStageLetHpBelowValue:
                    return new NeverInStageLetHpBelowValueQuestModel(questData);

                case QuestType.NeverInStageGetHitMoreThanValue:
                    return new NeverInStageGetHitMoreThanValueQuestModel(questData);

                case QuestType.DestroyAssets:
                    return new DestroyAssetsQuestModel(questData);
            }

            return null;
        }

        #endregion Class Methods
    }
}