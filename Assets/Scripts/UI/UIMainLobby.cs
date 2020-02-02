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
    private Dictionary<GameObject, RoomInfo> ButtonRoomInfoMap;
    private GameObject SelectedRoomButton;
    /// <summary>
    /// Used to restore colors of button when its no
    /// longer selected
    /// </summary>
    private ColorBlock SelectedRoomButtonColors;
    [SerializeField]
    private GameObject PanelRoomLobby;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// This list view will hold buttons for
    /// joining rooms in multiplayer lobby
    /// </summary>
    public UIControlListView LobbyRoomsButtonListView;
    /// <summary>
    /// Prefab to be used in rooms list view
    /// </summary>
    public GameObject LobbyRoomButtonPrefab;
    public Button JoinRoomButton;

    /*Private methods*/

    private void OnRoomListViewButtonClicked()
    {
        Button buttonComponent;

        if (null != SelectedRoomButton)
        {
            //Restore colors to previously selected button
            buttonComponent = SelectedRoomButton.GetComponent<Button>();
            buttonComponent.colors = SelectedRoomButtonColors;
        }

        //We know its project list view button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;
        buttonComponent = selectedButton.GetComponent<Button>();
        SelectedRoomButtonColors = buttonComponent.colors;

        ColorBlock newButtonColor = new ColorBlock();
        newButtonColor.selectedColor = SELECTED_ROOM_BUTTON_COLOR;
        newButtonColor.normalColor = SELECTED_ROOM_BUTTON_COLOR;

        buttonComponent.colors = newButtonColor;
        this.SelectedRoomButton = selectedButton;
    }

    private void AddLobbbyRoomsButtons()
    {
        foreach (RoomInfo lobbyRoomInfo in PhotonNetwork.GetRoomList())
        {
            GameObject lobbyRoomButton = GameObject.Instantiate(LobbyRoomButtonPrefab);
            Button buttonComponent = lobbyRoomButton.GetComponent<Button>();
            buttonComponent.onClick.AddListener(OnRoomListViewButtonClicked);
            Text textComponent = buttonComponent.GetComponentInChildren<Text>();

            string roomStatusText = lobbyRoomInfo.IsOpen ? "In lobby" : "In progress";
            string buttonText = string.Format("{0} {1}/{2} ({3})",
                                            lobbyRoomInfo.Name,
                                            lobbyRoomInfo.PlayerCount,
                                            lobbyRoomInfo.MaxPlayers,
                                            roomStatusText);
            textComponent.text = buttonText;

            ButtonRoomInfoMap.Add(lobbyRoomButton, lobbyRoomInfo);
            LobbyRoomsButtonListView.AddControl(lobbyRoomButton);

            if (false == lobbyRoomInfo.IsOpen)
            {
                buttonComponent.interactable = false;
            }
        }
    }

    private void Start()
    {
        ButtonRoomInfoMap = new Dictionary<GameObject, RoomInfo>();
        RefreshRoomList();
    }

    /*Public methods*/

    public void OnJoinRoomButtonClicked()
    {
        RoomInfo selectedRoomInfo = ButtonRoomInfoMap[SelectedRoomButton];
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
