using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class detects certain mouse pointer events on object
    /// that this script is attached to and invokes appropriate events.
    /// This can be used as alternative for Unity's 'EventTrigger' class as EventTrigger
    /// class intercepts events and breaks components like ScrollRect
    /// </summary>
    public class MousePointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public event UnityAction PointerEntered;
        public event UnityAction PointerExited;

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Removes all listeners for pointer enter and pointer exit
        /// </summary>
        public void RemoveAllListeners()
        {
            PointerEntered = null;
            PointerExited = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke();
        }
    } 
}
