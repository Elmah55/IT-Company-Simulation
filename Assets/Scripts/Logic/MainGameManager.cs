using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This class handles main game aspects like loading scenes and
/// handling multiplayer connection
/// </summary>
public class MainGameManager : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private MenuUIManager UIManagerComponent;
    private MultiplayerConnection MultiplayerConnectionComponent;

    /*Public consts fields*/

    public const string GAME_VERSION = "1.0";

    /*Public fields*/

    /// <summary>
    /// When this is set to true simulation will be run in
    /// Offline Mode. It means that this client won't be connected
    /// to server and simulation will be run in local environment.
    /// This will allow to run some of game mechanism like generation
    /// of projects and workers without actually connecting to server
    /// </summary>
    public bool OfflineMode;

    /*Private methods*/

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        MultiplayerConnectionComponent = GetComponent<MultiplayerConnection>();
    }

    /*Public methods*/

    public void StartGame()
    {
        if (false == OfflineMode)
        {
            MultiplayerConnectionComponent.JoinOrCreateRoom();
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        SceneManager.LoadScene("GameScene");
    }
}
