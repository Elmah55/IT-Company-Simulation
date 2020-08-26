using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This interface provides api for object pool that can be used to
    /// reuse exsisting objects instead of destroying them
    /// </summary>
    public interface IObjectPool<T>
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Gets object from pool
        /// </summary>
        /// <returns>Object if at least one object is in the pool. Null if there are no objects in pool</returns>
        T GetObject();
        /// <summary>
        /// Adds object to the pool
        /// </summary>
        void AddObject(T obj);
    } 
}
