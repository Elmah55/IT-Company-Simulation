using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI controls list view that allows to drop element onto it.
/// </summary>
namespace ITCompanySimulation.UI
{
    public class ControlListViewDrop : ControlListView, IDropHandler
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

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Awake()
        {
            foreach (int id in m_AllowedDraggableIds)
            {
                bool result = AllowedDraggableIds.Add(id);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (false == result)
                {
                    Debug.LogWarningFormat("[{0}] Allowed ID ({1}) defined multiple times.",
                        this.GetType().Name,
                        id);
                }
#endif
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

        private void OnDraggedElementParentChanged(GameObject obj, GameObject newParent)
        {
            if (newParent != Layout)
            {
                DraggableListViewElement drag = obj.GetComponent<DraggableListViewElement>();
                drag.ParentChanged -= OnDraggedElementParentChanged;
                RemoveControl(obj, false);
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
            }
        }
    }
}
