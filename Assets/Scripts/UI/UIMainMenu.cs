using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private MainGameManager GameManagerComponent;
    [SerializeField]
    private Button ButtonStartGame;
    [SerializeField]
    private Text TextButtonStartGame;
    [SerializeField]
    private GameObject UIMultiplayer;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        if (false == PhotonNetwork.offlineMode)
        {
            TextButtonStartGame.text = "Connecting...";
            ButtonStartGame.interactable = false;
        }
        else
        {
            TextButtonStartGame.text = "Start";
            ButtonStartGame.interactable = true;
        }
    }

    /*Public methods*/

    public void OnButtonStartGameClicked()
    {
        if (true == GameManagerComponent.UseRoom)
        {
            UIMultiplayer.SetActive(true);
            this.gameObject.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            GameManagerComponent.StartGame();
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        if (false == PhotonNetwork.offlineMode)
        {
            TextButtonStartGame.text = "Start";
            ButtonStartGame.interactable = true;
        }
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();

        if (false == PhotonNetwork.offlineMode)
        {
            TextButtonStartGame.text = "Connecting...";
            ButtonStartGame.interactable = false;
        }
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        base.OnConnectionFail(cause);

        if (false == PhotonNetwork.offlineMode)
        {
            TextButtonStartGame.text = "Connecting...";
            ButtonStartGame.interactable = false;
        }
    }

}
