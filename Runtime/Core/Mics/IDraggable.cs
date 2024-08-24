using UnityEngine;
using UnityEngine.EventSystems;

namespace Runtime.Core.Mics
{
    public interface IDraggable : IDragHandler, IEndDragHandler, IBeginDragHandler
    {
        #region Properties

        bool IsInteractable { get; set; }
        Transform OwnerTransform { get; }
        Vector3 DragToPosition { get; }
        bool HasSucceededDragging { get; }

        #endregion Properties

        #region Interface Methods

        void OnSucceededDragging();

        #endregion Interface Methods
    }
}