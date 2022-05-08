using ExitGames.Client.Photon;
using ITCompanySimulation.Character;
using ITCompanySimulation.Project;
using ITCompanySimulation.Multiplayer;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using ITCompanySimulation.Settings;
using AudioSettings = ITCompanySimulation.Settings.AudioSettings;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using Photon;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System.Reflection;
#endif

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

        /*Private fields*/

        /// <summary>
        /// Each of clients will send notification to master client when
        /// he received all data
        /// </summary>
        private int NumberOfClientsWithDataReceived;
        /// <summary>
        /// Number of this client's completed data transfers required by components to start simulation.
        /// This will be counted only for clients that are not master clients since master client
        /// only sends these data to other clients and not receives them.
        /// </summary>
        private int NumberOfCompletedDataTransfers;
        /// <summary>
        /// Number of this client's completed data transfers required by components to start simulation.
        /// Valid only if client is master client.
        /// </summary>
        private int NumberOfMasterClientCompletedDataTransfers;
        /// <summary>
        /// Components that need to receive data before starting session
        /// </summary>
        private IDataReceiver[] DataReceiverComponents;
        /// <summary>
        /// Components of master client that need to receive data before starting session
        /// </summary>
        private IMasterClientDataReceiver[] MasterClientDataReceiverComponents;
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
        /// True when session has started and is active
        /// </summary>
        public bool IsSessionActive { get; private set; }
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

        /*Private methods*/

        private void Awake()
        {
            //Display info in US format regardless of culture set on windows
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            Settings = Resources.Load<SettingsObject>("Settings");

            //Register custom classes that will be sent between clients.
            PhotonPeer.RegisterType(typeof(SharedProject), NetworkingData.PROJECT_BYTE_CODE, SharedProject.Serialize, SharedProject.Deserialize);
            PhotonPeer.RegisterType(typeof(PlayerData), NetworkingData.PLAYER_DATA_BYTE_CODE, PlayerData.Serialize, PlayerData.Deserialize);
            //Needed to register both base and derived class of worker because photon API requires
            //derived class to be registered even when using base class as argument in method.
            PhotonPeer.RegisterType(typeof(LocalWorker), NetworkingData.LOCAL_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);
            PhotonPeer.RegisterType(typeof(SharedWorker), NetworkingData.SHARED_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);
            PhotonPeer.RegisterType(typeof(SharedSimulationStats), NetworkingData.SIMULATION_STATS_BYTE_CODE, SharedSimulationStats.Serialize, SharedSimulationStats.Deserialize);

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
        }

        private void Start()
        {
            //Init other static classes
            AudioSettings.Load();

            LoadScene(SceneIndex.Menu);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private void OnGUI()
        {
            Event currentEvent = Event.current;

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

            if ((int)SceneIndex.Game == loadedScene.buildIndex)
            {
                //Clear event listeners to avoid problems when game scene is loaded again
                SessionStarted = null;
                NumberOfClientsWithDataReceived = 0;
                NumberOfCompletedDataTransfers = 0;
                NumberOfMasterClientCompletedDataTransfers = 0;

                if (false == PhotonNetwork.offlineMode && true == UseRoom)
                {
                    DataReceiverComponents = GameObject.FindObjectsOfType<UnityEngine.MonoBehaviour>().OfType<IDataReceiver>().ToArray();

                    foreach (IDataReceiver receiver in DataReceiverComponents)
                    {
                        if (true == receiver.IsDataReceived)
                        {
                            OnComponentDataTransfered();
                        }
                        else
                        {
                            receiver.DataReceived += OnComponentDataTransfered;
                        }
                    }

                    //Subscribe to data transfers needed also by master client
                    if (true == PhotonNetwork.isMasterClient)
                    {
                        MasterClientDataReceiverComponents =
                            GameObject.FindObjectsOfType<UnityEngine.MonoBehaviour>().OfType<IMasterClientDataReceiver>().ToArray();

                        foreach (IMasterClientDataReceiver receiver in MasterClientDataReceiverComponents)
                        {
                            if (true == receiver.IsDataReceived)
                            {
                                OnComponentDataTransfered();
                            }
                            else
                            {
                                receiver.DataReceived += OnMasterClientComponentDataTransfered;
                            }
                        }
                    }
                }
                else
                {
                    //No need to receive data in offline mode
                    StartSessionRPC();
                }
            }

            //Allow incoming messages again. Scene is already loaded so
            //components will receive messages sent by other clients.
            PhotonNetwork.isMessageQueueRunning = true;
        }

        private void OnPhotonNetworkEventCall(byte eventCode, object content, int senderId)
        {
            switch (eventCode)
            {
                case (byte)RaiseEventCode.ClientReceivedData:
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
                    NumberOfMasterClientCompletedDataTransfers == MasterClientDataReceiverComponents.Length)
                {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.LogFormat("[{0}] All clients ({1}) received data. Starting sesssion",
                        this.GetType().Name, PhotonNetwork.playerList.Length);
#endif
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
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogFormat("[{0}] {1} called. Starting session...",
                this.GetType().Name,
                MethodBase.GetCurrentMethod().Name);
#endif
            StartCoroutine(StartSessionCoroutine());
        }

        private IEnumerator StartSessionCoroutine()
        {
            //Wait one frame so components that want to subcribe to
            //SessionStarted event can be initialized after game scene
            //being loaded
            yield return null;

            IsSessionActive = true;
            //Session has started now, notify other components
            SessionStarted?.Invoke();
        }

        private void OnComponentDataTransfered()
        {
            ++NumberOfCompletedDataTransfers;

            if (DataReceiverComponents.Length == NumberOfCompletedDataTransfers)
            {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                Debug.LogFormat("[{0}] All components received data. Notifying master client",
                    this.GetType().Name);
#endif
                //Notify master client that all required data is received
                RaiseEventOptions options = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others
                };

                PhotonNetwork.RaiseEvent((byte)RaiseEventCode.ClientReceivedData, null, true, options);
            }
        }

        private void OnMasterClientComponentDataTransfered()
        {
            ++NumberOfMasterClientCompletedDataTransfers;
            //Try to start session here in case all other clients
            //already received required data and are waiting for master client
            TryStartSession();
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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (SimulationSettings.InitialBalance <= SimulationSettings.MinimalBalance)
            {
                Debug.LogWarningFormat("[{0}] Initial balance ({1} $) is smaller than or equal to minimal balance ({2} $)",
                                       this.GetType().Name,
                                       SimulationSettings.InitialBalance,
                                       SimulationSettings.MinimalBalance);
            }
#endif

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
            PhotonNetwork.LeaveRoom();
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

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Connected to master", this.GetType().Name);
            Debug.Log(msg);
#endif
        }

        /// <summary>
        /// Is called when client is connected to photon since auto
        /// join lobby is set to true.
        /// </summary>
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Joined lobby: {1}",
                                       this.GetType().Name,
                                       PhotonNetwork.lobby.Name);
            Debug.Log(msg);
#endif

            //Since auto lobby join is enabled this method
            //will be called when client is connected to server
            ConnectedToServer?.Invoke();
        }

        public override void OnLeftLobby()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Left lobby",
                                       this.GetType().Name);
            Debug.Log(msg);
#endif
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            //Game will be started in online mode but only with one player in room
            if (false == UseRoom && false == PhotonNetwork.offlineMode)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.room.IsVisible = false;
                StartGameInternalRPC();
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Joined room\n" +
                               "Name: {1}\n" +
                               "Number of players: {2}\n" +
                               "Max number of players : {3}\n" +
                               "Open: {4}",
                               this.GetType().Name,
                               PhotonNetwork.room.Name,
                               PhotonNetwork.room.PlayerCount,
                               PhotonNetwork.room.MaxPlayers,
                               PhotonNetwork.room.IsOpen);
            Debug.Log(msg);
#endif
        }

        public override void OnLeftRoom()
        {
            base.OnLeftRoom();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Left room", this.GetType().Name);
            Debug.Log(msg);
#endif
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Disconnected from Photon", this.GetType().Name);
            Debug.Log(msg);
#endif

            DisconnectedFromServer?.Invoke();
        }

        public override void OnConnectionFail(DisconnectCause cause)
        {
            base.OnConnectionFail(cause);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Connection failed. Reason: {1}",
                                       this.GetType().Name,
                                       cause);
            Debug.Log(msg);
#endif

            DisconnectedFromServer?.Invoke();
        }

        public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonJoinRoomFailed(codeAndMsg);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string msg = string.Format("[{0}] Failed to join the room", this.GetType().Name);
            Debug.Log(msg);
#endif
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
