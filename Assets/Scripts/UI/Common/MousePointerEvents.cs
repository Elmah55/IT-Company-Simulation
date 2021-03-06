﻿using UnityEngine;
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

        public event UnityAction PointerEntered;
        public event UnityAction PointerExited;
        public event UnityAction PointerDoubleClick;

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Removes all listeners of all events
        /// </summary>
        public void RemoveAllListeners()
        {
            PointerEntered = null;
            PointerExited = null;
            PointerDoubleClick = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEntered?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExited?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (2 == eventData.clickCount)
            {
                PointerDoubleClick?.Invoke();
            }
        }
    }
}
