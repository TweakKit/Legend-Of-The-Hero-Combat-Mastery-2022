using Runtime.Definition;

namespace Runtime.Gameplay.Quest
{
    public static class QuestFactory
    {
        #region Class Methods

        public static IQuest GetQuest(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.WinStage:
                    return new WinStageQuest();

                case QuestType.CompleteStageWithinDuration:
                    return new CompleteStageWithinDurationQuest();

                case QuestType.TriggerTrap:
                    return new TriggerTrapQuest();

                case QuestType.UseSpecialAttack:
                    return new UseSpecialAttackQuest();

                case QuestType.UseSpecialAttackKillEnemies:
                    return new UseSpecialAttackKillEnemiesQuest();

                case QuestType.CompleteStageWithHpAboveValue:
                    return new CompleteStageWithHpAboveValueQuest();

                case QuestType.NeverInStageLetHpBelowValue:
                    return new NeverInStageLetHpBelowValueQuest();

                case QuestType.NeverInStageGetHitMoreThanValue:
                    return new NeverInStageGetHitMoreThanValueQuest();

                case QuestType.DestroyAssets:
                    return new DestroyAssetsQuest();
            }

            return null;
        }

        #endregion Class Methods
    }
}