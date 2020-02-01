using System;
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

    private PhotonView PhotonViewComponent;
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

    /*Private methods*/

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        PhotonViewComponent = GetComponent<PhotonView>();
        PhotonNetwork.offlineMode = this.OfflineMode;
        PhotonNetwork.ConnectUsingSettings(GAME_VERSION);
    }

    [PunRPC]
    private void StartGameInternal()
    {
        //PhotonView component won't be need anymore since
        //MainSimulationManager component will be used in game scene
        GameObject.Destroy(PhotonViewComponent);
        SceneManager.LoadScene((int)SceneIndex.Game);
    }

    /*Public methods*/

    public void StartGame()
    {
        if (null == PhotonNetwork.room)
        {
            throw new InvalidOperationException(
                "Game can be started only when client is in room");
        }

        if (true == PhotonNetwork.isMasterClient)
        {
            PhotonViewComponent.RPC("StartGameInternal", PhotonTargets.All);
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
}
