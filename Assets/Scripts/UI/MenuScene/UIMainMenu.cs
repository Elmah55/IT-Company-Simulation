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
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        [SerializeField]
        private VerticalLayoutGroup LayoutMenuButtons;
        private bool ReconnectFailed;
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

        private void OnDestroy()
        {
            ApplicationManagerComponent.ReconnectFailed -= OnGameManagerComponentReconnectFailed;
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

            ApplicationManagerComponent = GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
            ApplicationManagerComponent.ReconnectFailed += OnGameManagerComponentReconnectFailed;
            ApplicationManagerComponent.ServerConnectionEstablished += OnServerConnectionEstablished;
            ApplicationManagerComponent.DisconnectedFromServer += OnServerDisconnected;

            ButtonStartGame.onClick.AddListener(ApplicationManagerComponent.StartGame);
            ButtonConnect.onClick.AddListener(() =>
            {
                ApplicationManagerComponent.Connect();
                ReconnectFailed = false;
            });
        }

        private void OnGameManagerComponentReconnectFailed()
        {
            InfoWindowComponent.ShowOk("Connection to server failed", null);
            ActivateStateButton(ApplicationState.Disconnected);
            ReconnectFailed = true;
        }

        private void SetApplicationStateButton()
        {
            if (false == PlayerInfoSettings.CredentialsCompleted)
            {
                ActivateStateButton(ApplicationState.MissingCredentials);
            }
            //Auto join lobby is enabled
            //Always "connected" while in offline mode
            else if (true == ApplicationManagerComponent.Connected)
            {
                ActivateStateButton(ApplicationState.Connected);
            }
            else if (false == ReconnectFailed)
            {
                ActivateStateButton(ApplicationState.Connecting);
            }
            else if (true == ReconnectFailed)
            {
                ActivateStateButton(ApplicationState.Disconnected);
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

            if (true == LayoutMenuButtons.gameObject.activeInHierarchy)
            {
                StartCoroutine(UpdateMenuButtonsLayout());
            }
        }

        /// <summary>
        /// This method is a workaround for glitched layout group.
        /// When one menu button is disabled and another one activated
        /// layout does not calculate position of activated button correctly.
        /// It happens only the first time button is activated. Calling
        /// LayoutRebuilder.ForceRebuildLayoutImmediate does not help.
        /// </summary>
        private IEnumerator UpdateMenuButtonsLayout()
        {
            LayoutMenuButtons.enabled = false;
            yield return null;
            LayoutMenuButtons.enabled = true;
        }

        private void OnServerConnectionEstablished()
        {
            SetApplicationStateButton();
            ReconnectFailed = false;
        }

        private void OnServerDisconnected()
        {
            SetApplicationStateButton();
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
