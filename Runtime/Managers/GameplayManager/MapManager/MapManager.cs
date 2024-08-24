using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Runtime.Core.Singleton;
using Pathfinding;
using UnityRandom = UnityEngine.Random;

namespace Runtime.Gameplay.Manager
{
    public class MapManager : MonoSingleton<MapManager>, IDisposable
    {
        #region Members

        private const int PATH_FINDING_COST_MULTIPLIER = 1000;
        [SerializeField]
        private int _randomOffsetPathThresholdSlotsCount = 2;
        [SerializeField]
        private int _randomMoveSearchMinSlotsCount = 2;
        [SerializeField]
        private int _randomMoveSearchMaxSlotsCount = 3;
        [SerializeField]
        [Range(0, 360)]
        private float _randomMoveSearchMinOffsetDegrees;
        [SerializeField]
        [Range(0, 360)]
        private float _randomMoveSearchMaxOffsetDegrees;
        private MapLoader _mapLoader;

        #endregion Members

        #region Properties

        public GridGraph ActiveGraph
        {
            get
            {
                var gridGraph = AstarPath.active.data.gridGraph;
                return gridGraph;
            }
        }

        public float SlotSize { get { return ActiveGraph.nodeSize; } }
        public float SlotHalfSize { get { return ActiveGraph.nodeSize * 0.5f; } }
        public float Width { get { return ActiveGraph.width; } }
        public float Height { get { return ActiveGraph.depth; } }
        public Vector3 Center { get { return ActiveGraph.center; } }
        public MapLoader MapLoader { get { return _mapLoader; } }

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            _mapLoader = FindObjectOfType<MapLoader>();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (_mapLoader == null)
                _mapLoader = FindObjectOfType<MapLoader>();
        }
#endif

        #endregion API Methods

        #region Class Methods

        public void Dispose()
            => _mapLoader.Dispose();

        public void FindPathWithRandomness(Vector2 startPosition, Vector2 endPosition, OnPathDelegate onPathCompleteCallback)
        {
            var startEndSqrDistance = (endPosition - startPosition).sqrMagnitude;
            var randomOffsetPathThresholdSqrDistance = (_randomOffsetPathThresholdSlotsCount * SlotSize) * (_randomOffsetPathThresholdSlotsCount * SlotSize);
            if (startEndSqrDistance > randomOffsetPathThresholdSqrDistance)
            {
                var randomMoveSearchSlotsCount = UnityRandom.Range(_randomMoveSearchMinSlotsCount, _randomMoveSearchMaxSlotsCount + 1);
                var randomMoveSearchSqrDistance = (randomMoveSearchSlotsCount * SlotSize) * (randomMoveSearchSlotsCount * SlotSize);
                if (randomMoveSearchSqrDistance > startEndSqrDistance)
                {
                    var startEndSlotsCount = Mathf.FloorToInt((endPosition - startPosition).magnitude / SlotSize);
                    randomMoveSearchSlotsCount = startEndSlotsCount;
                }
                var randomMoveSearchLength = randomMoveSearchSlotsCount * SlotSize;
                var randomMoveSearchOffsetDegrees = UnityRandom.Range(_randomMoveSearchMinOffsetDegrees, _randomMoveSearchMaxOffsetDegrees);
                var angleOffset = UnityRandom.Range(-randomMoveSearchOffsetDegrees, randomMoveSearchOffsetDegrees);
                Vector2 offset = Quaternion.Euler(0, 0, angleOffset) * (endPosition - startPosition).normalized * randomMoveSearchLength;
                endPosition = startPosition + offset;
                Path path = ABPath.Construct(startPosition, endPosition, onPathCompleteCallback);
                AstarPath.StartPath(path);
            }
            else
            {
                Path path = ABPath.Construct(startPosition, endPosition, onPathCompleteCallback);
                AstarPath.StartPath(path);
            }
        }

        public void FindStraightPath(Vector2 startPosition, Vector2 endPosition, OnPathDelegate onPathCompleteCallback)
        {
            Path path = ABPath.Construct(startPosition, endPosition, onPathCompleteCallback);
            AstarPath.StartPath(path);
        }

        public void FindMoveAwayTargetPath(Vector2 startPosition, Vector2 endPosition, int searchLength, int spreadLength, float aimStrength, OnPathDelegate onPathCompleteCallback)
        {
            FleePath fleePath = FleePath.Construct(startPosition, endPosition, searchLength * PATH_FINDING_COST_MULTIPLIER, onPathCompleteCallback);
            fleePath.spread = spreadLength * PATH_FINDING_COST_MULTIPLIER;
            fleePath.aimStrength = aimStrength;
            AstarPath.StartPath(fleePath);
        }

        public void FindNeighbourTargetPath(Vector2 fromPosition, Vector2 targetPosition, OnPathDelegate onPathCompleteCallback)
            => FindPathWithRandomness(fromPosition, targetPosition, onPathCompleteCallback);

        public void FindRandomPath(Vector2 fromPosition, int searchLength, int spreadLength, OnPathDelegate onPathCompleteCallback)
        {
            RandomPath randomPath = RandomPath.Construct(fromPosition, searchLength * PATH_FINDING_COST_MULTIPLIER, onPathCompleteCallback);
            randomPath.spread = spreadLength * PATH_FINDING_COST_MULTIPLIER;
            randomPath.aimStrength = 0.0f;
            AstarPath.StartPath(randomPath);
        }

        public bool IsWalkable(Vector2 position)
        {
            if (IsValidNodePosition(position))
            {
                var node = ActiveGraph.GetNode(position);
                return node.Walkable;
            }
            else return false;
        }

        public void UpdateMap(Vector2 position)
            => ActiveGraph.UpdateNodeStatus(position);

        public void UpdateMap(int indexX, int indexY)
            => ActiveGraph.UpdateNodeStatus(indexX, indexY);

        public bool IsValidNodePosition(Vector2 position)
            => ActiveGraph.IsValidNodePosition(position);

        public bool IsValidNodeIndex(int nodeIndexX, int nodeIndexY)
            => ActiveGraph.IsValidNodeIndex(nodeIndexX, nodeIndexY);

        public Vector2Int GetNodeIndex(Vector2 position)
            => ActiveGraph.GetNodeIndex(position);

        public void RescanMapArea(Vector2 scanPosition, int scanMaxBoundNodes)
        {
            var validNodeIndexes = new List<Vector2Int>();
            var scanCenterNodeIndex = GetNodeIndex(scanPosition);

            for (int x = -scanMaxBoundNodes; x <= scanMaxBoundNodes; x++)
            {
                for (int y = -scanMaxBoundNodes; y <= scanMaxBoundNodes; y++)
                {
                    int nodenIdexX = scanCenterNodeIndex.x + x;
                    int nodeIndexY = scanCenterNodeIndex.y + y;
                    var isValidNodeIndex = IsValidNodeIndex(nodenIdexX, nodeIndexY);
                    if (isValidNodeIndex)
                        validNodeIndexes.Add(new Vector2Int(nodenIdexX, nodeIndexY));
                }
            }

            var noDuplicatedValidNodeIndexes = validNodeIndexes.Distinct().ToList();
            foreach (var nodeIndex in noDuplicatedValidNodeIndexes)
                UpdateMap(nodeIndex.x, nodeIndex.y);
        }

        public List<Vector2> GetWalkablePositionsAroundPosition(Vector2 originPosition, float radius, int spreadNodesCount = 0, int numberOfDirections = 16)
        {
            var validPositions = new List<Vector2>();
            var rotateAngle = 360.0f / numberOfDirections;
            for (int c = 0; c <= spreadNodesCount; c++)
            {
                for (int i = 0; i < numberOfDirections; i++)
                {
                    var angle = Quaternion.AngleAxis(rotateAngle * i, Vector3.forward);
                    var checkPosition = (Vector2)(angle * Vector2.up) * (radius + c * SlotSize) + originPosition;
                    if (IsWalkable(checkPosition))
                        validPositions.Add(checkPosition);
                }
            }

            return validPositions;
        }

        #endregion Class Methods
    }
}