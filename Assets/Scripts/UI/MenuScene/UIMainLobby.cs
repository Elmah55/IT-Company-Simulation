using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ITCompanySimulation.UI
{
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
        private ListViewElement ListViewElementPrefab;
        [SerializeField]
        private Button ButtonJoinRoom;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        private RoomInfo SelectedRoom;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnRoomListViewSelectedButtonChanged(Button selectedButton)
        {
            if (null != selectedButton)
            {
                SelectedRoom =
                    (RoomInfo)selectedButton.GetComponent<ListViewElement>().RepresentedObject;
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
                ListViewElement element = GameObject.Instantiate<ListViewElement>(ListViewElementPrefab);
                element.RepresentedObject = room;
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
            ListViewRooms.RemoveAllControls();
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
            UIRoom.SetPhotonPlayerRoomLobbyState(RoomLobbyPlayerState.NotReady);
            ButtonJoinRoom.interactable = false;
            PhotonNetwork.JoinRoom(SelectedRoom.Name);
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
            string errorMsg = string.Format("Failed to join room\n" +
                                            "{0}\n" +
                                            "Error code: {1}",
                                            codeAndMsg[1],
                                            codeAndMsg[0]);
            InfoWindowComponent.ShowOk(errorMsg, RefreshRoomList);
        }
    }
}
