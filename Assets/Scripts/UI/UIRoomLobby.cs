using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIRoomLobby : Photon.PunBehaviour
{
    /*Private consts fields*/

    private readonly ColorBlock READY_PLAYER_BUTTON_COLORS =
        new ColorBlock { normalColor = Color.green, selectedColor = Color.green };
    private readonly ColorBlock NOT_READY_PLAYER_BUTTON_COLORS =
        new ColorBlock { normalColor = Color.red, selectedColor = Color.red };

    /*Private fields*/

    private Dictionary<PhotonPlayer, GameObject> PlayerButtonMap;
    [SerializeField]
    private Button ButtonStartGame;

    /*Public consts fields*/

    /*Public fields*/

    public GameObject RoomPlayerButtonPrefab;
    /// <summary>
    /// Will hold buttons displaying info about players in
    /// current room
    /// </summary>
    public ControlListView RoomPlayersButtonsListView;
    public MainGameManager GameManagerComponent;

    /*Private methods*/

    private void AddRoomPlayersButtons()
    {
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            AddRoomPlayerButton(player);
        }
    }

    private void AddRoomPlayerButton(PhotonPlayer player)
    {
        GameObject newRoomPlayerButton = GameObject.Instantiate(RoomPlayerButtonPrefab);
        Button buttonComponent = newRoomPlayerButton.GetComponent<Button>();
        Text textComponent = buttonComponent.GetComponentInChildren<Text>();

        PlayerButtonMap.Add(player, newRoomPlayerButton);
        RoomPlayersButtonsListView.AddControl(newRoomPlayerButton);

        string buttonText = player.NickName;

        if (true == player.IsLocal)
        {
            buttonText += " (You) ";
        }
        else if (true == player.IsMasterClient)

        {
            buttonText += " (Room master) ";
        }

        textComponent.text = buttonText;

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

    private void Start()
    {
        PlayerButtonMap = new Dictionary<PhotonPlayer, GameObject>();
    }

    /*Public methods*/

    public void OnStartGameButtonClicked()
    {
        GameManagerComponent.StartGame();
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        AddRoomPlayerButton(newPlayer);
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        GameObject playerButton = PlayerButtonMap[otherPlayer];
        PlayerButtonMap.Remove(otherPlayer);
        RoomPlayersButtonsListView.RemoveControl(playerButton);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        AddRoomPlayersButtons();
        ButtonStartGame.interactable = PhotonNetwork.isMasterClient;
    }
}