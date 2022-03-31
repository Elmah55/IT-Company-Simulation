using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This is safe version of int type. It allows to prevent any memory manipulation
    /// by other software.
    /// </summary>
    public struct SafeInt
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_Value;
        private int ObscureValue;

        /*Public consts fields*/

        /*Public fields*/

        public int Value
        {
            get
            {
                return GetValue();
            }
        }

        /*Private methods*/

        private int GetValue()
        {
            return m_Value ^ ObscureValue;
        }

        private void SetValue(int value)
        {
            m_Value = value ^ ObscureValue;
        }

        /*Public methods*/

        public SafeInt(int value)
        {
            m_Value = 0;
            ObscureValue = Random.Range(1, int.MaxValue);
            SetValue(value);
        }
    }
}
