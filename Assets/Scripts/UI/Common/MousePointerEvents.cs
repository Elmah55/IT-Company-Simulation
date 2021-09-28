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
    public class MousePointerEvents : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/


        /// <summary>
        /// True if mouse pointer is over game object.
        /// </summary>
        public bool IsMousePointerEntered { get; private set; }

        public UnityEvent PointerEntered = new UnityEvent();
        public UnityEvent PointerExited = new UnityEvent();
        public UnityEvent PointerDoubleClick = new UnityEvent();

        /*Private methods*/

        private void OnDisable()
        {
            if (true == IsMousePointerEntered)
            {
                OnPointerExit(null);
            }
        }

        /*Public methods*/

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsMousePointerEntered = true;
            PointerEntered.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsMousePointerEntered = false;
            PointerExited.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (2 == eventData.clickCount)
            {
                PointerDoubleClick.Invoke();
            }
        }
    }
}
