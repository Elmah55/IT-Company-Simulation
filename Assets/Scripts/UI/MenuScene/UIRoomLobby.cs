using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Multiplayer;
using ExitGames.Client.Photon;
using TMPro;
using ITCompanySimulation.Core;
using ITCompanySimulation.Settings;

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
        private ListViewElement ListViewElementPlayerPrefab;
        /// <summary>
        /// Will hold buttons displaying info about players in
        /// current room
        /// </summary>
        [SerializeField]
        private ControlListView ListViewPlayers;
        private ApplicationManager ApplicationManagerComponent;
        [SerializeField]
        private GameObject PanelMainLobby;
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
        [SerializeField]
        private ChatWindow ChatWindowComponent;
        private int NumberOfReadyClients;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            ApplicationManagerComponent = GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
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
            ChatWindowComponent.ClearChat();
        }

        private void OnDisable()
        {
            ListViewPlayers.RemoveAllControls();
        }

        private void AddPlayersListViewElements()
        {
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                AddPlayerListViewElement(player);
            }
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
        private void SetListViewElementImageColor(ListViewElement element, RoomLobbyPlayerState state)
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
            ListViewElement element =
                GameObject.Instantiate<ListViewElement>(ListViewElementPlayerPrefab);

            string buttonText = GetPlayerListViewElementText(player);
            element.Text.text = buttonText;
            element.RepresentedObject = player;

            ListViewPlayers.AddControl(element.gameObject);

            if (false == PhotonNetwork.offlineMode)
            {
                RoomLobbyPlayerState playerState =
                    (RoomLobbyPlayerState)player.CustomProperties[PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString()];
                SetListViewElementImageColor(element, playerState);
            }

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
            ApplicationManagerComponent.StartGame();
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

            ListViewElement playerElement = ListViewPlayers.FindElement(disconnectedPlayer);
            ListViewPlayers.RemoveControl(playerElement.gameObject);

            SetStartButtonState();
            SetListViewPlayersText();
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            ListViewElement newMasterClientElement = ListViewPlayers.FindElement(newMasterClient);
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

            ListViewElement playerElement = ListViewPlayers.FindElement(player);
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