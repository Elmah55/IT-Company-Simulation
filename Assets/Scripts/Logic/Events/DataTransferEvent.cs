using UnityEngine;
using UnityEngine.Events;

namespace ITCompanySimulation.Events
{
    [CreateAssetMenu(fileName = "DataTransferEvent", menuName = "ITCompanySimulation/Events/Data transfer event")]
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
