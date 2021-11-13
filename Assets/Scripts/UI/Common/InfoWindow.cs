using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class control info window that can provide some information to player.
    /// It contains text and confirmation button.
    /// </summary>
    public class InfoWindow : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private TextMeshProUGUI m_Text;
        [SerializeField]
        private Button ConfirmationButton;
        [SerializeField]
        private Button CancelButton;
        /// <summary>
        /// Invoked when player clicks "Ok" button in this window.
        /// </summary>
        private UnityAction ConfirmButtonClicked;
        /// <summary>
        /// Invoked when player clicks "Cancel" button in this window.
        /// </summary>
        private UnityAction CancelButtonClicked;
        /// <summary>
        /// Info window actions will be queued here in case showing window is called when info window is already visible.
        /// </summary>
        private Queue<InfoWindowData> InfoWindowActions = new Queue<InfoWindowData>();
        private struct InfoWindowData
        {
            public string Text;
            public UnityAction OnConfirmAction;
            public UnityAction OnCancelAction;
            public InfoWindowType Type;
        }
        private enum InfoWindowType
        {
            Text,
            Ok,
            OkCancel
        }

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

        private void OnConfirmButtonClicked()
        {
            ConfirmButtonClicked?.Invoke();
        }

        private void OnCancelButtonClicked()
        {
            CancelButtonClicked?.Invoke();
        }

        private void Awake()
        {
            ConfirmationButton.onClick.AddListener(OnConfirmButtonClicked);
            ConfirmationButton.onClick.AddListener(Hide);
            CancelButton.onClick.AddListener(OnCancelButtonClicked);
            CancelButton.onClick.AddListener(Hide);
        }

        private void Show(string text, UnityAction onConfirmAction, UnityAction onCancelAction, InfoWindowType type)
        {
            if (true == gameObject.GetActive())
            {
                //Info window is already active and is displaying other data,
                //store data so it can be displayed after user closes window

                InfoWindowData data = new InfoWindowData();
                data.Text = text;
                data.OnConfirmAction = onConfirmAction;
                data.OnCancelAction = onCancelAction;
                data.Type = type;

                InfoWindowActions.Enqueue(data);
            }
            else
            {
                switch (type)
                {
                    case InfoWindowType.Text:
                        CancelButton.gameObject.SetActive(false);
                        ConfirmationButton.gameObject.SetActive(false);
                        break;
                    case InfoWindowType.Ok:
                        CancelButton.gameObject.SetActive(false);
                        ConfirmationButton.gameObject.SetActive(true);
                        break;
                    case InfoWindowType.OkCancel:
                        CancelButton.gameObject.SetActive(true);
                        ConfirmationButton.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }

                this.Text = text;
                ConfirmButtonClicked = onConfirmAction;
                CancelButtonClicked = onCancelAction;
                gameObject.SetActive(true);
            }
        }

        /*Public methods*/

        /// <summary>
        /// Makes info window visible with text only. If used when info window is already visible, info window data
        /// will be queued and displayed when previous window is closed.
        /// </summary>
        /// <param name="text">Text displayed in this window</param>
        public void Show(string text)
        {
            Show(text, null, null, InfoWindowType.Text);
        }

        /// <summary>
        /// Makes info window visible with "Ok" button. If used when info window is already visible, info window data
        /// will be queued and displayed when previous window is closed.
        /// </summary>
        /// <param name="onConfirmAction">Method invoked after player has pressed confirmation button.
        /// If it is null nothing happens</param>
        /// <param name="text">Text displayed in this window</param>
        /// <param name="onConfirmAction">Event invoked when "Ok" button is pressed</param>
        public void ShowOk(string text, UnityAction onConfirmAction)
        {
            Show(text, onConfirmAction, null, InfoWindowType.Ok);
        }

        /// <summary>
        /// Makes info window visible with "Ok" and "Cancel" button. If used when info window is already visible, info window data
        /// will be queued and displayed when previous window is closed.
        /// </summary>
        /// <param name="text">Text displayed in this window</param>
        /// <param name="onConfirmAction">Event invoked when "Ok" button is pressed</param>
        /// <param name="onCancelAction">Event invoked when "Cancel" button is pressed</param>
        public void ShowOkCancel(string text, UnityAction onConfirmAction, UnityAction onCancelAction)
        {
            Show(text, onConfirmAction, onCancelAction, InfoWindowType.OkCancel);
        }

        /// <summary>
        /// Makes info window not visible
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);

            if (InfoWindowActions.Count > 0)
            {
                //Show next data
                InfoWindowData data = InfoWindowActions.Dequeue();
                Show(data.Text, data.OnConfirmAction, data.OnCancelAction, data.Type);
            }
        }
    }
}
