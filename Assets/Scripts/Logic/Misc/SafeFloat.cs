using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This is safe version of float type. It allows to prevent any memory manipulation
    /// by other software
    /// </summary>
    public struct SafeFloat
    {
        /*Private consts fields*/

        /*Private fields*/

        private float m_Value;
        private int ObscureValue;

        /*Public consts fields*/

        /*Public fields*/

        public float Value
        {
            get
            {
                return Get();
            }

            set
            {
                Assign(value);
            }
        }

        /*Private methods*/

        private unsafe float Get()
        {
            fixed (float* ptr = &m_Value)
            {
                int* intPtr = (int*)ptr;
                (*intPtr) = (*intPtr) ^ ObscureValue;
                return (*ptr);
            }
        }

        private unsafe void Assign(float value)
        {
            if (ObscureValue == default(int))
            {
                ObscureValue = Random.Range(int.MinValue, int.MaxValue);
            }

            m_Value = value;

            fixed (float* ptr = &m_Value)
            {
                int* intPtr = (int*)ptr;
                (*intPtr) = (*intPtr) ^ ObscureValue;
            }
        }

        /*Public methods*/

        public SafeFloat(float value)
        {
            m_Value = 0.0f;
            ObscureValue = Random.Range(int.MinValue, int.MaxValue);
            Assign(value);
        }
    }
}
