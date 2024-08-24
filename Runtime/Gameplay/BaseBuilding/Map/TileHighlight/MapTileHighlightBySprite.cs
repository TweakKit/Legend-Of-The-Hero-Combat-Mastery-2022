using System.Collections.Generic;
using UnityEngine;
using Sirenix.Utilities;

namespace Runtime.Gameplay.BaseBuilder
{
    public class MapTileHighlightBySprite :  MapTileHighlight
    {
        #region Members

        [SerializeField]
        private SpriteRenderer _highlightObjectPrefab;
        [SerializeField]
        private Color _validColor = Color.green;
        [SerializeField]
        private Color _invalidColor = Color.red;
        [SerializeField]
        private Color _infoColor = Color.blue;

        private List<SpriteRenderer> _highlightObjects = new List<SpriteRenderer>();
        private Dictionary<Vector2Int, SpriteRenderer> _showedHighlightObjects = new Dictionary<Vector2Int, SpriteRenderer>();

        #endregion Members

        #region Class Methods

        public void Highlight(IEnumerable<Vector2Int> points, Color color)
        {
            foreach (var position in points)
                Highlight(position, color);
        }

        public void Highlight(Vector2Int point, Color color)
        {
            if (!_showedHighlightObjects.ContainsKey(point))
            {
                SpriteRenderer spriteRenderer;
                if (_highlightObjects.Count != 0)
                {
                    spriteRenderer = _highlightObjects[0];
                    spriteRenderer.gameObject.SetActive(true);
                    _highlightObjects.RemoveAt(0);

                }
                else spriteRenderer = Instantiate(_highlightObjectPrefab, transform);

                spriteRenderer.transform.position = Map.Instance.GetCellWorldPosition(point);
                _showedHighlightObjects.Add(point, spriteRenderer);
            }

            _showedHighlightObjects[point].color = color;
        }


        public override void Highlight(IEnumerable<Vector2Int> points, bool valid)
            => Highlight(points, valid ? _validColor : _invalidColor);

        public override void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType)
            => Highlight(points, GetColor(mapTileHighlightType));

        public override void Highlight(Vector2Int point, bool isValid)
            => Highlight(point, isValid ? _validColor : _invalidColor);

        public override void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType)
            => Highlight(point, GetColor(mapTileHighlightType));

        public override void Clear()
        {
            _showedHighlightObjects.Values.ForEach(u => u.gameObject.SetActive(false));
            _highlightObjects.AddRange(_showedHighlightObjects.Values);
            _showedHighlightObjects.Clear();
        }

        private Color GetColor(MapTileHighlightType mapTileHighlightType)
        {
            switch (mapTileHighlightType)
            {
                case MapTileHighlightType.Valid:
                    return _validColor;
                case MapTileHighlightType.Invalid:
                    return _invalidColor;
                case MapTileHighlightType.Info:
                    return _infoColor;
                default:
                    return Color.red;
            }
        }

        #endregion Class Methods
    }
}