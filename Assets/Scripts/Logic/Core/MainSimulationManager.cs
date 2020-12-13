using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using ITCompanySimulation.Character;
using ITCompanySimulation.UI;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// This is core class for all aspects of gameplay that will
    /// happen during running simulation (like ending game if gameplay
    /// target is reached)
    /// </summary>
    public class MainSimulationManager : Photon.PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private GameTime GameTimeComponent;
        [SerializeField]
        private InfoWindow InfoWindowComponent;
        /// <summary>
        /// Sets the time scale that simulation should be run with. 1.0 is default scale
        /// </summary>
        [SerializeField]
        [Range(0.1f, 10.0f)]
        private float SimulationTimeScale;
        private MainGameManager GameManagerComponent;

        /*Public consts fields*/

        /*Public fields*/

        public PlayerCompany ControlledCompany { get; private set; }
        public SimulationEventNotificator NotificatorComponent { get; private set; }
        /// <summary>
        /// Invoked when any of player has reached the target balance
        /// and won game
        /// </summary>
        public event Action<int, SimulationFinishReason> SimulationFinished;
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
        /// <summary>
        /// This will map ID of photon player to list of player that his company has.
        /// </summary>
        public Dictionary<PhotonPlayer, List<SharedWorker>> OtherPlayersWorkers { get; private set; } = new Dictionary<PhotonPlayer, List<SharedWorker>>();
        /// <summary>
        /// True when any of player has won the game
        /// </summary>
        public bool IsSimulationFinished { get; private set; }
        /// <summary>
        /// ID of PhotonPlayer object of winner player valid only
        /// when IsSimulationFinished is true
        /// </summary>
        public PhotonPlayer WinnerPlayer { get; private set; }
        public SimulationFinishReason FinishReason { get; private set; }
        public bool IsSimulationActive { get; private set; }

        /*Private methods*/

        private void Update()
        {
            Time.timeScale = SimulationTimeScale;
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
            ControlledCompany.Balance = SimulationSettings.InitialBalance;
        }

        private void OnControlledCompanyBalanceChanged(int newBalance)
        {
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
            SimulationTimeScale = 1f;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogFormat("[{0}] Session started. Starting simulation with time scale {1}",
                this.GetType().Name, SimulationTimeScale);
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
        }

        private void OnControlledCompanyWorkerRemoved(SharedWorker removedWorker)
        {
            this.photonView.RPC(
                "OnControlledCompanyWorkerRemovedRPC", PhotonTargets.Others, removedWorker.ID, PhotonNetwork.player.ID);
            removedWorker.SalaryChanged -= OnCompanyWorkerSalaryChanged;
            removedWorker.AbilityUpdated -= OnCompanyWorkerAbilityUpdated;
            removedWorker.ExpierienceTimeChanged -= OnCompanyWorkerExpierienceTimeChanged;
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
            if (false == IsSimulationFinished)
            {
                //Stop time so events in game are no longer updated
                SimulationTimeScale = 0f;
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
                InfoWindowComponent.ShowOk(finishSimulationInfoMsg, GameManagerComponent.FinishSession);
            }
        }

        /*Public methods*/

        /// <summary>
        /// Removes worker with given ID from company of target PhotonPlayer
        /// </summary>
        public void RemoveOtherPlayerControlledCompanyWorker(PhotonPlayer target, int workerID)
        {
            this.photonView.RPC(
                "RemoveControlledCompanyWorkerRPC", target, workerID, PhotonNetwork.player.ID);
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

        public void Awake()
        {
            GameManagerComponent = GameObject.FindGameObjectWithTag("GameManager").GetComponent<MainGameManager>();
            //TODO: Add info window when waiting for session start
            GameManagerComponent.SessionStarted += OnGameManagerComponentSessionStarted;
            GameTimeComponent = GetComponent<GameTime>();
            NotificatorComponent = new SimulationEventNotificator(GameTimeComponent);
            IsSimulationActive = true;

            InitPlayersWorkers();
            //TEST
            CreateCompany();
        }
    }
}
