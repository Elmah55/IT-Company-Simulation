using System;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Events;
using ITCompanySimulation.Core;
using System.Linq;
using ITCompanySimulation.Project;
using Photon;

namespace ITCompanySimulation.Character
{
    /// <summary>
    /// This class handles represents workers market. Each of player can hire worker
    /// from market. Workers market is shared across all players
    /// </summary>
    public class WorkersMarket : PunBehaviour
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
        private GameTime GameTimeComponent;
        private ApplicationManager ApplicationManagerComponent;
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private DataTransferEvent InitialDataReceivedEvent;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// How many workers should be generated in market
        /// when simulation is run in offline mode.
        /// </summary>
        [Range(1.0f, 1000.0f)]
        public int NumberOfWorkersGeneratedInOfflineMode;
        /// <summary>
        /// Workers available on market. Key is ID of worker and value is SharedWoker instance.
        /// </summary>
        public Dictionary<int, SharedWorker> Workers { get; private set; } = new Dictionary<int, SharedWorker>();
        public WorkerGenerationData GenerationData;

        public event SharedWorkerAction WorkerAdded;
        public event SharedWorkerAction WorkerRemoved;

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
                Workers.Add(newWorker.ID, newWorker);
            }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            debugInfo = string.Format("[{0}] generated {1} workers",
               className, Workers.Count);
            Debug.Log(debugInfo);
#endif
        }

        private SharedWorker GenerateSingleWorker()
        {
            //Randomly selected name, surename and gender for new worker
            Gender newWorkerGender = (Gender)UnityEngine.Random.Range(0, NUMBER_OF_GENDERS);

            int nameIndex;
            string newWorkerName = CreateWorkerName(newWorkerGender, out nameIndex);
            int surenameIndex = UnityEngine.Random.Range(0, GenerationData.Surenames.Length);
            string newWorkerSurename = GenerationData.Surenames[surenameIndex];

            SharedWorker newMarketWorker =
                new SharedWorker(newWorkerName, newWorkerSurename, newWorkerGender);

            newMarketWorker.Abilites = CreateWorkerAbilities();
            newMarketWorker.ExperienceTime = CalculateWorkerExpierience(newMarketWorker);
            newMarketWorker.Salary = CalculateWorkerSalary(newMarketWorker);
            newMarketWorker.ID = SimulationManager.GenerateID();
            int avatarIndex;
            newMarketWorker.Avatar = CreateWorkerAvatar(newMarketWorker, out avatarIndex);
            newMarketWorker.AvatarIndex = avatarIndex;
            newMarketWorker.NameIndex = nameIndex;
            newMarketWorker.SurenameIndex = surenameIndex;

            return newMarketWorker;
        }

        private string CreateWorkerName(Gender workerGender, out int nameIndex)
        {
            string newWorkerName;

            if (Gender.Male == workerGender)
            {
                nameIndex = UnityEngine.Random.Range(0, GenerationData.MaleNames.Length);
                newWorkerName = GenerationData.MaleNames[nameIndex];
            }
            else
            {
                nameIndex = UnityEngine.Random.Range(0, GenerationData.FemaleNames.Length);
                newWorkerName = GenerationData.FemaleNames[nameIndex];
            }

            return newWorkerName;
        }

        private Sprite CreateWorkerAvatar(SharedWorker worker, out int avatarIndex)
        {
            Sprite avatar = null;
            int randomIndex = UnityEngine.Random.Range(0,
                worker.Gender == Gender.Male ?
                GenerationData.MaleCharactersAvatars.Length - 1 : GenerationData.FemaleCharactersAvatars.Length - 1);
            avatarIndex = randomIndex;

            if (Gender.Male == worker.Gender)
            {
                avatar = GenerationData.MaleCharactersAvatars[randomIndex];
            }
            else
            {
                avatar = GenerationData.FemaleCharactersAvatars[randomIndex];
            }

            return avatar;
        }

        private Dictionary<ProjectTechnology, SafeFloat> CreateWorkerAbilities()
        {
            Dictionary<ProjectTechnology, SafeFloat> workerAbilities = new Dictionary<ProjectTechnology, SafeFloat>();

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
                            workerAbilities.Add(newAbility, new SafeFloat(newAbilityValue));
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

            foreach (KeyValuePair<ProjectTechnology, SafeFloat> workerAbility in newWorker.Abilites)
            {
                abilitiesBonus += (int)(workerAbility.Value.Value * 30.0f);
            }

            workerSalary += expierienceBonus;
            workerSalary += abilitiesBonus;

            return workerSalary;
        }

        private int CalculateWorkerExpierience(SharedWorker newWorker)
        {
            int workerExpierience = UnityEngine.Random.Range(0, 101);

            foreach (KeyValuePair<ProjectTechnology, SafeFloat> workerAbility in newWorker.Abilites)
            {
                workerExpierience += (int)(45 * workerAbility.Value.Value);
            }

            return workerExpierience;
        }

        private void Start()
        {
            GameTimeComponent = GetComponent<GameTime>();
            GameTimeComponent.DayChanged += OnGameTimeDayChanged;
            ApplicationManagerComponent =
                GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
            SimulationManagerComponent = GetComponent<SimulationManager>();

            //Master client will generate all the workers on market
            //then send it to other clients
            if (true == PhotonNetwork.isMasterClient)
            {
                MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
                this.photonView.RPC("SetMaxWorkersOnMarketRPC", PhotonTargets.Others, MaxWorkersOnMarket);
                GenerateAndSendWorkers();
                InitialDataReceivedEvent.RaiseEvent(DataTransferSource.WorkersMarket);
            }
        }

        private void GenerateAndSendWorkers()
        {
            GenerateWorkers();
            SharedWorker[] generatedWorkers = Workers.Values.ToArray();
            this.photonView.RPC("OnWorkersGeneratedRPC", PhotonTargets.Others, generatedWorkers);
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

        /// <summary>
        /// Called when master client has generated workers in the market
        /// at the beggining of simulation.
        /// </summary>
        [PunRPC]
        private void OnWorkersGeneratedRPC(SharedWorker[] generatedWorkers)
        {
            foreach (SharedWorker worker in generatedWorkers)
            {
                this.Workers.Add(worker.ID, worker);
                this.WorkerAdded?.Invoke(worker);
            }

            if (false == PhotonNetwork.isMasterClient)
            {
                if (this.Workers.Count == MaxWorkersOnMarket)
                {
                    InitialDataReceivedEvent.RaiseEvent(DataTransferSource.WorkersMarket);
                }
            }
        }

        [PunRPC]
        private void OnWorkerAddedRPC(SharedWorker workerToAdd)
        {
            this.Workers.Add(workerToAdd.ID, workerToAdd);
            this.WorkerAdded?.Invoke(workerToAdd);
        }

        [PunRPC]
        private void OnWorkerRemovedRPC(int workerID)
        {
            SharedWorker workerToRemove = Workers[workerID];
            Workers.Remove(workerID);
            this.WorkerRemoved?.Invoke(workerToRemove);
        }

        /// <summary>
        /// Called by client that requested worker from market.
        /// </summary>
        /// <param name="requestedWorkerID">ID of requested worker</param>
        /// <param name="photonPlayerID">ID of photon player that is requesting worker</param>
        [PunRPC]
        private void OnWorkerRequestRPC(int requestedWorkerID, int photonPlayerID)
        {
            //Concept of requesting worker is introduced so master client can decide
            //which client should receive worker in case two RPCs from different clients
            //arrive in short time

            PhotonPlayer targetPlayer = Utils.PhotonPlayerFromID(photonPlayerID);

            if (true == Workers.ContainsKey(requestedWorkerID))
            {
                SharedWorker requestedWorker = Workers[requestedWorkerID];
                this.photonView.RPC("OnWorkerRequestCfmRPC", targetPlayer, requestedWorkerID);
                this.photonView.RPC("OnWorkerRemovedRPC", PhotonTargets.All, requestedWorkerID);
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            else
            {
                Debug.LogWarningFormat("[{0}] Worker (ID {1}) requested from Player: {2} (ID {3}) but worker" +
                                        "does not exists in market anymore",
                                        this.GetType().Name,
                                        requestedWorkerID,
                                        targetPlayer.NickName,
                                        targetPlayer.ID);
            }
#endif
        }

        /// <summary>
        /// Called when this client request for worker is confirmed.
        /// </summary>
        /// <param name="requestedWorkerID">ID of requested worker</param>
        [PunRPC]
        private void OnWorkerRequestCfmRPC(int requestedWorkerID)
        {
            SharedWorker requestedWorker = Workers[requestedWorkerID];
            LocalWorker requestedLocalWorker = new LocalWorker(requestedWorker);
            SimulationManagerComponent.ControlledCompany.AddWorker(requestedLocalWorker);
        }

        /// <summary>
        /// Removes all workers from market
        /// </summary>
        [PunRPC]
        private void ClearWorkersRPC()
        {
            SharedWorker[] removedWorkers = Workers.Values.ToArray();
            Workers.Clear();

            foreach (SharedWorker worker in removedWorkers)
            {
                WorkerRemoved?.Invoke(worker);
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

        /// <summary>
        /// Request worker from master client. If request was granted to this client
        /// RPC with confirmation will be called
        /// </summary>
        /// <param name="workerToRemove">Requested worker</param>
        public void RequestWorker(SharedWorker workerToRemove)
        {
            this.photonView.RPC("OnWorkerRequestRPC",
                                PhotonNetwork.masterClient,
                                workerToRemove.ID,
                                PhotonNetwork.player.ID);
        }

        public void AddWorker(SharedWorker workerToAdd)
        {
            this.photonView.RPC("OnWorkerAddedRPC", PhotonTargets.All, workerToAdd);
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);

            MaxWorkersOnMarket = CalculateMaxWorkersOnMarket();
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            if (true == PhotonNetwork.isMasterClient && 0 != PhotonNetwork.otherPlayers.Length)
            {
                //Number of players might have changed, recalculate max workers on market
                int maxWorkers = CalculateMaxWorkersOnMarket();
                this.photonView.RPC("SetMaxWorkersOnMarketRPC", PhotonTargets.All, maxWorkers);

                //If master client was switched during sending data, generate and send
                //workers again
                if (false == ApplicationManagerComponent.IsSessionActive)
                {
                    //Remove workers from other clients that this client might have not received so all clients'
                    //workers are synchronized
                    this.photonView.RPC("ClearWorkersRPC", PhotonTargets.All);
                    GenerateAndSendWorkers();
                }
            }
        }
    }
}