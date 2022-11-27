using UnityEngine;
using UnityEngine.Events;

namespace ITCompanySimulation.Event
{
    /// <summary>
    /// Represents event that does not have any args.
    /// </summary>
    [CreateAssetMenu(fileName = "VoidEvent", menuName = "ITCompanySimulation/Events/VoidEvent")]
    public class VoidEvent : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public event UnityAction EventInvoked;

        /*Private methods*/

        /*Public methods*/

        public void RaiseEvent()
        {
            EventInvoked?.Invoke();
        }
    } 
}
