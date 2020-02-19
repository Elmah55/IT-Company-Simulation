using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class handles main game aspects like loading scenes and
/// handling multiplayer connection
/// </summary>
public class MainGameManager : Photon.PunBehaviour
{
    /*Private consts fields*/

    private const string GAME_VERSION = "1.0";

    /*Private fields*/

    private bool MenuSceneLoadedAfterGameFinish;

    /*Public consts fields*/

    public const int MAX_NUMBER_OF_PLAYERS_PER_ROOM = 4;
    public const int MIN_NUMBER_OF_PLAYERS_PER_ROOM = 2;

    /*Public fields*/

    /// <summary>
    /// When this is set to true simulation will be run in
    /// Offline Mode. It means that this client won't be connected
    /// to server and simulation will be run in local environment.
    /// This will allow to run some of game mechanism like generation
    /// of projects and workers without actually connecting to server.
    /// Value of this variable will be also set in PhotonNetwork.offlineMode
    /// https://doc.photonengine.com/en-us/pun/current/gameplay/offlinemode
    /// </summary>
    public bool OfflineMode;
    /// <summary>
    /// If set to true before game starts user will have to create room and
    /// define simulation settings. If false game will start without need to create
    /// room and default room and simulation settings will be set
    /// </summary>
    public bool UseRoom;
    public SimulationSettings SettingsOfSimulation { get; private set; } = new SimulationSettings();
    /// <summary>
    /// Sets the time scale that simulation should be run with. 1.0 is default scale
    /// </summary>
    [Range(0.1f, 10.0f)]
    public float SimulationTimeScale;


    /// <summary>
    /// Below values can be used to set balance when game creation through room is not used
    /// </summary>
    [Range(SimulationSettings.MIN_INITIAL_BALANCE, SimulationSettings.MAX_INITIAL_BALANCE)]
    public int InitialCompanyBalance;
    [Range(SimulationSettings.MIN_TARGET_BALANCE, SimulationSettings.MAX_TARGET_BALANCE)]
    public int TargetCompanyBalance;

    /*Private methods*/

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        PhotonNetwork.offlineMode = this.OfflineMode;

        if (false == PhotonNetwork.offlineMode)
        {
            PhotonNetwork.ConnectUsingSettings(GAME_VERSION);
        }
    }

    private void Update()
    {
        Time.timeScale = SimulationTimeScale;
    }

    private IEnumerator StartGameCoroutine()
    {
        //Start game for other clients firts so their game scene is loaded
        //and data from master client can be sent to them
        //TODO: This is temp fix. Add synchronization mechanism in case when one client
        //scene loading is slower than other clients and delay time is not enough
        //(data will be sent when some of scripts are not started and will result in photon
        //RPC error)
        float delayTime = (true == PhotonNetwork.isMasterClient && false == PhotonNetwork.offlineMode) ?
            2.0f : 0.0f;
        yield return new WaitForSecondsRealtime(delayTime);
        SceneManager.LoadScene((int)SceneIndex.Game);
    }

    [PunRPC]
    private void StartGameInternal()
    {
        //PhotonView component won't be need anymore since
        //MainSimulationManager's PhotonView component will be used in game scene
        GameObject.Destroy(this.photonView);
        StartCoroutine(StartGameCoroutine());
    }

    /*Public methods*/

    public void StartGame()
    {
        if (false == UseRoom)
        {
            //Create room with default settings and join it
            SettingsOfSimulation.InitialBalance = this.InitialCompanyBalance;
            SettingsOfSimulation.TargetBalance = this.TargetCompanyBalance;
            RoomOptions options = new RoomOptions() { MaxPlayers = MAX_NUMBER_OF_PLAYERS_PER_ROOM };
            PhotonNetwork.JoinOrCreateRoom("Default", options, PhotonNetwork.lobby);
        }

        if (null == PhotonNetwork.room)
        {
            throw new InvalidOperationException(
                "Game can be started only when client is in room");
        }

        if (true == PhotonNetwork.isMasterClient)
        {
            this.photonView.RPC("StartGameInternal", PhotonTargets.All);
        }
    }

    public void FinishGame()
    {
        PhotonNetwork.LeaveRoom();
        //TODO: Find way to prevent spawning other GameManager
        //object when menu scene is loaded so destroying of this
        //object is not needed and client won't have connect to
        //photon server every time menu scene is loaded
        SceneManager.LoadScene((int)SceneIndex.Menu);
        //There is already another game object existing with this
        //script when menu scene is loaded
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Is called when client is connected to photon since auto
    /// join lobby is set to true
    /// </summary>
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();

        string msg = string.Format("Connected to Photon. Joined lobby: {0}",
                                   PhotonNetwork.lobby.Name);
        Debug.Log(msg);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        string msg = string.Format("Joined room\n" +
                                   "Name: {0}\n" +
                                   "Number of players: {1}\n" +
                                   "Max number of players : {2}",
                                   PhotonNetwork.room.Name,
                                   PhotonNetwork.room.PlayerCount,
                                   PhotonNetwork.room.MaxPlayers);
        Debug.Log(msg);
    }

    public override void OnDisconnectedFromPhoton()
    {
        base.OnDisconnectedFromPhoton();

        string msg = "Disconnected from Photon";
        Debug.Log(msg);
    }

    public override void OnConnectionFail(DisconnectCause cause)
    {
        base.OnConnectionFail(cause);

        string msg = string.Format("Connection failed. Reason: {0}", cause);
        Debug.Log(msg);
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        base.OnPhotonJoinRoomFailed(codeAndMsg);

        string msg = "Failed to join the room";
        Debug.Log(msg);
    }
}
