using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.UI
{
    public class UIMainMenu : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [SerializeField]
        private GameObject CredentialsPanel;
        private MainGameManager GameManagerComponent;
        [SerializeField]
        private Button ButtonStartGame;
        private MenuButtonSoundEffects ButtonStartGameSoundEffects;
        private TextMeshProUGUI TextButtonStartGame;
        [SerializeField]
        private GameObject PanelMainMenu;
        [SerializeField]
        private GameObject PanelMainLobby;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        private bool ReconnectFailed;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnEnable()
        {
            //Other view can be enabled and this script can be not
            //active before connetion is made so to avoid button 
            //showing wrong status it should be set again OnEnable
            ButtonStartGameSetState();
        }

        private void OnDestroy()
        {
            GameManagerComponent.ReconnectFailed -= OnGameManagerComponentReconnectFailed;
        }

        private void Awake()
        {
            TextButtonStartGame = ButtonStartGame.GetComponentInChildren<TextMeshProUGUI>();
            GameManagerComponent = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainGameManager>();
            GameManagerComponent.ReconnectFailed += OnGameManagerComponentReconnectFailed;
            ButtonStartGameSoundEffects = ButtonStartGame.GetComponent<MenuButtonSoundEffects>();
        }

        private void OnGameManagerComponentReconnectFailed()
        {
            InfoWindowComponent.ShowOk("Connection to server failed", null);
            ButtonStartGameStateDisconnected();
            ReconnectFailed = true;
        }

        private void ButtonStartGameSetState()
        {
            //Auto join lobby is enabled
            //Always "connected" while in offline mode
            if (true == PhotonNetwork.insideLobby || true == PhotonNetwork.offlineMode)
            {
                if (false == PlayerInfo.CredentialsCompleted)
                {
                    ButtonStartGameStateMissingCredentials();
                }
                else
                {
                    ButtonStartGameStateConnected();
                }
            }
            else if (false == ReconnectFailed)
            {
                ButtonStartGameStateConnecting();
            }
            else if (true == ReconnectFailed)
            {
                ButtonStartGameStateDisconnected();
            }
        }

        private void ButtonStartGameStateConnecting()
        {
            TextButtonStartGame.text = "Connecting...";
            ButtonStartGame.interactable = false;
        }

        private void ButtonStartGameStateMissingCredentials()
        {
            TextButtonStartGame.text = "Enter credentials";
            ButtonStartGame.onClick.RemoveAllListeners();
            ButtonStartGameSoundEffects.AddSoundEffects();
            ButtonStartGame.onClick.AddListener(() =>
            {
                CredentialsPanel.SetActive(true);
                this.gameObject.SetActive(false);
            });
            ButtonStartGame.interactable = true;
        }

        private void ButtonStartGameStateConnected()
        {
            TextButtonStartGame.text = "Start";
            ButtonStartGame.onClick.RemoveAllListeners();
            ButtonStartGameSoundEffects.AddSoundEffects();
            ButtonStartGame.onClick.AddListener(() =>
            {
                if (true == GameManagerComponent.UseRoom)
                {
                    PanelMainLobby.SetActive(true);
                    PanelMainMenu.SetActive(false);
                }
                else
                {
                    GameManagerComponent.StartGame();
                    ButtonStartGame.interactable = false;
                }
            });
            ButtonStartGame.interactable = true;
        }

        private void ButtonStartGameStateDisconnected()
        {
            TextButtonStartGame.text = "Connect";
            ButtonStartGame.interactable = true;
            ButtonStartGame.onClick.RemoveAllListeners();
            ButtonStartGameSoundEffects.AddSoundEffects();
            ButtonStartGame.onClick.AddListener(() =>
            {
                ReconnectFailed = false;
                GameManagerComponent.Connect();
            });
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

        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            if (true == PlayerInfo.CredentialsCompleted)
            {
                ButtonStartGameStateConnected();
            }
            else
            {
                ButtonStartGameStateMissingCredentials();
            }

            ReconnectFailed = false;
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

            if (false == ReconnectFailed)
            {
                ButtonStartGameStateConnecting();
            }
        }
    }
}
