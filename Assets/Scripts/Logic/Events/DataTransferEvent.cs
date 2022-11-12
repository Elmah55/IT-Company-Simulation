using UnityEngine;
using UnityEngine.Events;

namespace ITCompanySimulation.Event
{
    /// <summary>
    /// Event used for notyfing simulation manager about other components' data transfer completion.
    /// </summary>
    [CreateAssetMenu(fileName = "DataTransferEvent", menuName = "ITCompanySimulation/Events/DataTransferEvent")]
    public class DataTransferEvent : ScriptableObject
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        public event UnityAction<DataTransferSource> DataTransfered;

        /*Private methods*/

        /*Public methods*/

        public void RaiseEvent(DataTransferSource transferType)
        {
            DataTransfered?.Invoke(transferType);
        }
    }
}
