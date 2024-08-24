using System.Collections.Generic;
using UnityEngine;
using Runtime.Core.Singleton;

namespace Runtime.Gameplay.BaseBuilder
{
    public enum RotationMode
    {
        Rotate,
        Mirror,
        MirrorAndFlip,
        Disabled
    }

    [RequireComponent(typeof(Grid))]
    public abstract class Map : MonoSingleton<Map>, IMap
    {
        #region Members

        [SerializeField]
        protected Vector2Int size;
        [SerializeField]
        protected RotationMode rotation;
        protected Grid grid;

        #endregion Members

        #region Properties

        public virtual bool IsIsometric => Grid.cellLayout == GridLayout.CellLayout.Isometric;
        public virtual bool IsXY => Grid.cellSwizzle == GridLayout.CellSwizzle.XYZ || Grid.cellSwizzle == GridLayout.CellSwizzle.YXZ;
        public virtual Vector3 CellOffset => Grid.cellSize + Grid.cellGap;
        public virtual Vector3 WorldCenter => IsXY ?
                                              new Vector3(size.x / 2f * CellOffset.x, size.y / 2f * CellOffset.y, 0) :
                                              new Vector3(size.x / 2f * CellOffset.x, 0, size.y / 2f * CellOffset.y);
        public virtual Vector3 WorldSize => IsXY ? new Vector3(size.x * CellOffset.x, size.y * CellOffset.y, 1) : new Vector3(size.x * CellOffset.x, 1, size.y * CellOffset.y);
        public Grid Grid => this.grid ? this.grid : GetComponent<Grid>();
        Vector2Int IMap.Size => size;

        #endregion Properties

        #region API Methods

        protected override void Awake()
        {
            base.Awake();
            this.grid = gameObject.GetComponent<Grid>();
        }

        protected virtual void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(WorldCenter, WorldSize);
        }

        #endregion API Methods

        #region Class Methods

        public bool IsInside(Vector2Int position)
        {
            if (position.x < 0 || position.y < 0)
                return false;

            if (position.x >= size.x || position.y >= size.y)
                return false;

            return true;
        }

        public abstract bool IsBuildable(Vector2Int position);

        public virtual Vector3 ClampPosition(Vector3 position)
        {
            if (IsXY)
                return new Vector3(Mathf.Clamp(position.x, 0, size.x * CellOffset.x), Mathf.Clamp(position.y, 0, size.y * CellOffset.y), position.z);
            else
                return new Vector3(Mathf.Clamp(position.x, 0, size.x * CellOffset.x), position.y, Mathf.Clamp(position.z, 0, size.y * CellOffset.y));
        }

        public virtual Vector2Int GetCellGridPosition(Vector3 worldPosition) => (Vector2Int)this.Grid.WorldToCell(worldPosition);
        public virtual Vector3 GetCellWorldPosition(Vector2Int cellPosition) => this.Grid.CellToWorld((Vector3Int)cellPosition);

        public virtual void SetRotation(Transform transform, Vector3 direction)
        {
            if (!transform)
                return;

            if (rotation == RotationMode.Rotate)
            {
                if (IsXY)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
                }
                else transform.rotation = Quaternion.LookRotation(direction);
            }
            else if (rotation == RotationMode.Mirror || rotation == RotationMode.MirrorAndFlip)
            {
                transform.localScale = new Vector3(direction.x > 0 ? 1 : -1, transform.localScale.y, transform.localScale.z);
                if (rotation == RotationMode.MirrorAndFlip)
                    transform.localRotation = Quaternion.Euler(direction.y > 0 ? 90.0f : 0.0f, 0.0f, 0.0f);
            }
        }

        public virtual void SetRotation(Transform transform, float rotation)
        {
            if (!transform)
                return;

            if (IsXY)
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotation);
            else
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        public virtual float GetRotation(Vector3 direction)
        {
            if (IsXY)
                return Vector3.SignedAngle(direction, Vector3.right, Vector3.forward);
            else
                return Vector3.SignedAngle(direction, Vector3.right, Vector3.down);
        }

        public virtual Vector3 GetDirection(float angle)
        {
            if (IsXY)
                return Quaternion.AngleAxis(-angle, Vector3.forward) * Vector3.right;
            else
                return Quaternion.AngleAxis(-angle, Vector3.down) * Vector3.right;
        }

        public virtual void Highlight(IEnumerable<Vector2Int> points, bool valid) { }
        public virtual void Highlight(IEnumerable<Vector2Int> points, MapTileHighlightType mapTileHighlightType) { }
        public virtual void Highlight(Vector2Int point, bool isValid) { }
        public virtual void Highlight(Vector2Int point, MapTileHighlightType mapTileHighlightType) { }
        public virtual void ClearHighlight() { }

        #endregion Class Methods
    }
}