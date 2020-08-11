using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This script controls custom list view element
    /// </summary>
    [RequireComponent(typeof(LayoutElement))]
    public class ListViewElement : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private Image m_BackgroundImage;
        [SerializeField]
        private Image m_FrontImage;
        [SerializeField]
        private TextMeshProUGUI m_Text;

        /*Public consts fields*/

        /*Public fields*/

        public Image BackgroundImage
        {
            get
            {
                return m_BackgroundImage;
            }

            set
            {
                m_BackgroundImage = value;
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

            set
            {
                m_Text = value;
            }
        }

        //Tried to place here field for generic object that
        //this element is representing but unity does
        //not support generic types as of version 2019.2.1f1

        /*Private methods*/

        /*Public methods*/
    }
}
