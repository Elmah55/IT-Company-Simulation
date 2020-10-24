using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    public class ListViewElementImage : ListViewElement
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private Image m_FrontImage;

        /*Public consts fields*/

        /*Public fields*/

        public Image FrontImage
        {
            get
            {
                return m_FrontImage;
            }

            set
            {
                m_FrontImage = value;
            }
        }

        /*Private methods*/

        /*Public methods*/
    }
}
