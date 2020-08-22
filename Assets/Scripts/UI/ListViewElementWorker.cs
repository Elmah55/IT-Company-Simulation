using ITCompanySimulation.Character;
using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    public class ListViewElementWorker : ListViewElement
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
        public SharedWorker Worker { get; set; }

        //Tried to place here field for generic object that
        //this element is representing but unity does
        //not support generic types as of version 2019.2.1f1

        /*Private methods*/

        /*Public methods*/
    }
}
