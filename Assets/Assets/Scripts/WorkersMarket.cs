using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;

/// <summary>
/// This class handles represents workers market. Each of player can hire worker
/// from market. Workers market is shared across all players
/// </summary>
public class WorkersMarket : Photon.PunBehaviour
{
    /*Private consts fields*/

    private readonly int NUMBER_OF_PROJECT_TECHNOLOGIES =
        Enum.GetValues(typeof(ProjectTechnology)).Length;
    /// <summary>
    /// How many abilities created worker can have
    /// </summary>
    private const int MAX_NUMBER_OF_NEW_WORKER_ABILITIES = 5;
    /// <summary>
    /// How many workers should be added to market
    /// per one player in game
    /// </summary>
    private const int WORKERS_ON_MARKET_PER_PLAYER = 3;

    /*Private fields*/

    /// <summary>
    /// How many workers can be on market at one time
    /// </summary>
    private int MaxWorkersOnMarket;
    /// <summary>
    /// Used to assing unique ID for each worker
    /// </summary>
    private int WorkerID;
    private PhotonView PhotonViewComponent;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// Workers available on market
    /// </summary>
    public List<Worker> Workers { get; private set; }
    public event WorkerAction WorkerAdded;
    public event WorkerAction WorkerRemoved;

    /*Private methods*/

    private int CalculateMaxWorkersOnMarket()
    {
        int value = PhotonNetwork.room.PlayerCount * WORKERS_ON_MARKET_PER_PLAYER;
        return value;
    }

    private void GenerateWorkers()
    {
        while (Workers.Count != MaxWorkersOnMarket)
        {
            Worker newWorker = GenerateSingleWorker();
            Workers.Add(newWorker);
        }
    }

    private Worker GenerateSingleWorker()
    {
        //Randomly selected name and surename for new worker
        string newWorkerName =
            WorkerData.Names[UnityEngine.Random.Range(0, WorkerData.Names.Count)];
        string newWorkerSurename =
            WorkerData.Surenames[UnityEngine.Random.Range(0, WorkerData.Surenames.Count)];

        Worker newMarketWorker = new Worker(newWorkerName, newWorkerSurename);

        newMarketWorker.Abilites = CreateWorkerAbilities();
        newMarketWorker.ExperienceTime = CalculateWorkerExpierience(newMarketWorker);
        newMarketWorker.Salary = CalculateWorkerSalary(newMarketWorker);
        newMarketWorker.ID = WorkerID++;

        if (WorkerID < newMarketWorker.ID)
        {
            throw new OverflowException("Maximum number of generated workers exceeded");
        }

        return newMarketWorker;
    }

    private Dictionary<ProjectTechnology, float> CreateWorkerAbilities()
    {
        Dictionary<ProjectTechnology, float> workerAbilities = new Dictionary<ProjectTechnology, float>();

        //Value that will be used when adding abilities for
        //worker. After each ability added this value will be
        //decreased so there is less chance for new ability to be
        //added. Created worker will have at least one ability so 1st
        //ability will be added with 100% probability
        int abilityAddTreshold = 100;

        for (int i = 0; i < NUMBER_OF_PROJECT_TECHNOLOGIES; i++)
        {
            int randomNum = UnityEngine.Random.Range(0, 101);

            if (abilityAddTreshold >= randomNum)
            {
                bool uniqueAbility = false;

                while (true != uniqueAbility)
                {
                    int abilityIndex = UnityEngine.Random.Range(0, NUMBER_OF_PROJECT_TECHNOLOGIES);
                    ProjectTechnology newAbility = (ProjectTechnology)abilityIndex;
                    float newAbilityValue = UnityEngine.Random.Range(0.0f, Worker.MAX_ABILITY_VALUE);
                    uniqueAbility = (false == workerAbilities.ContainsKey(newAbility));

                    if (true == uniqueAbility)
                    {
                        workerAbilities.Add(newAbility, newAbilityValue);
                    }
                }
            }

            abilityAddTreshold -= 10 * (i + 1);
        }

        return workerAbilities;
    }

    private int CalculateWorkerSalary(Worker newWorker)
    {
        int workerSalary = Worker.BASE_SALARY;
        int expierienceBonus = newWorker.ExperienceTime * 6;
        int abilitiesBonus = 0;

        foreach (KeyValuePair<ProjectTechnology, float> workerAbility in newWorker.Abilites)
        {
            abilitiesBonus += (int)(workerAbility.Value * 30.0f);
        }

        workerSalary += expierienceBonus;
        workerSalary += abilitiesBonus;

        return workerSalary;
    }

    private int CalculateWorkerExpierience(Worker newWorker)
    {
        int workerExpierience = UnityEngine.Random.Range(0, 101);

        foreach (KeyValuePair<ProjectTechnology, float> workerAbility in newWorker.Abilites)
        {
            workerExpierience += (int)(20 * workerAbility.Value);
        }

        return workerExpierience;
    }

    private void Start()
    {
        PhotonViewComponent = GetComponent<PhotonView>();
        Workers = new List<Worker>();
        //Register type for sending workers available on market to other players
        PhotonPeer.RegisterType(typeof(Worker), 0, Worker.Serialize, Worker.Deserialize);

        //Master client will generate all the workers on market
        //then send it to other clients
        if (true == PhotonNetwork.isMasterClient)
        {
            MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
            GenerateWorkers();

            foreach (Worker singleWorker in Workers)
            {
                PhotonViewComponent.RPC("AddWorkerInternal", PhotonTargets.Others, singleWorker);
            }
        }
    }

    [PunRPC]
    public void AddWorkerInternal(object workerToAdd)
    {
        Worker workerObject = (Worker)workerToAdd;
        this.Workers.Add(workerObject);
        this.WorkerAdded?.Invoke(workerObject);
    }

    [PunRPC]
    private void RemoveWorkerInternal(int workerID)
    {
        Worker workerToRemove = null;

        for (int i = 0; i < Workers.Count; i++)
        {
            if (workerID == Workers[i].ID)
            {
                workerToRemove = Workers[i];
                Workers.RemoveAt(i);
                break;
            }
        }

        this.WorkerRemoved?.Invoke(workerToRemove);
    }

    /*Public methods*/

    public void RemoveWorker(Worker workerToRemove)
    {
        PhotonViewComponent.RPC("RemoveWorkerInternal", PhotonTargets.All, workerToRemove.ID);
    }

    public void AddWorker(Worker workerToAdd)
    {
        PhotonViewComponent.RPC("AddWorkerInternal", PhotonTargets.All, workerToAdd);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        if (true == PhotonNetwork.isMasterClient)
        {
            foreach (Worker singleWorker in Workers)
            {
                PhotonViewComponent.RPC("AddWorkerInternal", newPlayer, singleWorker);
            }
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();

        //Remove additional workers from market so workers count
        //matches new number of players
        while (Workers.Count > MaxWorkersOnMarket)
        {
            Workers.RemoveAt(Workers.Count - 1);
        }
    }
}