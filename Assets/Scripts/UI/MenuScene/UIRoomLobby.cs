using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Multiplayer;
using ExitGames.Client.Photon;
using TMPro;
using ITCompanySimulation.Core;

namespace ITCompanySimulation.UI
{
    public class UIRoomLobby : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Colors used for player's list view element when his state is ready
        /// </summary>
        [SerializeField]
        private Color ListViewElementPlayerReadyColor;
        /// <summary>
        /// Colors used for player's list view element when his state is not ready
        /// </summary>
        [SerializeField]
        private Color ListViewElementPlayerNotReadyColor;
        [SerializeField]
        private Button ButtonStartGame;
        [SerializeField]
        private Button ButtonReady;
        [SerializeField]
        private TextMeshProUGUI TextButtonReady;
        [SerializeField]
        private ListViewElementPhotonPlayer ListViewElementPrefab;
        /// <summary>
        /// Will hold buttons displaying info about players in
        /// current room
        /// </summary>
        [SerializeField]
        private ControlListView ListViewPlayers;
        [SerializeField]
        private MainGameManager GameManagerComponent;
        [SerializeField]
        private GameObject PanelMainLobby;
        private IObjectPool<ListViewElementPhotonPlayer> ListViewElementPool;
        [SerializeField]
        TextMeshProUGUI TextListViewPlayers;
        [SerializeField]
        private TextMeshProUGUI TextRoomName;
        [SerializeField]
        private TextMeshProUGUI TextInitialBalance;
        [SerializeField]
        private TextMeshProUGUI TextMinimalBalance;
        [SerializeField]
        private TextMeshProUGUI TextTargetBalance;
        private int NumberOfReadyClients;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            GameManagerComponent = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainGameManager>();
            ButtonReady.interactable = (false == PhotonNetwork.offlineMode);
        }

        private void OnEnable()
        {
            int initialBalance = (int)PhotonNetwork.room.CustomProperties[RoomCustomPropertiesKey.SettingsOfSimulationInitialBalance.ToString()];
            int targetBalance = (int)PhotonNetwork.room.CustomProperties[RoomCustomPropertiesKey.SettingsOfSimulationTargetBalance.ToString()];
            int minimalBalance = (int)PhotonNetwork.room.CustomProperties[RoomCustomPropertiesKey.SettingsOfSimulationMinimalBalance.ToString()];
            SimulationSettings.InitialBalance = initialBalance;
            SimulationSettings.TargetBalance = targetBalance;
            SimulationSettings.MinimalBalance = minimalBalance;

            AddPlayersListViewElements();
            SetSimulationSettingsText();
            SetListViewPlayersText();
            NumberOfReadyClients = 0;
            SetStartButtonState();
            SetReadyButtonText(RoomLobbyPlayerState.NotReady);
        }

        private void OnDisable()
        {
            for (int i = ListViewPlayers.Controls.Count - 1; i >= 0; i--)
            {
                ListViewElementPhotonPlayer element =
                    ListViewPlayers.Controls[i].GetComponent<ListViewElementPhotonPlayer>();
                DisablePlayerListViewElement(element);
            }
        }

        private void AddPlayersListViewElements()
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                AddPlayerListViewElement(player);
            }
        }

        /// <summary>
        /// Returns list view element associated with given photon player
        /// </summary>
        /// <param name="listView">List view element will be searched in this list view</param>
        private ListViewElementPhotonPlayer GetPhotonPlayerListViewElement(ControlListView listView, PhotonPlayer player)
        {
            return listView.Controls.Find((x) =>
            {
                return x.GetComponent<ListViewElementPhotonPlayer>().Player.ID == player.ID;
            }).GetComponent<ListViewElementPhotonPlayer>();
        }

        /// <summary>
        /// Returns button's text of single player in a room lobby. Text contains
        /// player's nickname and role in a room lobby
        /// </summary>
        private string GetPlayerListViewElementText(PhotonPlayer player)
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

            if (false == PhotonNetwork.offlineMode)
            {
                RoomLobbyPlayerState playerState =
                    (RoomLobbyPlayerState)player.CustomProperties[PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString()];

                switch (playerState)
                {
                    case RoomLobbyPlayerState.Ready:
                        buttonText += "\nReady";
                        break;
                    case RoomLobbyPlayerState.NotReady:
                        buttonText += "\nNot ready";
                        break;
                    default:
                        break;
                }
            }

            return buttonText;
        }

        private void SetSimulationSettingsText()
        {
            TextMinimalBalance.text = string.Format("{0} $", SimulationSettings.MinimalBalance);
            TextTargetBalance.text = string.Format("{0} $", SimulationSettings.TargetBalance);
            TextInitialBalance.text = string.Format("{0} $", SimulationSettings.InitialBalance);
            TextRoomName.text = PhotonNetwork.room.Name;
        }

        private void SetListViewPlayersText()
        {
            TextListViewPlayers.text = string.Format("Players ({0} / {1})",
                PhotonNetwork.room.playerCount, PhotonNetwork.room.MaxPlayers);
        }

        /// <summary>
        /// Sets color if image according to player state
        /// </summary>
        private void SetListViewElementImageColor(ListViewElementPhotonPlayer element, RoomLobbyPlayerState state)
        {
            switch (state)
            {
                case RoomLobbyPlayerState.Ready:
                    element.BackgroundImage.color = ListViewElementPlayerReadyColor;
                    break;
                case RoomLobbyPlayerState.NotReady:
                    element.BackgroundImage.color = ListViewElementPlayerNotReadyColor;
                    break;
                default:
                    break;
            }
        }

        private void AddPlayerListViewElement(PhotonPlayer player)
        {
            ListViewElementPhotonPlayer element = null;

            if (null != ListViewElementPool)
            {
                element = ListViewElementPool.GetObject();
            }

            if (null == element)
            {
                element = GameObject.Instantiate<ListViewElementPhotonPlayer>(ListViewElementPrefab);
            }
            else
            {
                element.gameObject.SetActive(true);
            }

            ListViewPlayers.AddControl(element.gameObject);

            string buttonText = GetPlayerListViewElementText(player);
            element.Text.text = buttonText;
            element.Player = player;

            if (false == PhotonNetwork.offlineMode)
            {
                RoomLobbyPlayerState playerState =
                    (RoomLobbyPlayerState)player.CustomProperties[PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString()];
                SetListViewElementImageColor(element, playerState);
            }

        }

        /// <summary>
        /// Disables list view element and returns it back to pool
        /// </summary>
        /// <param name="element"></param>
        private void DisablePlayerListViewElement(ListViewElementPhotonPlayer element)
        {
            ListViewPlayers.RemoveControl(element.gameObject, false);

            if (null == ListViewElementPool)
            {
                ListViewElementPool = new ObjectPool<ListViewElementPhotonPlayer>();
            }

            element.gameObject.SetActive(false);
            ListViewElementPool.AddObject(element);
        }

        private void SetStartButtonState()
        {
            //At least two players are needed to start game and only master client can start game
            if ((1 < PhotonNetwork.playerList.Length && true == PhotonNetwork.isMasterClient
                && PhotonNetwork.room.PlayerCount == NumberOfReadyClients)
                || true == PhotonNetwork.offlineMode)
            {
                ButtonStartGame.interactable = true;
            }
            else
            {
                ButtonStartGame.interactable = false;
            }
        }

        private void SetReadyButtonText(RoomLobbyPlayerState state)
        {
            string text = string.Empty;

            switch (state)
            {
                case RoomLobbyPlayerState.Ready:
                    text = "Not ready";
                    break;
                case RoomLobbyPlayerState.NotReady:
                    text = "Ready";
                    break;
                default:
                    break;
            }

            TextButtonReady.text = text;
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
        }

        public void OnButtonReadyClicked()
        {
            RoomLobbyPlayerState state =
                (RoomLobbyPlayerState)PhotonNetwork.player.CustomProperties[PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString()];
            state = (state == RoomLobbyPlayerState.Ready) ? RoomLobbyPlayerState.NotReady : RoomLobbyPlayerState.Ready;
            UIRoom.SetPhotonPlayerRoomLobbyState(state);
            SetReadyButtonText(state);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            base.OnPhotonPlayerConnected(newPlayer);

            AddPlayerListViewElement(newPlayer);
            SetStartButtonState();
            SetListViewPlayersText();
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer disconnectedPlayer)
        {
            base.OnPhotonPlayerDisconnected(disconnectedPlayer);

            ListViewElementPhotonPlayer playerElement =
                GetPhotonPlayerListViewElement(ListViewPlayers, disconnectedPlayer);

            DisablePlayerListViewElement(playerElement);

            SetStartButtonState();
            SetListViewPlayersText();
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            ListViewElementPhotonPlayer newMasterClientElement = GetPhotonPlayerListViewElement(ListViewPlayers, newMasterClient);
            newMasterClientElement.Text.text = GetPlayerListViewElementText(newMasterClient);

            if (true == PhotonNetwork.isMasterClient)
            {
                ButtonStartGame.interactable = true;
            }
        }

        /// <summary>
        /// Indexes of array defined in PUN documentation see:
        /// https://doc-api.photonengine.com/en/pun/v1/group__public_api.html#ggaf30bbea51cc8c4b1ddc239d1c5c1468fa67402d95c324cda2b6d6e2fc391ae941
        /// </summary>
        public override void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
        {
            base.OnPhotonPlayerPropertiesChanged(playerAndUpdatedProps);

            PhotonPlayer player = (PhotonPlayer)playerAndUpdatedProps[0];
            Hashtable customProperties = (Hashtable)playerAndUpdatedProps[1];

            ListViewElementPhotonPlayer playerElement = GetPhotonPlayerListViewElement(ListViewPlayers, player);
            RoomLobbyPlayerState state =
                (RoomLobbyPlayerState)customProperties[PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString()];
            SetListViewElementImageColor(playerElement, state);
            playerElement.Text.text = GetPlayerListViewElementText(player);
            NumberOfReadyClients = (state == RoomLobbyPlayerState.Ready) ? ++NumberOfReadyClients : --NumberOfReadyClients;
            SetStartButtonState();
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            PanelMainLobby.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}