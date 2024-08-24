using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Runtime.Gameplay.BaseBuilder
{
    public class MapTileHighlightByTilemap : MapTileHighlight
    {
        #region Members

        [SerializeField]
        private TileBase _validTile;
        [SerializeField]
        private TileBase _invalidTile;
        [SerializeField]
        private TileBase _infoTile;
        [SerializeField]
        private Tilemap _highlightTileMap;

        #endregion Members

        #region Class Methods

        public void Highlight(IEnumerable<Vector2Int> points, TileBase tile)
        {
            foreach (var position in points)
                _highlightTileMap.SetTile((Vector3Int)position, tile);
        }

        public void Highlight(Vector2Int point, TileBase tile)
            => _highlightTileMap.SetTile((Vector3Int)point, tile);

        public override void Highlight(IEnumerable<Vector2Int> points, bool valid)
            => Highlight(points, valid ? _validTile : _invalidTile);

        public override void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType)
            => Highlight(points, GetTile(mapTileHighlightType));

        public override void Highlight(Vector2Int point, bool isValid)
            => Highlight(point, isValid ? _validTile : _invalidTile);

        public override void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType)
            => Highlight(point, GetTile(mapTileHighlightType));

        public override void Clear()
            => _highlightTileMap.ClearAllTiles();

        private TileBase GetTile(MapTileHighlightType mapTileHighlightType)
        {
            switch (mapTileHighlightType)
            {
                case MapTileHighlightType.Valid:
                    return _validTile;
                case MapTileHighlightType.Invalid:
                    return _invalidTile;
                case MapTileHighlightType.Info:
                    return _infoTile;
                default:
                    return null;
            }
        }

        #endregion Class Methods
    }
}