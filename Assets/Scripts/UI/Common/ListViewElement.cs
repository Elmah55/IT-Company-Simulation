using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    public class ListViewElement : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        protected Image m_BackgroundImage;
        [SerializeField]
        protected Image m_FrontImage;
        [SerializeField]
        protected TextMeshProUGUI m_Text;
        [SerializeField]
        protected Button m_Button;

        /*Public consts fields*/

        /*Public fields*/

        public Image BackgroundImage
        {
            get
            {
                return m_BackgroundImage;
            }
        }
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
        public TextMeshProUGUI Text
        {
            get
            {
                return m_Text;
            }
        }
        public Button Button
        {
            get
            {
                return m_Button;
            }
        }
        /// <summary>
        /// Object that this list view element represents.
        /// </summary>
        public object RepresentedObject { get; set; }

        /*Private methods*/

        /*Public methods*/
    } 
}
