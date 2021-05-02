using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Character;
using ITCompanySimulation.UI;
using ITCompanySimulation.Multiplayer;
using ITCompanySimulation.Company;
using ITCompanySimulation.Developing;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This is core class for all aspects of gameplay that will
    /// happen during running simulation (like ending game if gameplay
    /// target is reached)
    /// </summary>
    public class SimulationManager : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private GameTime GameTimeComponent;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        /// <summary>
        /// Game object of UI element with simulation statistics
        /// </summary>
        [SerializeField]
        private GameObject SimulationStatsUI;
        /// <summary>
        /// Used for generating IDs of items
        /// </summary>
        private static int m_ID;
        /// <summary>
        /// Number of seconds passed from application start to simulation start
        /// </summary>
        private float SimulationStartTimestamp;
        private ApplicationManager ApplicationManagerComponent;

        /*Public consts fields*/

        /*Public fields*/

        public PlayerCompany ControlledCompany { get; private set; }
        public SimulationEventNotificator NotificatorComponent { get; private set; }
        /// <summary>
        /// This will map ID of photon player to list of player that his company has.
        /// </summary>
        public Dictionary<PhotonPlayer, List<SharedWorker>> OtherPlayersWorkers { get; private set; }
        /// <summary>
        /// True when any of player has won the game
        /// </summary>
        public bool IsSimulationFinished { get; private set; }
        /// <summary>
        /// ID of PhotonPlayer object of winner player valid only
        /// when IsSimulationFinished is true
        /// </summary>
        public PhotonPlayer WinnerPlayer { get; private set; }
        /// <summary>
        /// Reason of simulation finish. This field is only valid when IsSimulationFinished is
        /// set to true.
        /// </summary>
        public SimulationFinishReason FinishReason { get; private set; }
        public bool IsSimulationActive { get; private set; }
        /// <summary>
        /// Simulation statistics collected during this session
        /// </summary>
        public SimulationStats Stats { get; private set; }

        public event SimulationFinishAction SimulationFinished;
        public event Action SimulationStarted;
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
        /// Called when company of other player reaches minimal balance defined in settings
        /// of simulation
        /// </summary>
        public event PhotonPlayerAction OtherPlayerCompanyMinimalBalanceReached;

        /*Private methods*/

        private void Start()
        {
            InfoWindowComponent.Show("Waiting for session start...");
            ApplicationManagerComponent.SessionStarted += OnGameManagerComponentSessionStarted;
        }

        private void Awake()
        {
            ApplicationManagerComponent = GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();
            GameTimeComponent = GetComponent<GameTime>();
            NotificatorComponent = new SimulationEventNotificator(GameTimeComponent);
            OtherPlayersWorkers = new Dictionary<PhotonPlayer, List<SharedWorker>>();
            Stats = new SimulationStats();

            InitPlayersWorkers();
            //TEST
            CreateCompany();
        }

        private void InitPlayersWorkers()
        {
            //Init PlayersWorkers
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                //No need to get list of local player's workers
                if (PhotonNetwork.player.ID == player.ID)
                {
                    continue;
                }

                OtherPlayersWorkers.Add(player, new List<SharedWorker>());
            }
        }

        private void CreateCompany()
        {
            ControlledCompany = new PlayerCompany("", gameObject);

            ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
            ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
            ControlledCompany.BalanceChanged += OnControlledCompanyBalanceChanged;
            ControlledCompany.ProjectAdded += OnControlledCompanyProjectAdded;
            ControlledCompany.Balance = SimulationSettings.InitialBalance;
        }

        private void StartSimulation()
        {
            InfoWindowComponent.Hide();
            GameTimeComponent.StartTime();
            IsSimulationActive = true;
            SimulationStarted?.Invoke();
            SimulationStartTimestamp = Time.realtimeSinceStartup;
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            scrumObj.BindedProject.Completed += (proj) =>
              {
                  Stats.ProjectsCompleted++;
              };
        }

        private void OnControlledCompanyBalanceChanged(int newBalance, int balanceDelta)
        {
            if (balanceDelta >= 0)
            {
                Stats.MoneyEarned += balanceDelta;
            }
            else
            {
                Stats.MoneySpent += Mathf.Abs(balanceDelta);
            }

            if (newBalance >= SimulationSettings.TargetBalance)
            {
                this.photonView.RPC("FinishSimulationRPC",
                                    PhotonTargets.All,
                                    PhotonNetwork.player.ID,
                                    (int)SimulationFinishReason.PlayerCompanyReachedTargetBalance);
            }
            else if (newBalance <= SimulationSettings.MinimalBalance)
            {
                SimulationFinishReason finishReason = SimulationFinishReason.PlayerCompanyReachedMinimalBalance;

                this.photonView.RPC("OnOtherPlayerControlledCompanyMinimalBalanceReachedRPC",
                                    PhotonTargets.Others,
                                    PhotonNetwork.player.ID);

                FinishSimulationRPC(PhotonNetwork.player.ID, (int)finishReason);
            }
        }

        private void OnGameManagerComponentSessionStarted()
        {
            StartSimulation();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogFormat("[{0}] Session started. Starting simulation with time scale {1}",
                this.GetType().Name, GameTimeComponent.Scale);
#endif
        }

        #region Workers events

        private void OnControlledCompanyWorkerAdded(SharedWorker addedWorker)
        {
            this.photonView.RPC(
                "OnControlledCompanyWorkerAddedRPC", PhotonTargets.Others, addedWorker, PhotonNetwork.player.ID);

            addedWorker.SalaryChanged += OnCompanyWorkerSalaryChanged;
            addedWorker.AbilityUpdated += OnCompanyWorkerAbilityUpdated;
            addedWorker.ExpierienceTimeChanged += OnCompanyWorkerExpierienceTimeChanged;

            Stats.WorkersHired++;
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker removedWorker)
        {
            this.photonView.RPC(
                "OnControlledCompanyWorkerRemovedRPC", PhotonTargets.Others, removedWorker.ID, PhotonNetwork.player.ID);
            removedWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
            removedWorker.AbilityUpdated -= OnCompanyWorkerAbilityUpdated;
            removedWorker.ExpierienceTimeChanged -= OnCompanyWorkerExpierienceTimeChanged;

            Stats.WorkersLeftCompany++;
        }

        private void OnCompanyWorkerExpierienceTimeChanged(SharedWorker companyWorker)
        {
            this.photonView.RPC("OnOtherPlayerCompanyWorkerAttirbuteChangedRPC",
                    PhotonTargets.Others,
                    PhotonNetwork.player.ID,
                    companyWorker.ID,
                    companyWorker.ExperienceTime,
                    (int)WorkerAttribute.ExpierienceTime);
        }

        private void OnCompanyWorkerAbilityUpdated(SharedWorker companyWorker, ProjectTechnology workerAbility, float workerAbilityValue)
        {
            this.photonView.RPC("OnOtherPlayerCompanyWorkerAbilityChangedRPC",
                                PhotonTargets.Others,
                                PhotonNetwork.player.ID,
                                companyWorker.ID,
                                (int)workerAbility,
                                workerAbilityValue);
        }

        private void OnCompanyWorkerSalaryChanged(SharedWorker companyWorker)
        {
            this.photonView.RPC("OnOtherPlayerCompanyWorkerAttirbuteChangedRPC",
                                PhotonTargets.Others,
                                PhotonNetwork.player.ID,
                                companyWorker.ID,
                                companyWorker.Salary,
                                (int)WorkerAttribute.Salary);
        }

        /// <summary>
        /// This method is called when ability of worker of other player is updated so it can be
        /// also updated in this client's list of other player's workers
        /// </summary>
        [PunRPC]
        private void OnOtherPlayerCompanyWorkerAbilityChangedRPC(int photonPlayerID, int workerID, int updatedAbility, float updatedAbilityValue)
        {
            SharedWorker updatedWorker =
                OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID).Value.First(x => x.ID == workerID);
            ProjectTechnology updatedAbilityEnum = (ProjectTechnology)updatedAbility;
            updatedWorker.UpdateAbility(updatedAbilityEnum, updatedAbilityValue);
        }

        /// <summary>
        /// This method is called when salary of worker of other player is updated so it can be
        /// also updated in this client's list of other player's workers
        /// </summary>
        [PunRPC]
        private void OnOtherPlayerCompanyWorkerAttirbuteChangedRPC(int photonPlayerID, int workerID, object attributeValue, int changedAttirbute)
        {
            WorkerAttribute attribute = (WorkerAttribute)changedAttirbute;
            SharedWorker updatedWorker =
                OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID).Value.First(x => x.ID == workerID);

            switch (attribute)
            {
                case WorkerAttribute.Salary:
                    int newSalary = (int)attributeValue;
                    updatedWorker.Salary = newSalary;
                    break;
                case WorkerAttribute.ExpierienceTime:
                    int newExpierienceTime = (int)attributeValue;
                    updatedWorker.ExperienceTime = newExpierienceTime;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Called by client that removed worker from controlled company so other clients can update list
        /// of other players' workers
        /// </summary>
        [PunRPC]
        private void OnControlledCompanyWorkerRemovedRPC(int removedWorkerID, int photonPlayerID)
        {
            KeyValuePair<PhotonPlayer, List<SharedWorker>> workerPair = OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID);
            List<SharedWorker> workers = workerPair.Value;
            SharedWorker localRemovedWorker = workers.Find(x => x.ID == removedWorkerID);
            workers.Remove(localRemovedWorker);
            OtherPlayerWorkerRemoved?.Invoke(localRemovedWorker, workerPair.Key);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("[{3}] Received worker update from player {0} (ID: {1}).\n" +
                "Worker removed (ID: {2}). Local workers collection synchronized",
                workerPair.Key.NickName, workerPair.Key.ID, removedWorkerID, this.GetType().Name);
            Debug.Log(debugInfo);
#endif
        }

        /// <summary>
        /// Called by client that added worker to controlled company so other clients can update list
        /// of other players' workers
        /// </summary>
        [PunRPC]
        private void OnControlledCompanyWorkerAddedRPC(SharedWorker addedWorker, int photonPlayerID)
        {
            KeyValuePair<PhotonPlayer, List<SharedWorker>> workerPair = OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID);
            List<SharedWorker> workers = workerPair.Value;
            workers.Add(addedWorker);
            OtherPlayerWorkerAdded?.Invoke(addedWorker, workerPair.Key);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("[{3}] Received worker update from player {0} (ID: {1}).\n" +
                "Worker added (ID: {2}). Local workers collection synchronized",
                workerPair.Key.NickName, workerPair.Key.ID, addedWorker.ID, this.GetType().Name);
            Debug.Log(debugInfo);
#endif
        }

        #endregion

        [PunRPC]
        private void OnOtherPlayerControlledCompanyMinimalBalanceReachedRPC(int photonPlayerID)
        {
            KeyValuePair<PhotonPlayer, List<SharedWorker>> workerPair = OtherPlayersWorkers.First(x => x.Key.ID == photonPlayerID);
            this.OtherPlayerCompanyMinimalBalanceReached?.Invoke(workerPair.Key);
        }

        [PunRPC]
        private void RemoveControlledCompanyWorkerRPC(int workerID, int senderID)
        {
            LocalWorker workerToRemove = this.ControlledCompany.Workers.First(x => x.ID == workerID);
            this.ControlledCompany.RemoveWorker(workerToRemove);

            PhotonPlayer sender = PhotonNetwork.playerList.FirstOrDefault(x => x.ID == senderID);
            string notification;
            notification = string.Format("Player {0} hired your worker !",
                //Check if player didnt disconnect since sending RPC
                default(PhotonPlayer) == sender ? string.Empty : sender.NickName);
            NotificatorComponent.Notify(notification);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string debugInfo = string.Format("[{3}] Player {0} (ID: {1}) removed worker ID: {2} from your company",
                sender.NickName, sender.ID, workerToRemove.ID, this.GetType().Name);
            Debug.Log(debugInfo);
#endif
        }

        [PunRPC]
        private void FinishSimulationRPC(int winnerPhotonPlayerID, int finishReason)
        {
            //Check if simulation has been already finished.
            //Simulation finish can be triggered by few events (see SimulationFinishReason enum)
            if (false == IsSimulationFinished)
            {
                //Stop time so events in game are no longer updated
                GameTimeComponent.StopTime();
                IsSimulationFinished = true;
                IsSimulationActive = false;
                this.WinnerPlayer = PhotonNetwork.playerList.FirstOrDefault(x => x.ID == winnerPhotonPlayerID);
                this.FinishReason = (SimulationFinishReason)finishReason;
                string finishSimulationInfoMsg = "Game finished";

                //Check if player didnt left already
                if (null != WinnerPlayer)
                {
                    switch (this.FinishReason)
                    {
                        case SimulationFinishReason.PlayerCompanyReachedTargetBalance:
                            string winnerInfo = (WinnerPlayer.IsLocal ? "You have" : WinnerPlayer.NickName) + " won";
                            finishSimulationInfoMsg = string.Format("Game finished !\n{0}", winnerInfo);
                            break;
                        case SimulationFinishReason.PlayerCompanyReachedMinimalBalance:
                            finishSimulationInfoMsg = string.Format("Your company's balance reached minimum allowed balance ({0} $)",
                            SimulationSettings.MinimalBalance);
                            break;
                        case SimulationFinishReason.OnePlayerInRoom:
                            finishSimulationInfoMsg = "You are the only player left in simulation.\nYou have won !";
                            break;
                        case SimulationFinishReason.Disconnected:
                            finishSimulationInfoMsg = "You have been disconnected from server";
                            break;
                        default:
                            break;
                    }
                }

                SimulationFinished?.Invoke(winnerPhotonPlayerID, (SimulationFinishReason)finishReason);
                ApplicationManagerComponent.FinishSession();
                InfoWindowComponent.ShowOk(finishSimulationInfoMsg, () => SimulationStatsUI.SetActive(true));

                float simulationRunningTime = Time.realtimeSinceStartup - SimulationStartTimestamp;
                Stats.SimulationRunningTime = TimeSpan.FromSeconds(simulationRunningTime);
                Stats.DaysSinceStart = GameTimeComponent.DaysSinceStart;
            }
        }

        /*Public methods*/

        /// <summary>
        /// Removes worker with given ID from company of target PhotonPlayer
        /// </summary>
        public void RemoveOtherPlayerControlledCompanyWorker(PhotonPlayer target, SharedWorker worker)
        {
            this.photonView.RPC(
                "RemoveControlledCompanyWorkerRPC", target, worker.ID, PhotonNetwork.player.ID);
        }

        public void HireOtherPlayerWorker(PhotonPlayer otherPlayer, SharedWorker worker)
        {
            RemoveOtherPlayerControlledCompanyWorker(otherPlayer, worker);
            LocalWorker hiredWorker = new LocalWorker(worker);
            hiredWorker.Salary = hiredWorker.HireSalary;
            ControlledCompany.Balance -= hiredWorker.Salary;
            ControlledCompany.AddWorker(hiredWorker);
            Stats.OtherPlayersWorkersHired++;
        }

        public static int GenerateID()
        {
            if (m_ID == int.MaxValue)
            {
                throw new InvalidOperationException("Reached max ID value. Cannot generate ID");
            }

            return m_ID++;
        }

        public override void OnDisconnectedFromPhoton()
        {
            base.OnDisconnectedFromPhoton();

            FinishSimulationRPC(PhotonNetwork.player.ID, (int)SimulationFinishReason.Disconnected);
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);

            //Only one player is left in room (local player) so he wins the game
            if (1 == PhotonNetwork.room.PlayerCount)
            {
                FinishSimulationRPC(PhotonNetwork.player.ID, (int)SimulationFinishReason.OnePlayerInRoom);
            }
            else
            {
                OtherPlayersWorkers.Remove(otherPlayer);
            }
        }
    }
}
