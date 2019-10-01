using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameManager : Photon.PunBehaviour
{
    public const string GAME_VERSION = "1.0";

    private MenuUIManager UIManagerComponent;
    private MultiplayerConnection MultiplayerConnectionComponent;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        MultiplayerConnectionComponent = GetComponent<MultiplayerConnection>();
    }

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
