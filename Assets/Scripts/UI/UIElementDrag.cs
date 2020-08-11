using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Attaching this script to UI element allows to drag it
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class UIElementDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        private RectTransform TransformComponent;
        private RectTransform PreviousParentTransform;
        /// <summary>
        /// Offset of dragged object from mouse position
        /// </summary>
        private Vector2 MousePositionOffset;
        /// <summary>
        /// Stored to place element in same order after dragging
        /// </summary>
        private int SiblingIndex;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// This object will be child of this transform when dragging
        /// to avoid any effects that can be applied to this object
        /// when dragging
        /// </summary>
        public RectTransform DragParentTransform { get; set; }
        public event ParentChangeAction ParentChanged;

        /*Private methods*/

        private void Start()
        {
            TransformComponent = GetComponent<RectTransform>();
            PreviousParentTransform =
                TransformComponent.transform.parent.gameObject.GetComponent<RectTransform>();
        }

        /*Public methods*/

        public void OnBeginDrag(PointerEventData eventData)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            SiblingIndex = TransformComponent.GetSiblingIndex();
            TransformComponent.SetParent(DragParentTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DragParentTransform, Input.mousePosition, null, out MousePositionOffset);
            MousePositionOffset = MousePositionOffset - (Vector2)TransformComponent.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DragParentTransform, Input.mousePosition, null, out newPosition);
            newPosition -= MousePositionOffset;
            TransformComponent.localPosition = newPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            GetComponent<CanvasGroup>().blocksRaycasts = true;

            //Parent might be changed by other script when ui element is dropped
            if (TransformComponent.parent.gameObject != DragParentTransform.gameObject)
            {
                PreviousParentTransform = TransformComponent.parent.gameObject.GetComponent<RectTransform>();
                TransformComponent.SetParent(PreviousParentTransform);
                ParentChanged?.Invoke(this.gameObject, PreviousParentTransform.gameObject);
            }
            else
            {
                TransformComponent.SetParent(PreviousParentTransform);
                TransformComponent.SetSiblingIndex(SiblingIndex);
            }
        }
    }
}
