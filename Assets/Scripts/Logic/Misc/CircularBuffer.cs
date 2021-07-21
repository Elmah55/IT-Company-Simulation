using System.Collections;
using System;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// Fixed size collection with O(1) element access complexity.
    /// Most recent elemens will override oldest one.
    /// </summary>
    public class CircularBuffer<T> : IEnumerable
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Array holding elements of buffer.
        /// </summary>
        private T[] InnerBuffer;
        /// <summary>
        /// Index of element most recent element in inner buffer.
        /// </summary>
        private int LastElementIndex = -1;
        /// <summary>
        /// Index of oldest element in inner buffer.
        /// </summary>
        private int FirstElementIndex = -1;

        private class Enumerator<T> : IEnumerator
        {
            private T[] Collection;
            private int CurrentIndex = -1;
            private int CollectionNumberOfElements;
            private int LastElementIndex;
            private int FirstElementIndex;

            public Enumerator(T[] arr, int arrNumOfElements, int arrFirstElementIndex, int arrLastElementIndex)
            {
                this.Collection = arr;
                this.LastElementIndex = arrLastElementIndex;
                this.FirstElementIndex = arrFirstElementIndex;
                this.CollectionNumberOfElements = arrNumOfElements;

            }
            public object Current
            {
                get
                {
                    return Collection[CurrentIndex];
                }
            }

            public bool MoveNext()
            {
                bool result = false;

                if (CollectionNumberOfElements > 0)
                {
                    if (-1 == CurrentIndex)
                    {
                        CurrentIndex = FirstElementIndex;
                        result = true;
                    }
                    else if (CurrentIndex != LastElementIndex)
                    {
                        CurrentIndex++;
                        CurrentIndex = CurrentIndex % Collection.Length;
                        result = true;
                    }
                }

                return result;
            }

            public void Reset()
            {
                CurrentIndex = -1;
            }
        }

        /*Public consts fields*/

        /*Public fields*/

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= InnerBuffer.Length)
                {
                    throw new IndexOutOfRangeException(
                        "Index you are trying to access is out of range ");
                }

                index = FirstElementIndex + index;
                index = index % InnerBuffer.Length;

                return InnerBuffer[index];
            }
        }
        public bool IsFull
        {
            get
            {
                return Size == InnerBuffer.Length;
            }
        }

        /// <summary>
        /// Number of elements in buffer.
        /// </summary>
        public int Size { get; private set; } = 0;
        /// <summary>
        /// How many element buffer can hold.
        /// </summary>
        public int Capacity
        {
            get
            {
                return InnerBuffer.Length;
            }
        }

        /*Private methods*/

        /*Public methods*/

        /// <param name="capacity">Indicates how many elements buffer can hold.</param>
        public CircularBuffer(int capacity)
        {
            InnerBuffer = new T[capacity];
        }

        /// <summary>
        /// Adds element to buffer.
        /// </summary>
        public void Add(T element)
        {
            if (false == IsFull)
            {
                Size++;
            }

            if (1 == Size)
            {
                FirstElementIndex = LastElementIndex = 0;
            }
            else
            {
                LastElementIndex++;
                LastElementIndex = LastElementIndex % InnerBuffer.Length;

                //Last element index is equal to first element index.
                //Older element will be overriden by new one.
                if (LastElementIndex == FirstElementIndex)
                {
                    FirstElementIndex++;
                    FirstElementIndex = FirstElementIndex % InnerBuffer.Length;
                }
            }

            InnerBuffer[LastElementIndex] = element;
        }

        /// <summary>
        /// Removes all elements from buffer.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < InnerBuffer.Length; i++)
            {
                InnerBuffer[i] = default(T);
            }

            LastElementIndex = -1;
            FirstElementIndex = -1;
            Size = 0;
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator<T>(InnerBuffer, Size, FirstElementIndex, LastElementIndex);
        }

    }
}
