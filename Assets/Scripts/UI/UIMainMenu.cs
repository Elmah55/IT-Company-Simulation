using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMainMenu : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private GameObject CredentialsPanel;
    [SerializeField]
    private MainGameManager GameManagerComponent;
    [SerializeField]
    private Button ButtonStartGame;
    private TextMeshProUGUI TextButtonStartGame;
    [SerializeField]
    private GameObject UIObjectMainMenuPanel;
    [SerializeField]
    private GameObject UIObjectMultiplayer;
    private PlayerInfo PlayerInfoComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void OnEnable()
    {
        if (null == TextButtonStartGame)
        {
            TextButtonStartGame = ButtonStartGame.GetComponentInChildren<TextMeshProUGUI>();
        }

        //Other view can be enabled and this script can be not
        //active before connetion is made so to avoid button 
        //showing wrong status it should be set again OnEnable
        ButtonStartGameSetState();
    }

    private void Start()
    {
        PlayerInfoComponent = GameManagerComponent.GetComponent<PlayerInfo>();
        ButtonStartGameSetState();
    }

    private void ButtonStartGameSetState()
    {
        //Auto join lobby is enabled
        //Always "connected" while in offline mode
        if (true == PhotonNetwork.insideLobby || true == PhotonNetwork.offlineMode)
        {
            if (false == PlayerInfoComponent.CredentialsCompleted)
            {
                ButtonStartGameStateMissingCredentials();
            }
            else
            {
                ButtonStartGameStateConnected();
            }
        }
        else
        {
            ButtonStartGameStateConnecting();
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
        ButtonStartGame.onClick.AddListener(() =>
        {
            if (true == GameManagerComponent.UseRoom)
            {
                UIObjectMultiplayer.SetActive(true);
                UIObjectMainMenuPanel.SetActive(false);
            }
            else
            {
                GameManagerComponent.StartGame();
            }
        });
        ButtonStartGame.interactable = true;
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

        if (true == PlayerInfoComponent.CredentialsCompleted)
        {
            ButtonStartGameStateConnected();
        }
        else
        {
            ButtonStartGameStateMissingCredentials();
        }
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();

        ButtonStartGameStateConnecting();
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        base.OnConnectionFail(cause);

        ButtonStartGameStateConnecting();
    }

}
