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
        [SerializeField]
        private Button m_Button;

        /*Public consts fields*/

        /*Public fields*/

        public Image BackgroundImage
        {
            get
            {
                return m_BackgroundImage;
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

        /*Private methods*/

        /*Public methods*/
    } 
}
