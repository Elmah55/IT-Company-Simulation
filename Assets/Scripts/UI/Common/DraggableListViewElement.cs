using UnityEngine;
using UnityEngine.EventSystems;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// List view element that can be dragged to other control list view.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DraggableListViewElement : ListViewElement, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        private RectTransform TransformComponent;
        /// <summary>
        /// Offset of dragged object from mouse position
        /// </summary>
        private Vector2 MousePositionOffset;
        /// <summary>
        /// Stored to place element in same order after dragging
        /// </summary>
        private int SiblingIndex;
        [Tooltip("If ID matches allowed IDs of control list view it can be dropped onto that list view.")]
        [SerializeField]
        private int m_ID;
        private RectTransform LayoutTransform;
        private CanvasGroup CanvasGroupComponent;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// This object will be child of this transform when dragging
        /// to avoid any effects that can be applied to this object
        /// when dragging.
        /// </summary>
        public RectTransform BeginDragParentTransform { get; set; }
        public event ParentChangeAction ParentChanged;
        /// <summary>
        /// If ID matches allowed IDs of control list view it can be dropped onto that list view.
        /// </summary>
        public int ID
        {
            get
            {
                return m_ID;
            }
        }

        /*Private methods*/

        private void Start()
        {
            TransformComponent = GetComponent<RectTransform>();
            CanvasGroupComponent = GetComponent<CanvasGroup>();
        }

        /*Public methods*/

        public void OnBeginDrag(PointerEventData eventData)
        {
            CanvasGroupComponent.blocksRaycasts = false;
            LayoutTransform = (RectTransform)gameObject.transform.parent;
            SiblingIndex = TransformComponent.GetSiblingIndex();
            gameObject.transform.SetParent(BeginDragParentTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(BeginDragParentTransform, Input.mousePosition, null, out MousePositionOffset);
            MousePositionOffset = MousePositionOffset - (Vector2)TransformComponent.localPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 newPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(BeginDragParentTransform, Input.mousePosition, null, out newPosition);
            newPosition -= MousePositionOffset;
            TransformComponent.localPosition = newPosition;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            CanvasGroupComponent.blocksRaycasts = true;

            //Parent might be changed by other script when ui element is dropped
            if (TransformComponent.parent.gameObject != BeginDragParentTransform.gameObject)
            {
                ParentChanged?.Invoke(this.gameObject, TransformComponent.parent.gameObject);
            }
            else
            {
                gameObject.transform.SetParent(LayoutTransform);
                TransformComponent.SetSiblingIndex(SiblingIndex);
            }
        }
    }
}
