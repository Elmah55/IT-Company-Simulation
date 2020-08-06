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

        public void OnDrop(PointerEventData eventData)
        {
            AddControl(eventData.pointerDrag);
        }
    }
}
