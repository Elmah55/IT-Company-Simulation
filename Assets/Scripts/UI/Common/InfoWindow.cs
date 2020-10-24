using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class control info window that can provide some information to player.
    /// It contains text and confirmation button
    /// </summary>
    public class InfoWindow : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private TextMeshProUGUI m_Text;
        [SerializeField]
        private Button ConfirmationButton;
        /// <summary>
        /// Invoked when player clicks button in this window
        private UnityAction ConfirmationButtonClicked;

        /*Public consts fields*/

        /*Public fields*/

        public string Text
        {
            get
            {
                return m_Text.text;
            }

            set
            {
                m_Text.text = value;
            }
        }

        /*Private methods*/

        private void OnConfirmationButtonClicked()
        {
            ConfirmationButtonClicked?.Invoke();
        }

        private void Awake()
        {
            ConfirmationButton.onClick.AddListener(OnConfirmationButtonClicked);
        }

        /*Public methods*/

        /// <summary>
        /// Makes info window visible
        /// </summary>
        public void Show()
        {
            Show(null);
        }

        /// <summary>
        /// Makes info window visible
        /// </summary>
        /// <param name="onConfirmAction"> Method invoked after player has pressed confirmation button.
        /// If it is null nothing happens</param>
        public void Show(UnityAction onConfirmAction)
        {
            ConfirmationButtonClicked = onConfirmAction;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Makes info window visible
        /// </summary>
        /// <param name="onConfirmAction">Method invoked after player has pressed confirmation button.
        /// If it is null nothing happens</param>
        /// <param name="text">Text displayed in this window</param>
        public void Show(string text, UnityAction onConfirmAction)
        {
            this.Text = text;
            ConfirmationButtonClicked = onConfirmAction;
            gameObject.SetActive(true);
        }

        /// <summary>
        /// Makes info window not visible
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
