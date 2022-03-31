using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This is safe version of float type. It allows to prevent any memory manipulation
    /// by other software.
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
                return GetValue();
            }
        }

        /*Private methods*/

        private unsafe float GetValue()
        {
            float tmpValue = m_Value;
            int* intPtr = (int*)&tmpValue;
            *intPtr = *intPtr ^ ObscureValue;
            return tmpValue;
        }

        private unsafe void SetValue(float value)
        {
            int* intPtr = (int*)&value;
            (*intPtr) = (*intPtr) ^ ObscureValue;
            m_Value = value;
        }

        /*Public methods*/

        public SafeFloat(float value)
        {
            m_Value = 0f;
            ObscureValue = Random.Range(1, int.MaxValue);
            SetValue(value);
        }
    }
}
