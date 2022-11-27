using System;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// Provides functionality for persistent storage and reading of objects.
    /// </summary>
    public interface IObjectStorage
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Serializes provided object to file with provided name.
        /// </summary>
        /// <param name="objectToSerialize">Object that will be serialized.</param>
        /// <param name="configFileName">Name of file that serialized object will be stored in.</param>
        void SerializeObject(object objectToSerialize, string configFileName);
        /// <summary>
        /// Deserializes object of provided type from file with provided name.
        /// </summary>
        /// <param name="objectToDeserializeInstance">Instance which will be overidden by deserialized data.</param>
        /// <param name="configFileName">Name of file that deserialized object should be read from.</param>
        void DeserializeObject(object objectToDeserializeInstance, string configFileName);

    }
}
