using Runtime.Definition;

namespace Runtime.Utilities
{
    public static class GameHelper
    {
        #region Class Methods

        public static int GetFullStageId(int worldId, StageModeType mode, int stageId)
        {
            var outputStageId = stageId + (int)mode * Constants.STAGE_MODE_FACTOR + worldId * Constants.WORLD_FACTOR;
            return outputStageId;
        }

        public static int GetFullEquipmentId(EquipmentType equipmentType, int setId, RarityType rarityType, EquipmentGrade grade)
        {
            var fullId = Constants.EQUIPMENT_NUMBER + (int)equipmentType * Constants.EQUIPMENT_TYPE_FACTOR + setId +
                         (int)rarityType * Constants.EQUIPMENT_RARITY_FACTOR + (int)grade * Constants.EQUIPMENT_GRADE_FACTOR;
            return fullId;
        }

        public static int GetNextStage(int worldId, bool isMaxStage, int stageId, StageModeType mode = StageModeType.Normal)
        {
            if (isMaxStage)
            {
                worldId++;
                stageId = 1;
            }
            else
            {
                stageId++;
            }

            var fullStageId = GetFullStageId(worldId, mode, stageId);
            return fullStageId;
        }

        public static bool BelongStageInWorld(int stageId, int worldId, int fullStageId)
        {
            var a = stageId + worldId * 10_000;
            var stageMode = fullStageId - a;
            return stageMode <= 9 && stageMode >= 0;
        }

        public static bool BelongWorldInMode(int worldId, StageModeType mode, int fullStageId)
        {
            var stageId = fullStageId - worldId * Constants.WORLD_FACTOR - ((int)mode) * Constants.STAGE_MODE_FACTOR;
            return stageId <= Constants.MAX_STAGE_ID && stageId >= Constants.MIN_STAGE_ID;
        }

        #endregion Class Methods
    }
}