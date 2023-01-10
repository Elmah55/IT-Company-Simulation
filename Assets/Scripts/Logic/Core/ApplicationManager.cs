using ExitGames.Client.Photon;
using ITCompanySimulation.Character;
using ITCompanySimulation.Project;
using ITCompanySimulation.Multiplayer;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System;
using ITCompanySimulation.Settings;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using Photon;
using ITCompanySimulation.Event;
using ITCompanySimulation.UI;
using ITCompanySimulation.Utilities;
using System.Reflection;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This class handles application aspects like loading scenes, data transfer,
    /// handling multiplayer connection and multiplayer session
    /// </summary>
    public class ApplicationManager : PunBehaviour
    {
        /*Private consts fields*/

        private const string GAME_VERSION = "1.0";
        /// <summary>
        /// Number of components that need to receive data before simulation start.
        /// </summary>
        private readonly int NUMBER_OF_REQUIRED_DATA_TRANSFERS =
            Enum.GetValues(typeof(DataTransferSource)).Length;

        /*Private fields*/

        /// <summary>
        /// Each of clients will send notification to master client when
        /// he received all data
        /// </summary>
        private int NumberOfClientsWithDataReceived;
        /// <summary>
        /// Collection of components that received data that needs to be received before starting
        /// session.
        /// </summary>
        private HashSet<DataTransferSource> ComponentsWithReceivedData = new HashSet<DataTransferSource>();
        /// <summary>
        /// When this is set to true simulation will be run in
        /// Offline Mode. It means that this client won't be connected
        /// to server and simulation will be run in local environment.
        /// This will allow to run some of game mechanism like generation
        /// of projects and workers without actually connecting to server.
        /// Value of this variable will be also set in PhotonNetwork.offlineMode
        /// https://doc.photonengine.com/en-us/pun/current/gameplay/offlinemode
        /// </summary>
        private bool OfflineMode;
        /// <summary>
        /// Settings set in inspector
        /// </summary>
        private SettingsObject Settings;
        private List<AsyncOperation> SceneLoadingOperations = new List<AsyncOperation>();
        /// <summary>
        /// Cleans resources when before scene is unloaded. This list should be
        /// used for components that need cleanup before scene unload but callback
        /// like OnDestoy won't be called on them because they are inactive.
        /// </summary>
        private static List<IDisposable> ObjectsToDispose = new List<IDisposable>();
        /// <summary>
        /// Event invoked when component received data that is needed before start of simulation.
        /// </summary>
        [SerializeField]
        private DataTransferEvent SimulationInitialDataReceived;
        [SerializeField]
        private string ConfigFileDirectoryPath;

        /*Public consts fields*/

        public const int MAX_NUMBER_OF_PLAYERS_PER_ROOM = 4;
        public const int MIN_NUMBER_OF_PLAYERS_PER_ROOM = 2;

        /*Public fields*/

        /// <summary>
        /// If set to true before game starts user will have to create room and
        /// define simulation settings. If false game will start without need to create
        /// room and default room and simulation settings will be set
        /// </summary>
        public bool UseRoom { get; private set; }
        /// <summary>
        /// True when session has started and is active.
        /// </summary>
        public bool IsSessionActive { get; private set; }
        /// <summary>
        /// True if session has started.
        /// </summary>
        public bool IsSessionStarted { get; private set; }
        public float SceneLoadingProgress { get; private set; }
        public event UnityAction SessionStarted;
        public event UnityAction<Scene> SceneStartedLoading;
        public event UnityAction<Scene> SceneFinishedLoading;
        public event UnityAction<float> SceneLoadingProgressChanged;
        /// <summary>
        /// Invoked when client is disconnected from game server.
        /// </summary>
        public event UnityAction DisconnectedFromServer;
        /// <summary>
        /// Invoked when connection to game server is established;
        /// </summary>
        public event UnityAction ConnectedToServer;
        [Tooltip("Specifies time in seconds after which timeout occurs if session did not start." +
            "Time is measured since loading of game scene.")]
        public float SessionStartTimeout;
        public IObjectStorage ConfigStorage { get; private set; }
        public static ApplicationManager Instance { get; private set; }

        /*Private methods*/

        private void Awake()
        {
            if (null != Instance)
            {
                string msg = string.Format("Only one instance of {0} should exist but is instantiated multiple times.",
                                           this.GetType().Name);
                RestrictedDebug.Log(msg, LogType.Error);
            }

            Instance = this;

            //Display info in US format regardless of culture set on windows
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Settings = Resources.Load<SettingsObject>("Settings");
            ConfigStorage = new ConfigFileManager(this.ConfigFileDirectoryPath);

            //Register custom classes that will be sent between clients.
            PhotonPeer.RegisterType(typeof(SharedProject), NetworkingData.PROJECT_BYTE_CODE, SharedProject.Serialize, SharedProject.Deserialize);
            PhotonPeer.RegisterType(typeof(PlayerData), NetworkingData.PLAYER_DATA_BYTE_CODE, PlayerData.Serialize, PlayerData.Deserialize);
            //Needed to register both base and derived class of worker because photon API requires
            //derived class to be registered even when using base class as argument in method.
            PhotonPeer.RegisterType(typeof(LocalWorker), NetworkingData.LOCAL_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);
            PhotonPeer.RegisterType(typeof(SharedWorker), NetworkingData.SHARED_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);

            //Init static classes that need to be loaded before other components
            PlayerInfoSettings.Load();

            //Fetch settings
            UseRoom = Settings.UseRoom;
            OfflineMode = Settings.OfflineMode;

            PhotonNetwork.offlineMode = this.OfflineMode;
            this.photonView.viewID = 1;
            //In PUN Classic, if the client does not set a UserId,
            //the UserId will be set to the Nickname before connecting. In PUN2 this is no longer the case.
            string userID = Guid.NewGuid().ToString();
            PhotonNetwork.AuthValues = new AuthenticationValues(userID);

            if (false == PhotonNetwork.offlineMode)
            {
                Connect();
            }

            PhotonNetwork.OnEventCall += OnPhotonNetworkEventCall;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SimulationInitialDataReceived.DataTransfered += OnComponentDataTransfered;
        }

        private void Start()
        {
            LoadScene(SceneIndex.Menu);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void OnGUI()
        {
            UnityEngine.Event currentEvent = UnityEngine.Event.current;

            if (true == currentEvent.isKey)
            {
                KeyCode requiredKey = KeyCode.D;

                //Disconnect from game server when proper key combination is pressed (Ctrl + D).
                //This should be used only for debug purposes.
                if (EventType.KeyDown == currentEvent.type && true == currentEvent.control && requiredKey == currentEvent.keyCode &&
                   (true == PhotonNetwork.isMasterClient || null == PhotonNetwork.room))
                {
                    PhotonNetwork.Disconnect();
                }
            }
        }
#endif

        private void OnSceneLoaded(Scene loadedScene, LoadSceneMode sceneLoadMode)
        {
            SceneFinishedLoading?.Invoke(loadedScene);
            SceneIndex loadedSceneIndex = (SceneIndex)loadedScene.buildIndex;

            switch (loadedSceneIndex)
            {
                case SceneIndex.Game:
                    //Reset variables needed for initial data reception to avoid problems
                    //when game scene is loaded again
                    NumberOfClientsWithDataReceived = 0;
                    IsSessionStarted = false;
                    ComponentsWithReceivedData.Clear();

                    if (true == OfflineMode || (false == OfflineMode && false == UseRoom))
                    {
                        //No need to receive data in offline mode or when player is not creating room
                        //since only one player will be inside of default room
                        StartSessionRPC();
                    }

                    StartCoroutine(CheckSessionStartTimeout());

                    break;
                default:
                    break;
            }

            //Allow incoming messages again. Scene is already loaded so
            //components will receive messages sent by other clients.
            PhotonNetwork.isMessageQueueRunning = true;
        }

        private IEnumerator CheckSessionStartTimeout()
        {
            yield return new WaitForSecondsRealtime(SessionStartTimeout);

            if (false == IsSessionActive && false == IsSessionStarted)
            {
                FinishSession();
                LoadScene(SceneIndex.Menu);
                InfoWindow.Instance.RemoveAllMessages();
                InfoWindow.Instance.ShowOk("Session has timed out.");
            }
        }

        private void OnPhotonNetworkEventCall(byte eventCode, object content, int senderId)
        {
            switch (eventCode)
            {
                case (byte)RaiseEventCode.ClientDataTransferCompleted:
                    ++NumberOfClientsWithDataReceived;
                    TryStartSession();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Starts session if all requirements for starting
        /// session has been fulfilled.
        /// </summary>
        private void TryStartSession()
        {
            //Only master client can start session
            if (true == PhotonNetwork.isMasterClient)
            {
                //Make sure all data that needs to be transfered between components before session
                //start is already transfered
                if (NumberOfClientsWithDataReceived == PhotonNetwork.otherPlayers.Length &&
                    NUMBER_OF_REQUIRED_DATA_TRANSFERS == ComponentsWithReceivedData.Count)
                {
                    string msg = string.Format("All clients ({0}) received data. Starting sesssion",
                         PhotonNetwork.playerList.Length);
                    RestrictedDebug.Log(msg);

                    photonView.RPC("StartSessionRPC", PhotonTargets.All);
                }
            }
        }

        [PunRPC]
        private void StartGameInternalRPC()
        {
            //Queue incoming data from other clients until
            //game scene has been loaded. Receiving messages
            //will be turned on again after scene is already
            //loaded and components can receive data from other
            //clients.
            PhotonNetwork.isMessageQueueRunning = false;
            LoadScene(SceneIndex.Game);
        }

        [PunRPC]
        private void StartSessionRPC()
        {
            string msg = string.Format("{0} called. Starting session...",
                MethodBase.GetCurrentMethod().Name);
            RestrictedDebug.Log(msg);

            StartCoroutine(StartSessionCoroutine());
        }

        private IEnumerator StartSessionCoroutine()
        {
            //Wait one frame so components that want to subcribe to
            //SessionStarted event can be initialized after game scene
            //being loaded
            yield return null;

            IsSessionStarted = true;
            IsSessionActive = true;
            //Session has started now, notify other components
            SessionStarted?.Invoke();
        }

        private void OnComponentDataTransfered(DataTransferSource source)
        {
            if (false == ComponentsWithReceivedData.Add(source))
            {
                string msg = string.Format("Data from same source ({0}) received more than one time",
                                        source.ToString());
                RestrictedDebug.Log(msg, LogType.Warning);
            }

            if (NUMBER_OF_REQUIRED_DATA_TRANSFERS == ComponentsWithReceivedData.Count)
            {
                if (false == PhotonNetwork.isMasterClient)
                {
                    string msg = string.Format("All components received data. Notifying master client");
                    RestrictedDebug.Log(msg);

                    //Notify master client that all required data is received
                    RaiseEventOptions options = new RaiseEventOptions
                    {
                        Receivers = ReceiverGroup.MasterClient
                    };

                    PhotonNetwork.RaiseEvent((byte)RaiseEventCode.ClientDataTransferCompleted, null, true, options);
                }
                else
                {
                    TryStartSession();
                }
            }
        }

        private IEnumerator LoadSceneAsync()
        {
            SceneLoadingProgress = 0f;
            SceneLoadingProgressChanged?.Invoke(SceneLoadingProgress);

            foreach (AsyncOperation operation in SceneLoadingOperations)
            {
                while (false == operation.isDone)
                {
                    float loadingProgress = 0f;

                    foreach (AsyncOperation asyncOperation in SceneLoadingOperations)
                    {
                        loadingProgress += asyncOperation.progress;
                    }

                    loadingProgress /= SceneLoadingOperations.Count;
                    loadingProgress *= 100f;

                    if (loadingProgress != SceneLoadingProgress)
                    {
                        SceneLoadingProgress = loadingProgress;
                        SceneLoadingProgressChanged?.Invoke(SceneLoadingProgress);
                    }

                    yield return null;
                }
            }

            SceneLoadingOperations.Clear();
        }

        private static void CleanupDisposableObjects()
        {
            foreach (IDisposable disposableObject in ObjectsToDispose)
            {
                disposableObject.Dispose();
            }

            ObjectsToDispose.Clear();
        }

        /*Public methods*/

        public void StartGame()
        {
            if (false == UseRoom)
            {
                //Create room with default settings and join it
                SimulationSettings.InitialBalance = Settings.InitialBalance;
                SimulationSettings.TargetBalance = Settings.TargetBalance;
                SimulationSettings.MinimalBalance = Settings.MinimalBalance;
                RoomOptions options = new RoomOptions() { MaxPlayers = MAX_NUMBER_OF_PLAYERS_PER_ROOM };
                PhotonNetwork.JoinOrCreateRoom("Default", options, PhotonNetwork.lobby);
            }

            if (SimulationSettings.InitialBalance <= SimulationSettings.MinimalBalance)
            {
                string msg = string.Format("Initial balance ({0} $) is smaller than or equal to minimal balance ({1} $)",
                                           SimulationSettings.InitialBalance,
                                           SimulationSettings.MinimalBalance);
                RestrictedDebug.Log(msg, LogType.Warning);
            }

            if (true == PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = false;
                //Start game for all clients in the room
                this.photonView.RPC("StartGameInternalRPC", PhotonTargets.All);
            }
        }

        public void FinishSession()
        {
            IsSessionActive = false;

            if (null != PhotonNetwork.room)
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        public void Connect()
        {
            PhotonNetwork.ConnectUsingSettings(GAME_VERSION);
        }

        public void LoadScene(SceneIndex index)
        {
            int intIndex = (int)index;
            Scene activeScene = SceneManager.GetActiveScene();

            if (activeScene.buildIndex != intIndex)
            {
                SceneLoadingProgress = 0f;
                CleanupDisposableObjects();
                SceneStartedLoading?.Invoke(activeScene);

                //Do not try to unload previous scene at application start when no previous scene was loaded
                if (SceneManager.sceneCount > 1)
                {
                    SceneIndex sceneToUnload = (SceneIndex.Menu == index) ? SceneIndex.Game : SceneIndex.Menu;
                    SceneLoadingOperations.Add(SceneManager.UnloadSceneAsync((int)sceneToUnload));
                }

                SceneLoadingOperations.Add(SceneManager.LoadSceneAsync(intIndex, LoadSceneMode.Additive));
                StartCoroutine(LoadSceneAsync());
            }
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            string msg = "Connected to master";
            RestrictedDebug.Log(msg);
        }

        /// <summary>
        /// Is called when client is connected to photon since auto
        /// join lobby is set to true.
        /// </summary>
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

            string msg = string.Format("Joined lobby: {0}",
                                       PhotonNetwork.lobby.Name);
            RestrictedDebug.Log(msg);

            //Since auto lobby join is enabled this method
            //will be called when client is connected to server
            ConnectedToServer?.Invoke();
        }

        public override void OnLeftLobby()
        {
            string msg = "Left lobby";
            RestrictedDebug.Log(msg);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            //Game will be started in online mode but only with one player in room
            if (false == UseRoom && false == OfflineMode)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
                StartGameInternalRPC();
            }

            string msg = string.Format("Joined room\n" +
                               "Name: {0}\n" +
                               "Number of players: {1}\n" +
                               "Max number of players : {2}\n" +
                               "Open: {3}",
                               PhotonNetwork.room.Name,
                               PhotonNetwork.room.PlayerCount,
                               PhotonNetwork.room.MaxPlayers,
                               PhotonNetwork.room.IsOpen);
            RestrictedDebug.Log(msg);
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

            string msg = "Left room";
            RestrictedDebug.Log(msg);
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

            InfoWindow.Instance.ShowOk("Disconnected from game server.");

            string msg = "Disconnected from Photon";
            RestrictedDebug.Log(msg);

            DisconnectedFromServer?.Invoke();
        }

        public override void OnConnectionFail(DisconnectCause cause)
        {
            base.OnConnectionFail(cause);

            string msg = string.Format("Connection failed. Reason: {0}",
                                       cause);
            RestrictedDebug.Log(msg, LogType.Warning);

            DisconnectedFromServer?.Invoke();
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonJoinRoomFailed(codeAndMsg);

            string errorMsg = string.Format("Failed to create room\n" +
                                "{0}\n" +
                                "Error code: {1}",
                                codeAndMsg[1],
                                codeAndMsg[0]);
            InfoWindow.Instance.ShowOk(errorMsg);

            string msg = "Failed to join the room";
            RestrictedDebug.Log(msg);
        }

        /// <summary>
        /// Register object that needs to be disposed before scene is unloaded.
        /// </summary>
        /// <param name="obj">Object that will be disposed before scene unload.</param>
        public static void RegisterObjectForCleanup(IDisposable obj)
        {
            ObjectsToDispose.Add(obj);
        }
    }
}
