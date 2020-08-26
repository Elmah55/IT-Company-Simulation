using System.Collections.Generic;
using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    public class ObjectPool<T> : IObjectPool<T>
    {
        /*Private consts fields*/

        /*Private fields*/

        private Queue<T> Pool;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public void AddObject(T obj)
        {
            Pool.Enqueue(obj);
        }

        public T GetObject()
        {
            T obj = default(T);

            if (Pool.Count > 0)
            {
                obj = Pool.Dequeue();
            }

            return obj;
        }

        public ObjectPool()
        {
            Pool = new Queue<T>();
        }
    }
}
