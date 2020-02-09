using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is core class for all aspects of gameplay that will
/// happen during running simulation (like ending game if gameplay
/// target is reached)
/// </summary>
public class MainSimulationManager : Photon.PunBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private PhotonView PhotonViewComponent;
    private PlayerInfo PlayerInfoComponent;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// MainGameManager component will be fetched from "GameManager" object created
    /// in menu scene. Its public field will be stored here for other objects that need
    /// to use it
    /// </summary>
    public MainGameManager GameManagerComponent { get; private set; }
    public PlayerCompany ControlledCompany { get; private set; }
    public event Action GameFinished;
    /// <summary>
    /// Called when worker is added to company of other player
    /// present in simulation
    /// </summary>
    public event MultiplayerWorkerAction OtherPlayerWorkerAdded;
    /// <summary>
    /// Called when worker is removed from company of other player
    /// present in simulation
    /// </summary>
    public event MultiplayerWorkerAction OtherPlayerWorkerRemoved;
    /// <summary>
    /// This will map ID of photon player to list of player that his company has.
    /// </summary>
    public Dictionary<PhotonPlayer, List<Worker>> OtherPlayersWorkers { get; private set; }

    /*Private methods*/

    private void InitPlayersWorkers()
    {
        OtherPlayersWorkers = new Dictionary<PhotonPlayer, List<Worker>>();

        //Init PlayersWorkers
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
        {
            //No need to get list of local player's workers
            if (PhotonNetwork.player.ID == player.ID)
            {
                continue;
            }

            OtherPlayersWorkers.Add(player, new List<Worker>());
        }
    }

    private void CreateCompany()
    {
        ControlledCompany = new PlayerCompany("", gameObject);

        ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
        ControlledCompany.Balance = GameManagerComponent.SettingsOfSimulation.InitialBalance;
        ControlledCompany.BalanceChanged += OnControlledCompanyBalanceChanged;
    }

    private void OnControlledCompanyBalanceChanged(int newBalance)
    {
        if (newBalance >= GameManagerComponent.SettingsOfSimulation.TargetBalance)
        {
            PhotonViewComponent.RPC("FinishGame", PhotonTargets.All);
        }
    }

    private void OnControlledCompanyWorkerAdded(Worker addedWorker)
    {
        this.photonView.RPC(
            "OnControlledCompanyWorkerAddedRPC", PhotonTargets.Others, addedWorker, PhotonNetwork.player.ID);
    }

    private void OnControlledCompanyWorkerRemoved(Worker removedWorker)
    {
        this.photonView.RPC(
            "OnControlledCompanyWorkerRemovedRPC", PhotonTargets.Others, removedWorker, PhotonNetwork.player.ID);
    }

    [PunRPC]
    private void OnControlledCompanyWorkerRemovedRPC(Worker removedWorker, int photonPlayerID)
    {
        KeyValuePair<PhotonPlayer, List<Worker>> workerPair = OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID);
        List<Worker> workers = workerPair.Value;
        workers.Remove(removedWorker);
        OtherPlayerWorkerRemoved?.Invoke(removedWorker, workerPair.Key);
    }

    [PunRPC]
    private void OnControlledCompanyWorkerAddedRPC(Worker addedWorker, int photonPlayerID)
    {
        KeyValuePair<PhotonPlayer, List<Worker>> workerPair = OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID);
        List<Worker> workers = workerPair.Value;
        workers.Add(addedWorker);
        OtherPlayerWorkerAdded?.Invoke(addedWorker, workerPair.Key);
    }

    [PunRPC]
    private void FinishGame()
    {
        //Stop time so events in game are no longer updated
        Time.timeScale = 0.0f;
        GameFinished?.Invoke();
    }

    /*Public methods*/

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        //Only one player is left in room so he wins the game
        if (1 == PhotonNetwork.room.PlayerCount)
        {
            FinishGame();
        }
        else
        {
            OtherPlayersWorkers.Remove(otherPlayer);
        }
    }

    public void Start()
    {
        //Obtain refence to game manager object wich was created in
        //menu scene
        GameObject gameManagerObject = GameObject.Find("GameManager");

        GameManagerComponent = gameManagerObject.GetComponent<MainGameManager>();
        PlayerInfoComponent = gameManagerObject.GetComponent<PlayerInfo>();
        PhotonViewComponent = GetComponent<PhotonView>();

        InitPlayersWorkers();
        //TEST
        CreateCompany();
    }
}
