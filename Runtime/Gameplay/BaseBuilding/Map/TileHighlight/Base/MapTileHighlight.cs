using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Gameplay.BaseBuilder
{
    public abstract class MapTileHighlight : MonoBehaviour
    {
        #region Interface Methods

        public abstract void Highlight(IEnumerable<Vector2Int> points, bool valid);
        public abstract void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType);
        public abstract void Highlight(Vector2Int point, bool isValid);
        public abstract void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType);
        public abstract void Clear();

        #endregion Interface Methods
    }
}