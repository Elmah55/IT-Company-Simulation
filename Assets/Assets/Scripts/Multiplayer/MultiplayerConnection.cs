using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon;

public class MultiplayerConnection : Photon.PunBehaviour
{
    /*Private consts fields*/

    private const int MAX_PLAYERS_PER_ROOM = 4;

    /*Private fields*/

    private MainGameManager GameManagerComponent;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        GameManagerComponent = GetComponent<MainGameManager>();

        if (false == GameManagerComponent.OfflineMode)
        {
            ConnectToServer();
        }
    }

    /*Public methods*/

    /// <summary>
    /// Connects to available room and if there isn't any creates one
    /// </summary>
    public void JoinOrCreateRoom()
    {
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = MAX_PLAYERS_PER_ROOM;

        PhotonNetwork.JoinOrCreateRoom("IT Company", options, null);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        string msg = string.Format("Player {0} has joined tha game", newPlayer.NickName);
        Debug.Log(msg);
    }

    public override void OnConnectedToPhoton()
    {
        base.OnConnectedToPhoton();

        string msg = "Connected to Photon. Attempting to connecto to master server...";
        Debug.Log(msg);
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        string msg = "Connected to master server";
        Debug.Log(msg);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        string msg = string.Format("Joined game room. Max players in room: {0}", MAX_PLAYERS_PER_ROOM);
        Debug.Log(msg);
    }

    private void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings(MainGameManager.GAME_VERSION);
    }
}
