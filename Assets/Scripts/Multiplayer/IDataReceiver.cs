using UnityEngine.Events;

namespace ITCompanySimulation.Multiplayer
{
    /// <summary>
    /// Interface for component that will receive data from other clients.
    /// This component should not receive data if local clients is master client.
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
