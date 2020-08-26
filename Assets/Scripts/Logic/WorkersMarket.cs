using ExitGames.Client.Photon;
using ITCompanySimulation.Character;
using ITCompanySimulation.Multiplayer;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    /// <summary>
    /// Probability in % of adding new worker each day
    /// (only when number of workers did not reach maximum
    /// number of projects) - MaxWorkersOnMarket
    /// </summary>
    private const int WORKER_ADD_PROBABILITY_DAILY = 10;

    /*Private fields*/

    /// <summary>
    /// How many workers can be on market at one time
    /// </summary>
    private int MaxWorkersOnMarket;
    /// <summary>
    /// Used to assing unique ID for each worker
    /// </summary>
    private int WorkerID;
    private GameTime GameTimeComponent;

    /*Public consts fields*/

    /*Public fields*/

    /// <summary>
    /// How many workers should be generated in market
    /// when simulation is run in offline mode
    /// </summary>
    [Range(1.0f, 1000.0f)]
    public int NumberOfWorkersGeneratedInOfflineMode;
    /// <summary>
    /// Workers available on market
    /// </summary>
    public List<SharedWorker> Workers { get; private set; } = new List<SharedWorker>();
    public event WorkerAction WorkerAdded;
    public event WorkerAction WorkerRemoved;

    /*Private methods*/

    private int CalculateMaxWorkersOnMarket()
    {
        int maxWorkersOnMarket = PhotonNetwork.offlineMode ?
            (NumberOfWorkersGeneratedInOfflineMode) : (PhotonNetwork.room.PlayerCount * WORKERS_ON_MARKET_PER_PLAYER);

        return maxWorkersOnMarket;
    }

    private void GenerateWorkers()
    {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
        string className = this.GetType().Name;
        string debugInfo = string.Format("{0} generating {1} workers...",
           className, MaxWorkersOnMarket);
        Debug.Log(debugInfo);
#endif

        while (Workers.Count != MaxWorkersOnMarket)
        {
            SharedWorker newWorker = GenerateSingleWorker();
            Workers.Add(newWorker);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        debugInfo = string.Format("{0} generated {1} workers",
           className, this.Workers.Count);
        Debug.Log(debugInfo);
#endif
    }

    private SharedWorker GenerateSingleWorker()
    {
        //Randomly selected name and surename for new worker
        string newWorkerName =
            WorkerData.Names[UnityEngine.Random.Range(0, WorkerData.Names.Count)];
        string newWorkerSurename =
            WorkerData.Surenames[UnityEngine.Random.Range(0, WorkerData.Surenames.Count)];

        SharedWorker newMarketWorker = new SharedWorker(newWorkerName, newWorkerSurename);

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
                    float newAbilityValue = UnityEngine.Random.Range(0.0f, LocalWorker.MAX_ABILITY_VALUE);
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

    private int CalculateWorkerSalary(SharedWorker newWorker)
    {
        int workerSalary = SharedWorker.BASE_SALARY;
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

    private int CalculateWorkerExpierience(SharedWorker newWorker)
    {
        int workerExpierience = UnityEngine.Random.Range(0, 101);

        foreach (KeyValuePair<ProjectTechnology, float> workerAbility in newWorker.Abilites)
        {
            workerExpierience += (int)(45 * workerAbility.Value);
        }

        return workerExpierience;
    }

    private void Start()
    {
        //Register type for sending workers available on market to other players
        //Needed to register both base and derived class of worker because photon API requires
        //derived class to be register event when using base class argument in method
        PhotonPeer.RegisterType(typeof(LocalWorker), NetworkingData.LOCAL_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);
        PhotonPeer.RegisterType(typeof(SharedWorker), NetworkingData.SHARED_WORKER_BYTE_CODE, SharedWorker.Serialize, SharedWorker.Deserialize);

        GameTimeComponent = GetComponent<GameTime>();
        GameTimeComponent.DayChanged += OnGameTimeDayChanged;

        //Master client will generate all the workers on market
        //then send it to other clients
        if (true == PhotonNetwork.isMasterClient)
        {
            MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
            GenerateWorkers();

            foreach (SharedWorker singleWorker in Workers)
            {
                this.photonView.RPC("AddWorkerInternal", PhotonTargets.Others, singleWorker);
            }
        }
    }

    private void OnGameTimeDayChanged()
    {
        if (Workers.Count < MaxWorkersOnMarket)
        {
            int randomNuber = UnityEngine.Random.Range(0, 101);

            if (randomNuber <= WORKER_ADD_PROBABILITY_DAILY)
            {
                SharedWorker newWorker = GenerateSingleWorker();
                AddWorker(newWorker);
            }
        }
    }

    [PunRPC]
    public void AddWorkerInternal(object workerToAdd)
    {
        SharedWorker workerObject = (SharedWorker)workerToAdd;
        this.Workers.Add(workerObject);
        this.WorkerAdded?.Invoke(workerObject);
    }

    [PunRPC]
    private void RemoveWorkerInternal(int workerID)
    {
        SharedWorker workerToRemove = null;

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

    public void RemoveWorker(SharedWorker workerToRemove)
    {
        this.photonView.RPC("RemoveWorkerInternal", PhotonTargets.All, workerToRemove.ID);
    }

    public void AddWorker(SharedWorker workerToAdd)
    {
        this.photonView.RPC("AddWorkerInternal", PhotonTargets.All, workerToAdd);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        if (true == PhotonNetwork.isMasterClient)
        {
            foreach (LocalWorker singleWorker in Workers)
            {
                this.photonView.RPC("AddWorkerInternal", newPlayer, singleWorker);
            }
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
    }
}