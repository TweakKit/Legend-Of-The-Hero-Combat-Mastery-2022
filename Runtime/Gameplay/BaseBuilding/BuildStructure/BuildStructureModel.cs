using System;
using System.Collections.Generic;
using UnityEngine;
using Runtime.ConfigModel;
using Runtime.Definition;
using Runtime.Manager.Data;

namespace Runtime
{
    public static class BuildStructureModelExtensions
    {
        #region Class Methods

        public static List<Vector2Int> GetLocalPoints(this int structureId)
        {
            var structureInfoConfig = DataManager.Config.GetStructureInfo();
            StructureInfoConfigItem structureConfigData = structureInfoConfig.GetStructureData(structureId);
            return structureConfigData.structureInfo.localPoints;
        }

        public static double GetBuildTime(this int structureId, int level)
        {
            var structureInfoConfig = DataManager.Config.GetStructureInfo();
            StructureInfoConfigItem structureConfigData = structureInfoConfig.GetStructureData(structureId);
            StructureLevelInfo structureLevelInfo = structureConfigData.GetStructureLevelInfo(level);
            return structureLevelInfo.buildTime;
        }

        public static int GetSkipBuildTimeCost(this double buildTime)
        {
            int numberOfMinutes = (int)(buildTime / 60);
            return numberOfMinutes * Constants.BUILD_TIME_COST_PER_MINUTE;
        }

        public static string GetStructureName(this int structureId)
        {
            var structureInfoConfig = DataManager.Config.GetStructureInfo();
            StructureInfoConfigItem structureConfigData = structureInfoConfig.GetStructureData(structureId);
            return structureConfigData.structureInfo.name;
        }

        public static string GetStructureDescription(this int structureId)
        {
            var structureInfoConfig = DataManager.Config.GetStructureInfo();
            StructureInfoConfigItem structureConfigData = structureInfoConfig.GetStructureData(structureId);
            return structureConfigData.structureInfo.description;
        }

        #endregion Class Methods
    }

    public class BuildStructureModel
    {
        #region Members

        public string uid;
        public int id;
        public int currentLevel;
        public StructureState currentState;
        public Vector2Int origin;
        public List<Vector2Int> localPoints;
        private bool _isFlipped;

        #endregion Members

        #region Properties

        public Action<bool> OnFlipped { get; set; }

        public string PrefabId => id.ToString();

        public int StructureCategoryId
        {
            get
            {
                (int,int) extractStructureId = id.ExtractStructureId();
                return extractStructureId.Item1;
            }
        }

        public StructureCategoryType StructureCategoryType => (StructureCategoryType)StructureCategoryId;

        public bool IsFlipped
        {
            get => _isFlipped;
            set
            {
                _isFlipped = value;
                OnFlipped?.Invoke(value);
            }
        }

        #endregion Properties

        #region Class Methods

        public BuildStructureModel(int id, int level, Vector2Int origin, List<Vector2Int> localPoints, bool isFlipped, StructureState structureState, string uid = null)
        {
            this.uid = uid;
            this.id = id;
            this.currentLevel = level;
            this.origin = origin;
            this.localPoints = localPoints;
            IsFlipped = isFlipped;
            currentState = structureState;
        }

        #endregion Class Methods
    }

    public class NewBuildStructureModel : BuildStructureModel
    {
        #region Members

        public ResourceData buyResourceData;

        #endregion Members

        #region Class Methods

        public NewBuildStructureModel(int id, int level, Vector2Int origin, List<Vector2Int> localPoints, bool isFlipped, StructureState structureState, ResourceData buyResourceData, string uid = null)
            : base(id, level, origin, localPoints, isFlipped, structureState, uid)
            => this.buyResourceData = buyResourceData;

        #endregion Class Methods
    }
}