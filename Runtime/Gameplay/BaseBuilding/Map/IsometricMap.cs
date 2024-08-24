using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace Runtime.Gameplay.BaseBuilder
{
    [RequireComponent(typeof(Grid))]
    public class IsometricMap : Map
    {
        #region Members

        [SerializeField]
        private Tilemap _ground;
        [SerializeField]
        private TileBase[] _buildingBlockingTiles;
        [SerializeField]
        private MapTileHighlight _mapTileHighlight;
        [SerializeField]
        private bool _isDrawGizmos;
        [SerializeField]
        [ShowIf(nameof(_isDrawGizmos), true)]
        private Color _gizmosColor = Color.yellow;

        #endregion Members

        #region Properties

        public override Vector3 WorldCenter => GetCellWorldPosition(size / 2);

        #endregion Properties

        #region API Methods

        protected override void OnDrawGizmos()
        {
            if (_isDrawGizmos)
            {
                base.OnDrawGizmos();
                Gizmos.color = _gizmosColor;
                Gizmos.DrawLine(GetCellWorldPosition(new Vector2Int(0, 0)), GetCellWorldPosition(new Vector2Int(size.x, 0)));
                Gizmos.DrawLine(GetCellWorldPosition(new Vector2Int(size.x, 0)), GetCellWorldPosition(new Vector2Int(size.x, size.y)));
                Gizmos.DrawLine(GetCellWorldPosition(new Vector2Int(size.x, size.y)), GetCellWorldPosition(new Vector2Int(0, size.y)));
                Gizmos.DrawLine(GetCellWorldPosition(new Vector2Int(0, size.y)), GetCellWorldPosition(new Vector2Int(0, 0)));
            }
        }

        #endregion API Methods

        #region Class Methods

        public override bool IsBuildable(Vector2Int point)
        {
            if (_ground)
            {
                var tileBase = _ground.GetTile((Vector3Int)point);
                return tileBase != null && !_buildingBlockingTiles.Any(x => x == tileBase);
            }
            else return true;
        }

        public override Vector3 ClampPosition(Vector3 position)
        {
            if (IsXY)
                return new Vector3(Mathf.Clamp(position.x, -size.x * CellOffset.x / 2.0f, size.x * CellOffset.x / 2.0f), Mathf.Clamp(position.y, 0.0f, size.y * CellOffset.y), position.z);
            else
                return new Vector3(Mathf.Clamp(position.x, -size.x * CellOffset.x / 2.0f, size.x * CellOffset.x / 2.0f), position.y, Mathf.Clamp(position.z, 0.0f, size.y * CellOffset.y));
        }

        public override void Highlight(IEnumerable<Vector2Int> points, bool valid)
            => _mapTileHighlight?.Highlight(points, valid);

        public override void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType)
             => _mapTileHighlight?.Highlight(points, mapTileHighlightType);
        public override void Highlight(Vector2Int point, bool isValid)
             => _mapTileHighlight?.Highlight(point, isValid);

        public override void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType)
             => _mapTileHighlight?.Highlight(point, mapTileHighlightType);

        public override void ClearHighlight()
             => _mapTileHighlight?.Clear();

        #endregion Class Methods
    }
}