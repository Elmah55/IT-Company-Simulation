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

    /*Private methods*/

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        MultiplayerConnectionComponent = GetComponent<MultiplayerConnection>();
    }

    /*Public methods*/

    public void StartGame()
    {
        MultiplayerConnectionComponent.JoinOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        SceneManager.LoadScene("GameScene");
    }
}
