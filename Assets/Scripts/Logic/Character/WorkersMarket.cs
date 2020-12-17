using ITCompanySimulation.Character;
using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Utilities;
using UnityEngine.Events;

/// <summary>
/// This class handles represents workers market. Each of player can hire worker
/// from market. Workers market is shared across all players
/// </summary>
public class WorkersMarket : Photon.PunBehaviour, IDataReceiver
{
    /*Private consts fields*/

    private readonly int NUMBER_OF_PROJECT_TECHNOLOGIES =
        Enum.GetValues(typeof(ProjectTechnology)).Length;
    private const int NUMBER_OF_GENDERS = 2;
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
    [SerializeField]
    private Sprite[] WorkersAvatarsMale;
    [SerializeField]
    private Sprite[] WorkersAvatarsFemale;

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
    public bool IsDataReceived { get; private set; }

    public event SharedWorkerAction WorkerAdded;
    public event SharedWorkerAction WorkerRemoved;
    public event UnityAction DataReceived;

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
        int workersToGenerate = MaxWorkersOnMarket - Workers.Count;
        string className = this.GetType().Name;
        string debugInfo = string.Format("[{0}] generating {1} workers...",
           className, workersToGenerate);
        Debug.Log(debugInfo);
#endif

        while (Workers.Count != MaxWorkersOnMarket)
        {
            SharedWorker newWorker = GenerateSingleWorker();
            Workers.Add(newWorker);
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        debugInfo = string.Format("[{0}] generated {1} workers",
           className, workersToGenerate);
        Debug.Log(debugInfo);
#endif
    }

    private SharedWorker GenerateSingleWorker()
    {
        //Randomly selected name, surename and gender for new worker
        Gender newWorkerGender = (Gender)UnityEngine.Random.Range(0, NUMBER_OF_GENDERS);
        string newWorkerName;

        if (Gender.Male == newWorkerGender)
        {
            newWorkerName = WorkerData.MaleNames[UnityEngine.Random.Range(0, WorkerData.MaleNames.Count)];
        }
        else
        {
            newWorkerName = WorkerData.FemaleNames[UnityEngine.Random.Range(0, WorkerData.FemaleNames.Count)];
        }

        string newWorkerSurename =
            WorkerData.Surenames[UnityEngine.Random.Range(0, WorkerData.Surenames.Count)];

        SharedWorker newMarketWorker = new SharedWorker(newWorkerName, newWorkerSurename, newWorkerGender);

        newMarketWorker.Abilites = CreateWorkerAbilities();
        newMarketWorker.ExperienceTime = CalculateWorkerExpierience(newMarketWorker);
        newMarketWorker.Salary = CalculateWorkerSalary(newMarketWorker);
        newMarketWorker.ID = WorkerID++;
        newMarketWorker.Avatar = CreateWorkerAvatar(newMarketWorker);

        if (WorkerID < newMarketWorker.ID)
        {
            throw new OverflowException("Maximum number of generated workers exceeded");
        }

        return newMarketWorker;
    }

    private Sprite CreateWorkerAvatar(SharedWorker worker)
    {
        Sprite avatar = null;
        int randomIndex = UnityEngine.Random.Range(0, WorkersAvatarsMale.Length - 1);

        if (Gender.Male == worker.Gender)
        {
            avatar = WorkersAvatarsMale[randomIndex];
        }
        else
        {
            avatar = WorkersAvatarsFemale[randomIndex];
        }

        return avatar;
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
        GameTimeComponent = GetComponent<GameTime>();
        GameTimeComponent.DayChanged += OnGameTimeDayChanged;

        //Master client will generate all the workers on market
        //then send it to other clients
        if (true == PhotonNetwork.isMasterClient)
        {
            MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
            this.photonView.RPC("SetMaxWorkersOnMarketRPC", PhotonTargets.Others, MaxWorkersOnMarket);
            GenerateAndSendWorkers();
        }
    }

    private void GenerateAndSendWorkers()
    {
        GenerateWorkers();

        foreach (SharedWorker singleWorker in Workers)
        {
            this.photonView.RPC("AddWorkerInternalRPC", PhotonTargets.Others, singleWorker);
        }
    }

    private void OnGameTimeDayChanged()
    {
        if (true == PhotonNetwork.isMasterClient && Workers.Count < MaxWorkersOnMarket)
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
    public void AddWorkerInternalRPC(object workerToAdd)
    {
        SharedWorker workerObject = (SharedWorker)workerToAdd;
        this.Workers.Add(workerObject);
        this.WorkerAdded?.Invoke(workerObject);

        if (false == PhotonNetwork.isMasterClient && false == IsDataReceived)
        {
            if (this.Workers.Count == MaxWorkersOnMarket)
            {
                IsDataReceived = true;
                DataReceived?.Invoke();
            }
        }
    }

    [PunRPC]
    private void RemoveWorkerInternalRPC(int workerID)
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

    /// <summary>
    /// Truncates workers list so its length matches provided parameter
    /// Does nothing if provided parameter is equal or greater than number of workers
    /// </summary>
    [PunRPC]
    private void TruncateWorkersRPC(int count)
    {
        if (count < this.Workers.Count)
        {
            while (this.Workers.Count != count)
            {
                this.Workers.RemoveAt(this.Workers.Count - 1);
            }
        }
    }

    /// <summary>
    /// Used by master client to inform other clients about number of
    /// market workers that will be generated by master client
    /// </summary>
    [PunRPC]
    private void SetMaxWorkersOnMarketRPC(int numberOfWorkers)
    {
        this.MaxWorkersOnMarket = numberOfWorkers;
    }

    /*Public methods*/

    public void RemoveWorker(SharedWorker workerToRemove)
    {
        this.photonView.RPC("RemoveWorkerInternalRPC", PhotonTargets.All, workerToRemove.ID);
    }

    public void AddWorker(SharedWorker workerToAdd)
    {
        this.photonView.RPC("AddWorkerInternalRPC", PhotonTargets.All, workerToAdd);
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        base.OnPhotonPlayerConnected(newPlayer);

        if (true == PhotonNetwork.isMasterClient)
        {
            foreach (SharedWorker singleWorker in Workers)
            {
                this.photonView.RPC("AddWorkerInternalRPC", newPlayer, singleWorker);
            }
        }
    }

    public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        base.OnPhotonPlayerDisconnected(otherPlayer);

        MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
    }

    public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);

        //If master client was switcher during sending data, generate ramining workers and
        //send to other players
        if (true == PhotonNetwork.isMasterClient && 0 != PhotonNetwork.otherPlayers.Length)
        {
            //Number of players might have changed, recalculate max workers on market
            MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
            //Remove workers from other clients that this client might have not received so all clients'
            //workers are synchronized
            this.photonView.RPC("TruncateWorkersRPC", PhotonTargets.Others, this.Workers.Count);
            this.photonView.RPC("SetMaxWorkersOnMarketRPC", PhotonTargets.Others, MaxWorkersOnMarket);

            //Remove excessive workers. If there are not enough workers, generate more
            if (this.Workers.Count > MaxWorkersOnMarket)
            {
                TruncateWorkersRPC(MaxWorkersOnMarket);
            }
            else
            {
                GenerateAndSendWorkers();
            }
        }
    }
}