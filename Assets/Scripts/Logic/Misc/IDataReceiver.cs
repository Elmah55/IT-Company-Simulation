using UnityEngine.Events;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// Component that will receive data from other clients
    /// </summary>
    public interface IDataReceiver
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Invoked when data is received
        /// </summary>
        event UnityAction DataReceived;
        /// <summary>
        /// Indicates whether data is received
        /// </summary>
        bool IsDataReceived { get; }

        /*Private methods*/

        /*Public methods*/
    }
}
