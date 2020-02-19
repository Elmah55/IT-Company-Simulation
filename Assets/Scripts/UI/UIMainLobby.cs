using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Class handling UI of main multiplayer lobby
/// </summary>
public class UIMainLobby : Photon.PunBehaviour
{
    /*Private consts fields*/

    private readonly Color SELECTED_ROOM_BUTTON_COLOR = Color.gray;

    /*Private fields*/

    /// <summary>
    /// Maps room info to coresponding button in lobby rooms list view
    /// </summary>
    private Dictionary<Button, RoomInfo> ButtonRoomInfoMap = new Dictionary<Button, RoomInfo>();
    [SerializeField]
    private GameObject PanelRoomLobby;
    private IButtonSelector RoomButtonSelector = new ButtonSelector();
    /// <summary>
    /// This list view will hold buttons for
    /// joining rooms in multiplayer lobby
    /// </summary>
    [SerializeField]
    public ControlListView LobbyRoomsButtonListView;
    /// <summary>
    /// Prefab to be used in rooms list view
    /// </summary>
    [SerializeField]
    public Button LobbyRoomButtonPrefab;
    [SerializeField]
    public Button JoinRoomButton;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void AddLobbbyRoomsButtons()
    {
        foreach (RoomInfo lobbyRoomInfo in PhotonNetwork.GetRoomList())
        {
            Button buttonComponent = GameObject.Instantiate<Button>(LobbyRoomButtonPrefab);
            Text textComponent = buttonComponent.GetComponentInChildren<Text>();

            string roomStatusText = lobbyRoomInfo.IsOpen ? "In lobby" : "In progress";
            string buttonText = string.Format("{0} {1}/{2} ({3})",
                                            lobbyRoomInfo.Name,
                                            lobbyRoomInfo.PlayerCount,
                                            lobbyRoomInfo.MaxPlayers,
                                            roomStatusText);
            textComponent.text = buttonText;

            ButtonRoomInfoMap.Add(buttonComponent, lobbyRoomInfo);
            LobbyRoomsButtonListView.AddControl(buttonComponent.gameObject);
            RoomButtonSelector.AddButton(buttonComponent);

            if (false == lobbyRoomInfo.IsOpen)
            {
                buttonComponent.interactable = false;
            }
        }
    }

    private void OnEnable()
    {
        RefreshRoomList();
    }

    /*Public methods*/

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        RefreshRoomList();
    }

    public void OnJoinRoomButtonClicked()
    {
        RoomInfo selectedRoomInfo = ButtonRoomInfoMap[RoomButtonSelector.GetSelectedButton()];
        if (true == PhotonNetwork.JoinRoom(selectedRoomInfo.Name))
        {
            PanelRoomLobby.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }

    public void RefreshRoomList()
    {
        ButtonRoomInfoMap.Clear();
        LobbyRoomsButtonListView.RemoveAllControls();
        AddLobbbyRoomsButtons();
        JoinRoomButton.interactable = (0 != PhotonNetwork.GetRoomList().Length);
    }
}
