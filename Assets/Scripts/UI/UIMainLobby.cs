using ITCompanySimulation.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Class handling UI of main multiplayer lobby
/// </summary>
public class UIMainLobby : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private GameObject PanelRoomLobby;
    private IButtonSelector RoomButtonSelector = new ButtonSelector();
    /// <summary>
    /// This list view will hold list of rooms on the server
    /// </summary>
    [SerializeField]
    private ControlListView ListViewRooms;
    [SerializeField]
    private TextMeshProUGUI TextRoomList;
    /// <summary>
    /// Prefab to be used in rooms list view
    /// </summary>
    [SerializeField]
    private ListViewElementRoom ListViewElementPrefab;
    [SerializeField]
    private Button ButtonJoinRoom;
    private Queue<ListViewElementRoom> ListViewElementPool;
    private RoomInfo SelectedRoom;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void OnRoomListViewSelectedButtonChanged(Button selectedButton)
    {
        if (null != selectedButton)
        {
            SelectedRoom =
                RoomButtonSelector.GetSelectedButton().GetComponent<ListViewElementRoom>().Room;
            ButtonJoinRoom.interactable = SelectedRoom.IsOpen
                && SelectedRoom.PlayerCount <= SelectedRoom.MaxPlayers;
        }
        else
        {
            SelectedRoom = null;
            ButtonJoinRoom.interactable = false;
        }
    }

    private void AddLobbbyRoomsButtons()
    {
        foreach (RoomInfo room in PhotonNetwork.GetRoomList())
        {
            ListViewElementRoom element;

            if (null != ListViewElementPool && ListViewElementPool.Count > 0)
            {
                element = ListViewElementPool.Dequeue();
                element.gameObject.SetActive(true);
            }
            else
            {
                element = GameObject.Instantiate<ListViewElementRoom>(ListViewElementPrefab);
            }

            element.Room = room;
            Button buttonComponent = element.GetComponent<Button>();

            string roomStatusText = room.IsOpen ? "In lobby" : "In progress";
            string buttonText = string.Format("{0} Players: {1}/{2}\nStatus: {3}",
                                            room.Name,
                                            room.PlayerCount,
                                            room.MaxPlayers,
                                            roomStatusText);
            element.Text.text = buttonText;

            ListViewRooms.AddControl(element.gameObject);
            RoomButtonSelector.AddButton(buttonComponent);
        }
    }

    private void OnEnable()
    {
        RefreshRoomList();
    }

    private void Start()
    {
        SetRoomListText();
        RoomButtonSelector.SelectedButtonChanged += OnRoomListViewSelectedButtonChanged;
    }

    private void RefreshRoomList()
    {
        if (ListViewRooms.Controls.Count > 0 && null == ListViewElementPool)
        {
            ListViewElementPool = new Queue<ListViewElementRoom>();
        }

        foreach (GameObject obj in ListViewRooms.Controls)
        {
            ListViewElementRoom elem = obj.GetComponent<ListViewElementRoom>();
            elem.gameObject.SetActive(false);
            ListViewElementPool.Enqueue(elem);
        }

        ListViewRooms.RemoveAllControls(false);
        RoomButtonSelector.RemoveAllButtons();
        AddLobbbyRoomsButtons();
        ButtonJoinRoom.interactable = false;
        SetRoomListText();
    }

    private void SetRoomListText()
    {
        string text;

        if (PhotonNetwork.GetRoomList().Length > 0)
        {
            text = string.Format(string.Format("Room list ({0})",
                PhotonNetwork.GetRoomList().Length));
        }
        else
        {
            text = "Room list";
        }

        TextRoomList.text = text;
    }

    /*Public methods*/

    public void OnButtonJoinRoomClicked()
    {
        PhotonNetwork.JoinRoom(SelectedRoom.Name);
        ButtonJoinRoom.interactable = false;
    }

    public void OnButtonRefreshClicked()
    {
        RefreshRoomList();
    }

    public override void OnJoinedRoom()
    {
        PanelRoomLobby.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        base.OnPhotonJoinRoomFailed(codeAndMsg);

        RefreshRoomList();
    }
}
