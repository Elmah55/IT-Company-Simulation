using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using ITCompanySimulation.Utilities;

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
        /// Game object that contains window's content.
        /// </summary>
        [SerializeField]
        private GameObject InfoWindowContent;
        /// <summary>
        /// Game object that contains window's buttons.
        /// </summary>
        [SerializeField]
        private GameObject ButtonsGameObject;
        /// <summary>
        /// Invoked when player clicks "Ok" button in this window.
        /// </summary>
        private UnityAction ConfirmButtonClicked;
        /// <summary>
        /// Invoked when player clicks "Cancel" button in this window.
        /// </summary>
        private UnityAction CancelButtonClicked;
        /// <summary>
        /// Info window messages will be queued here in case showing window method is called when info window
        /// is already visible and is showing current message.
        /// </summary>
        private Queue<InfoWindowData> InfoWindowPendingMessages = new Queue<InfoWindowData>();
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

            private set
            {
                m_Text.text = value;
            }
        }

        //Since only one window can be displayed at a time
        //make this a singleton.
        public static InfoWindow Instance { get; private set; }

        /*Private methods*/

        private void OnConfirmButtonClicked()
        {
            ConfirmButtonClicked?.Invoke();
            Hide();
        }

        private void OnCancelButtonClicked()
        {
            CancelButtonClicked?.Invoke();
            Hide();
        }

        private void Awake()
        {
            if (null != Instance)
            {
                string debugInfo = string.Format("Only one instance of {0} should exist but is instantiated multiple times",
                    this.GetType().Name);
                RestrictedDebug.Log(debugInfo, LogType.Error);
            }

            Instance = this;

            ConfirmationButton.onClick.AddListener(OnConfirmButtonClicked);
            CancelButton.onClick.AddListener(OnCancelButtonClicked);
            //Disable all buttons
            Hide();
        }

        private void Show(string text, UnityAction onConfirmAction, UnityAction onCancelAction, InfoWindowType type)
        {
            if (true == InfoWindowContent.GetActive())
            {
                //Info window is already active and is displaying other data,
                //store data so it can be displayed after user closes window

                InfoWindowData data = new InfoWindowData();
                data.Text = text;
                data.OnConfirmAction = onConfirmAction;
                data.OnCancelAction = onCancelAction;
                data.Type = type;

                InfoWindowPendingMessages.Enqueue(data);
            }
            else
            {
                switch (type)
                {
                    case InfoWindowType.Ok:
                        ButtonsGameObject.SetActive(true);
                        ConfirmationButton.gameObject.SetActive(true);
                        break;
                    case InfoWindowType.OkCancel:
                        ButtonsGameObject.SetActive(true);
                        CancelButton.gameObject.SetActive(true);
                        ConfirmationButton.gameObject.SetActive(true);
                        break;
                    default:
                        break;
                }

                this.Text = text;
                ConfirmButtonClicked = onConfirmAction;
                CancelButtonClicked = onCancelAction;
                InfoWindowContent.SetActive(true);
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
        public void ShowOk(string text, UnityAction onConfirmAction = null)
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
        public void ShowOkCancel(string text, UnityAction onConfirmAction = null, UnityAction onCancelAction = null)
        {
            Show(text, onConfirmAction, onCancelAction, InfoWindowType.OkCancel);
        }

        /// <summary>
        /// Makes info window not visible.
        /// </summary>
        public void Hide()
        {
            InfoWindowContent.SetActive(false);
            ButtonsGameObject.SetActive(false);
            CancelButton.gameObject.SetActive(false);
            ConfirmationButton.gameObject.SetActive(false);

            if (InfoWindowPendingMessages.Count > 0)
            {
                //Show next data
                InfoWindowData data = InfoWindowPendingMessages.Dequeue();
                Show(data.Text, data.OnConfirmAction, data.OnCancelAction, data.Type);
            }
        }

        /// <summary>
        /// Removes currently displayed message all messages waiting to be shown in info window
        /// then hides window.
        /// </summary>
        public void RemoveAllMessages()
        {
            InfoWindowPendingMessages.Clear();
            Hide();
        }
    }
}
