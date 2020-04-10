using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomLobby : Photon.PunBehaviour
{
    /*Private consts fields*/

    //private readonly ColorBlock READY_PLAYER_BUTTON_COLORS =
    //    new ColorBlock { normalColor = Color.green, selectedColor = Color.green };
    //private readonly ColorBlock NOT_READY_PLAYER_BUTTON_COLORS =
    //    new ColorBlock { normalColor = Color.red, selectedColor = Color.red };

    /*Private fields*/

    private Dictionary<PhotonPlayer, Button> PlayerButtonMap = new Dictionary<PhotonPlayer, Button>();
    /// <summary>
    /// Reference to master client used to update button text when new client becomes master client
    /// </summary>
    private PhotonPlayer CurrentMasterClient;
    [SerializeField]
    private Button ButtonStartGame;
    [SerializeField]
    private Button RoomPlayerListViewButtonPrefab;
    /// <summary>
    /// Will hold buttons displaying info about players in
    /// current room
    /// </summary>
    [SerializeField]
    private ControlListView RoomPlayersButtonsListView;
    [SerializeField]
    private MainGameManager GameManagerComponent;
    [SerializeField]
    private GameObject PanelMainLobby;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/
    private void OnEnable()
    {
        AddRoomPlayersButtons();
        CurrentMasterClient = PhotonNetwork.masterClient;

        if (true == PhotonNetwork.isMasterClient)
        {
            ButtonStartGame.interactable = true;
        }
    }

    private void OnDisable()
    {
        PlayerButtonMap.Clear();
        RoomPlayersButtonsListView.RemoveAllControls();
    }

    private void AddRoomPlayersButtons()
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            AddRoomPlayerButton(player);
        }
    }

    /// <summary>
    /// Returns button's text of single player in a room lobby. Text contains
    /// player's nickname and role in a room lobby
    /// </summary>
    private string CreatePlayerButtonText(PhotonPlayer player)
    {
        string buttonText = player.NickName + " ";

        if (true == player.IsLocal)
        {
            buttonText += "(You) ";
        }

        if (true == player.IsMasterClient)
        {
            buttonText += "(Room master) ";
        }

        return buttonText;
    }

    private void AddRoomPlayerButton(PhotonPlayer player)
    {
        Button newRoomPlayerButton = GameObject.Instantiate<Button>(RoomPlayerListViewButtonPrefab);
        Button buttonComponent = newRoomPlayerButton.GetComponent<Button>();
        Text textComponent = buttonComponent.GetComponentInChildren<Text>();

        PlayerButtonMap.Add(player, newRoomPlayerButton);
        RoomPlayersButtonsListView.AddControl(newRoomPlayerButton.gameObject);

        string buttonText = CreatePlayerButtonText(player);
        textComponent.text = buttonText;

        //TODO: Add player ready / not ready state
        //Currently only one player property will be kept so 0 will be used to access it
        //RoomLobbyPlayerState playerState = (RoomLobbyPlayerState)player.CustomProperties[0];

        //switch (playerState)
        //{
        //    case RoomLobbyPlayerState.Ready:
        //        buttonComponent.colors = READY_PLAYER_BUTTON_COLORS;
        //        buttonText += "Ready";
        //        break;
        //    case RoomLobbyPlayerState.NotReady:
        //        buttonComponent.colors = NOT_READY_PLAYER_BUTTON_COLORS;
        //        buttonText += "Not ready";
        //        break;
        //    default:
        //        break;
        //}
    }

    /*Public methods*/

    public void OnButtonStartGameClicked()
    {
        ButtonStartGame.interactable = false;
        GameManagerComponent.StartGame();
    }

    public void OnButtonLeaveRoomClicked()
    {
        PhotonNetwork.LeaveRoom();
        PanelMainLobby.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        AddRoomPlayerButton(newPlayer);

        //At least two players are needed to start game and only master client can start game
        if (1 < PhotonNetwork.playerList.Length && true == PhotonNetwork.isMasterClient)
        {
            ButtonStartGame.interactable = true;
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        Button playerButton = PlayerButtonMap[otherPlayer];
        PlayerButtonMap.Remove(otherPlayer);
        RoomPlayersButtonsListView.RemoveControl(playerButton.gameObject);

        //At least two players are needed to start game and only master client can start game
        if (1 == PhotonNetwork.playerList.Length && true == PhotonNetwork.isMasterClient)
        {
            ButtonStartGame.interactable = false;
        }
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        Button oldMasterClientButton = PlayerButtonMap[CurrentMasterClient];
        Text textComponent = oldMasterClientButton.GetComponentInChildren<Text>();
        textComponent.text = CreatePlayerButtonText(CurrentMasterClient);

        Button masterClientButton = PlayerButtonMap[newMasterClient];
        textComponent = masterClientButton.GetComponentInChildren<Text>();
        textComponent.text += CreatePlayerButtonText(newMasterClient);

        if (true == PhotonNetwork.isMasterClient)
        {
            ButtonStartGame.interactable = true;
        }

        CurrentMasterClient = PhotonNetwork.masterClient;
    }
}