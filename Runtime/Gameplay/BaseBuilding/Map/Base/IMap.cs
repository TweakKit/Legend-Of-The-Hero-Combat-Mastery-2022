using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.BaseBuilder
{
    public interface IMap
    {
        #region Properties

        bool IsXY { get; }
        bool IsIsometric { get; }
        Vector2Int Size { get; }
        Vector3 CellOffset { get; }
        Vector3 WorldCenter { get; }

        #endregion Properties

        #region Interface Methods

        bool IsInside(Vector2Int point);
        bool IsBuildable(Vector2Int point);
        Vector3 ClampPosition(Vector3 position);
        Vector2Int GetCellGridPosition(Vector3 worldPosition);
        Vector3 GetCellWorldPosition(Vector2Int cellPosition);
        void SetRotation(Transform transform, Vector3 direction);
        void SetRotation(Transform transform, float rotation);
        float GetRotation(Vector3 direction);
        Vector3 GetDirection(float angle);
        void Highlight(IEnumerable<Vector2Int> points, bool valid);
        void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType);
        void Highlight(Vector2Int point, bool isValid);
        void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType);
        void ClearHighlight();

        #endregion Interface Methods
    }
}