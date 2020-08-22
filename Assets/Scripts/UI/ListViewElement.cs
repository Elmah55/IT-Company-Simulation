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
        private Image m_BackgroundImage;
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

        /*Private methods*/

        /*Public methods*/
    } 
}
