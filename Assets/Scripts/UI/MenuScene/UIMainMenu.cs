using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Core;
using ITCompanySimulation.Settings;
using System.Collections;

namespace ITCompanySimulation.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private GameObject CredentialsPanel;
        private ApplicationManager ApplicationManagerComponent;
        [SerializeField]
        private Button ButtonStartGame;
        [SerializeField]
        private Button ButtonEnterCredentials;
        [SerializeField]
        private Button ButtonConnecting;
        [SerializeField]
        private Button ButtonConnect;
        [SerializeField]
        private GameObject PanelMainMenu;
        [SerializeField]
        private GameObject PanelMainLobby;
        private InfoWindow InfoWindowComponent;
        [SerializeField]
        private VerticalLayoutGroup LayoutMenuButtons;
        /// <summary>
        /// Array with buttons that must be activated or deactived
        /// depending on current application state.
        /// </summary>
        private Button[] ApplicationStateButtons;
        private enum ApplicationState
        {
            MissingCredentials,
            Connecting,
            Connected,
            Disconnected
        }

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnEnable()
        {
            //Other view can be enabled and this script can be not
            //active before connetion is made so to avoid button 
            //showing wrong state it should be set again OnEnable
            SetApplicationStateButton();
        }

        private void Awake()
        {
            ApplicationStateButtons = new Button[]
            {
                ButtonStartGame,
                ButtonConnect,
                ButtonConnecting,
                ButtonEnterCredentials
            };

            InfoWindowComponent = InfoWindow.Instance;
            ApplicationManagerComponent = GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
            ApplicationManagerComponent.DisconnectedFromServer += OnDisconnectedFromServer;
            ApplicationManagerComponent.ConnectedToServer += OnConnectedToSever;
            ButtonStartGame.onClick.AddListener(() =>
            {
                if (true == ApplicationManagerComponent.UseRoom)
                {
                    PanelMainMenu.gameObject.SetActive(false);
                    PanelMainLobby.gameObject.SetActive(true);
                }
                else
                {
                    ApplicationManagerComponent.StartGame();
                }
            });
            ButtonConnect.onClick.AddListener(() =>
            {
                ApplicationManagerComponent.Connect();
                SetApplicationStateButton();
            });
        }

        private void OnDestroy()
        {
            ApplicationManagerComponent.DisconnectedFromServer -= OnDisconnectedFromServer;
            ApplicationManagerComponent.ConnectedToServer -= OnConnectedToSever;
        }

        private void OnConnectedToSever()
        {
            SetApplicationStateButton();
        }

        private void OnDisconnectedFromServer()
        {
            InfoWindowComponent.ShowOk("Connection to server failed", null);
            SetApplicationStateButton();
        }

        private void SetApplicationStateButton()
        {
            if (false == PlayerInfoSettings.CredentialsCompleted)
            {
                ActivateStateButton(ApplicationState.MissingCredentials);
            }
            else
            {
                switch (PhotonNetwork.connectionState)
                {
                    case ConnectionState.Disconnected:
                        ActivateStateButton(ApplicationState.Disconnected);
                        break;
                    case ConnectionState.Connecting:
                    case ConnectionState.InitializingApplication:
                        ActivateStateButton(ApplicationState.Connecting);
                        break;
                    case ConnectionState.Connected:
                        ActivateStateButton(ApplicationState.Connected);
                        break;
                    default:
                        ActivateStateButton(ApplicationState.Disconnected);
                        break;
                }
            }
        }

        private void ActivateStateButton(ApplicationState state)
        {
            foreach (Button stateButton in ApplicationStateButtons)
            {
                stateButton.gameObject.SetActive(false);
            }

            Button currentStateButton = null;

            switch (state)
            {
                case ApplicationState.MissingCredentials:
                    currentStateButton = ButtonEnterCredentials;
                    break;
                case ApplicationState.Connecting:
                    currentStateButton = ButtonConnecting;
                    break;
                case ApplicationState.Connected:
                    currentStateButton = ButtonStartGame;
                    break;
                case ApplicationState.Disconnected:
                    currentStateButton = ButtonConnect;
                    break;
                default:
                    currentStateButton = ButtonConnect;
                    break;
            }

            currentStateButton.gameObject.SetActive(true);
        }

        /*Public methods*/

        public void OnButtonExitClicked()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
    }
}
