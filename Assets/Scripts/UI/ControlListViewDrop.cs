using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI controls list view that allows to drop element onto it
/// </summary>
namespace ITCompanySimulation.UI
{
    public class ControlListViewDrop : ControlListView, IDropHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public override void AddControl(GameObject control)
        {
            base.AddControl(control);

            UIElementDrag drag = control.GetComponent<UIElementDrag>();

            if (null != drag)
            {
                drag.ParentChanged += OnDraggedElementParentChanged;
            }
        }

        private void OnDraggedElementParentChanged(GameObject obj, GameObject newParent)
        {
            if (newParent != Layout)
            {
                UIElementDrag drag = obj.GetComponent<UIElementDrag>();
                drag.ParentChanged -= OnDraggedElementParentChanged;
                RemoveControl(obj, false);
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            ListViewElement e = eventData.pointerDrag.GetComponent<ListViewElement>();
            //Element might be dropped in list view that it was dragged from
            //For some reason sometimes Unity UI system call this even when
            //dropping at same object it was dragged from
            if (false == Controls.Contains(eventData.pointerDrag)
                && e != null)
            {
                AddControl(eventData.pointerDrag);
            }
        }
    }
}
