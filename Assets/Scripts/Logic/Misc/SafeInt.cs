using UnityEngine;

namespace ITCompanySimulation.Utilities
{
    /// <summary>
    /// This is safe version of int type. It allows to prevent any memory manipulation
    /// by other software
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
                return Get();
            }

            set
            {
                Assign(value);
            }
        }

        /*Private methods*/

        private int Get()
        {
            return m_Value ^ ObscureValue;
        }

        private void Assign(int value)
        {
            if (ObscureValue == default(int))
            {
                ObscureValue = Random.Range(int.MinValue, int.MaxValue);
            }

            m_Value = value;
            m_Value ^= ObscureValue;
        }

        /*Public methods*/

        public SafeInt(int value)
        {
            m_Value = 0;
            ObscureValue = Random.Range(int.MinValue, int.MaxValue);
            Assign(value);
        }
    }
}
