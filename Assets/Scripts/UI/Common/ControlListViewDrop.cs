using ITCompanySimulation.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI controls list view that allows to drag and drop elements onto it.
/// </summary>
namespace ITCompanySimulation.UI
{
    public class ControlListViewDrop : ControlListView, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Array with allowed IDs. Used only to support adding IDs through editor.
        /// </summary>
        [SerializeField]
        private int[] m_AllowedDraggableIds;
        /// <summary>
        /// Contains allowed IDs of draggable objects. If draggable object has ID
        /// that is allowed it can be dropped onto this list view.
        /// </summary>
        private HashSet<int> AllowedDraggableIds = new HashSet<int>();
        /// <summary>
        /// Clone of currently dragged element. It is used to visualize position of
        /// dragged element that it will be placed at when element is dropped onto this
        /// list view.
        /// </summary>
        private GameObject DraggedElementClone;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            foreach (int id in m_AllowedDraggableIds)
            {
                bool result = AllowedDraggableIds.Add(id);

                if (false == result)
                {
                    string debugInfo = string.Format("Allowed ID ({0}) defined multiple times", id);
                    RestrictedDebug.Log(debugInfo, LogType.Warning);
                }
            }
        }

        private void Update()
        {
            //Check if dragged element clone layout index needs to be updated because of
            //dragging it to another position
            if (null != DraggedElementClone)
            {
                Transform draggedElementNeighbour1 = null;
                Transform draggedElementNeighbour2 = null;
                int siblingIndex = DraggedElementClone.transform.GetSiblingIndex();

                //Dragged element clone has neighbours if it is not first or last element in layout
                if (siblingIndex > 0)
                {
                    draggedElementNeighbour1 = Layout.transform.GetChild(siblingIndex - 1);
                }

                if (siblingIndex < (Layout.transform.childCount - 1))
                {
                    draggedElementNeighbour2 = Layout.transform.GetChild(siblingIndex + 1);
                }

                float mousePositionY = Input.mousePosition.y;
                int newSiblingIndex = siblingIndex;

                if (mousePositionY > draggedElementNeighbour1?.position.y)
                {
                    newSiblingIndex--;
                }

                if (mousePositionY < draggedElementNeighbour2?.position.y)
                {
                    newSiblingIndex++;
                }

                DraggedElementClone.transform.SetSiblingIndex(newSiblingIndex);
            }
        }

        private void OnDraggedElementParentChanged(GameObject obj, GameObject newParent)
        {
            if (newParent != Layout)
            {
                DraggableListViewElement drag = obj.GetComponent<DraggableListViewElement>();
                drag.ParentChanged -= OnDraggedElementParentChanged;
                RemoveControl(obj, false);
            }
        }

        /*Public methods*/

        public override void AddControl(GameObject control)
        {
            base.AddControl(control);

            DraggableListViewElement drag = control.GetComponent<DraggableListViewElement>();

            if (null != drag)
            {
                drag.ParentChanged += OnDraggedElementParentChanged;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            DraggableListViewElement drag = eventData.pointerDrag.GetComponent<DraggableListViewElement>();
            //Element might be dropped in list view that it was dragged from
            //For some reason sometimes Unity UI system call this even when
            //dropping at same object it was dragged from
            if (drag != null
                && false == Controls.Contains(eventData.pointerDrag)
                && true == AllowedDraggableIds.Contains(drag.ID))
            {
                AddControl(eventData.pointerDrag);

                if (null != DraggedElementClone)
                {
                    int newElementSiblingIndex = DraggedElementClone.transform.GetSiblingIndex();
                    eventData.pointerDrag.transform.SetSiblingIndex(newElementSiblingIndex);
                }
            }

            if (null != DraggedElementClone)
            {
                GameObject.Destroy(DraggedElementClone);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if ((true == eventData.dragging) && (Controls.Count > 0))
            {
                DraggableListViewElement draggedElement = eventData.pointerDrag.GetComponent<DraggableListViewElement>();

                //TODO: Contains has bad performance, find better solution
                if ((null != draggedElement) && (false == Controls.Contains(draggedElement.gameObject))
                    && (true == AllowedDraggableIds.Contains(draggedElement.ID)))
                {
                    //Initially set closest element as first control in list view. Then iterate through remaining
                    //controls to find closest one
                    RectTransform closestElementTransform = (RectTransform)Controls[0].transform;
                    RectTransform draggedElementTransform = (RectTransform)draggedElement.transform;
                    float smallestDeltaY = Mathf.Abs(draggedElementTransform.position.y - closestElementTransform.position.y);

                    //Element's index in controls collection may not match sibling index so iteration over whole collection
                    //is needed (next element can be further from dragged element but next next element can be closer)
                    for (int i = 1; i < Controls.Count; i++)
                    {
                        RectTransform controlTransform = (RectTransform)Controls[i].transform;
                        float deltaY = Mathf.Abs(draggedElementTransform.position.y - controlTransform.position.y);

                        if (deltaY < smallestDeltaY)
                        {
                            smallestDeltaY = deltaY;
                            closestElementTransform = controlTransform;
                        }
                    }

                    int cloneElementSiblingIndex = draggedElementTransform.position.y >= closestElementTransform.position.y ?
                        (closestElementTransform.GetSiblingIndex() - 1) : (closestElementTransform.GetSiblingIndex() + 1);
                    cloneElementSiblingIndex = Mathf.Clamp(cloneElementSiblingIndex, 0, Layout.transform.childCount - 1);

                    DraggedElementClone = GameObject.Instantiate(draggedElement.gameObject, Layout.transform);
                    DraggedElementClone.transform.SetSiblingIndex(cloneElementSiblingIndex);
                    CanvasGroup canvasGroupComponent = DraggedElementClone.GetComponent<CanvasGroup>();
                    canvasGroupComponent.alpha = DraggableListViewElement.DRAGGED_ELEMENT_CLONE_COLOR_ALPHA;
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (null != DraggedElementClone)
            {
                GameObject.Destroy(DraggedElementClone);
            }
        }
    }
}
