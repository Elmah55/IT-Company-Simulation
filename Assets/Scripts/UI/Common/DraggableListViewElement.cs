using UnityEngine;
using UnityEngine.EventSystems;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// List view element that can be dragged and dropped to control list view.
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
        [Tooltip("If ID matches allowed IDs of control list view it can be dropped onto that list view.")]
        [SerializeField]
        private int m_ID;
        private RectTransform LayoutTransform;
        private CanvasGroup CanvasGroupComponent;
        /// <summary>
        /// Clone of this element placed in list view that this element is dragged from. It is used
        /// to visualize position of this element before it was dragged out of list view.
        /// </summary>
        private DraggableListViewElement DraggedElementClone;

        /*Public consts fields*/

        /// <summary>
        /// Color of dragged element clone. See: <see cref="DraggedElementClone"/>
        /// </summary>
        public const float DRAGGED_ELEMENT_CLONE_COLOR_ALPHA = 0.5f;

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

        private void Awake()
        {
            TransformComponent = GetComponent<RectTransform>();
            CanvasGroupComponent = GetComponent<CanvasGroup>();
        }

        /*Public methods*/

        public void OnBeginDrag(PointerEventData eventData)
        {
            CanvasGroupComponent.blocksRaycasts = false;
            LayoutTransform = (RectTransform)gameObject.transform.parent;
            int siblingIndex = TransformComponent.GetSiblingIndex();
            gameObject.transform.SetParent(BeginDragParentTransform);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(BeginDragParentTransform, Input.mousePosition, null, out MousePositionOffset);
            MousePositionOffset = MousePositionOffset - (Vector2)TransformComponent.localPosition;

            DraggedElementClone =
                GameObject.Instantiate<DraggableListViewElement>(gameObject.GetComponent<DraggableListViewElement>(), LayoutTransform);
            DraggedElementClone.transform.SetSiblingIndex(siblingIndex);
            DraggedElementClone.CanvasGroupComponent.alpha = DRAGGED_ELEMENT_CLONE_COLOR_ALPHA;
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

            //Parent might be changed by list view that this element is dropped into
            if (TransformComponent.parent.gameObject != BeginDragParentTransform.gameObject)
            {
                ParentChanged?.Invoke(this.gameObject, TransformComponent.parent.gameObject);
            }
            //Element was not dropped to another list view. Place this element back to previous
            //list view's layout with same sibling index as before.
            else
            {
                int siblingIndex = DraggedElementClone.transform.GetSiblingIndex();
                gameObject.transform.SetParent(LayoutTransform);
                TransformComponent.SetSiblingIndex(siblingIndex);
            }

            GameObject.Destroy(DraggedElementClone.gameObject);
        }
    }
}
