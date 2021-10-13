using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    public class ListViewElementImage : ListViewElement
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private Image m_FrontImagee;

        /*Public consts fields*/

        /*Public fields*/

        public Image FrontImage
        {
            get
            {
                return m_FrontImagee;
            }

            set
            {
                m_FrontImagee = value;
            }
        }

        /*Private methods*/

        /*Public methods*/
    }
}
