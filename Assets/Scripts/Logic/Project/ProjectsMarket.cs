using ITCompanySimulation.Core;
using ITCompanySimulation.Multiplayer;
using ITCompanySimulation.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

namespace ITCompanySimulation.Project
{
    /// <summary>
    /// This class handles represents projects market. Each of players can take project
    /// from market. Each of project can only be progressed by one player.
    /// Projects market is shared across all players
    /// </summary>
    public class ProjectsMarket : Photon.PunBehaviour, IDataReceiver
    {
        /*Private consts fields*/

        private readonly int NUMBER_OF_PROJECT_TECHNOLOGIES =
            Enum.GetValues(typeof(ProjectTechnology)).Length;
        /// <summary>
        /// How many projects should be added to market
        /// per one player in game
        /// </summary>
        private const int PROJECTS_ON_MARKET_PER_PLAYER = 8;
        /// <summary>
        /// How many abilites can one project have at a time
        /// </summary>
        private const int MAX_NUMBER_OF_PROJECT_ABILITIES = 1;
        /// <summary>
        /// How much money will be added to bonus for completing
        /// project per one technology in project
        /// </summary>
        private const int PROJECT_BONUS_PER_TECHNOLOGY = 25000;
        /// <summary>
        /// Base amount of money for completing project
        /// </summary>
        private const int PROJECT_BONUS_BASE = 80000;
        /// <summary>
        /// Probability in % of adding new project each day
        /// (only when number of projects did not reach maximum
        /// number of projects) - MaxProjectsOnMarket
        /// </summary>
        private const int PROJECT_ADD_PROBABILITY_DAILY = 10;
        /// <summary>
        /// Min and max values of days in which project should be completed
        /// </summary>
        private const int PROJECT_COMPLETION_TIME_MIN = 40;
        private const int PROJECT_COMPLETION_TIME_MAX = 160;

        /*Private fields*/

        /// <summary>
        /// How many projects can be on market at one time
        /// </summary>
        private int MaxProjectsOnMarket;
        private GameTime GameTimeComponent;
        /// <summary>
        /// How many projects should be generated in market
        /// when simulation is run in offline mode
        /// </summary>
        [Range(1.0f, 1000.0f)]
        [SerializeField]
        private int NumberOfProjectsGeneratedInOfflineMode;
        private SimulationManager SimulationManagerComponent;
        private ApplicationManager ApplicationManagerComponent;

        /*Public consts fields*/

        /*Public fields*/

        public event SharedProjectAction ProjectRemoved;
        public event SharedProjectAction ProjectAdded;
        public event UnityAction DataReceived;

        /// <summary>
        /// Map containing all projects in market with project id as key and project as value.
        /// </summary>
        public Dictionary<int, SharedProject> Projects { get; private set; } = new Dictionary<int, SharedProject>();
        public bool IsDataReceived { get; private set; }
        public ProjectGenerationData GenerationData;

        /*Private methods*/

        private void OnGameTimeDayChanged()
        {
            if (true == PhotonNetwork.isMasterClient && Projects.Count < MaxProjectsOnMarket)
            {
                int randomNumber = UnityEngine.Random.Range(1, 101);

                if (randomNumber <= PROJECT_ADD_PROBABILITY_DAILY)
                {
                    SharedProject newProject = GenerateSingleProject();
                    this.photonView.RPC("OnProjectAddedRPC", PhotonTargets.All, newProject);
                }
            }
        }

        private int CalculateMaxProjectsOnMarket()
        {
            int maxProjectsOnMarket = PhotonNetwork.offlineMode ?
                (NumberOfProjectsGeneratedInOfflineMode) : (PhotonNetwork.room.PlayerCount * PROJECTS_ON_MARKET_PER_PLAYER);

            return maxProjectsOnMarket;
        }

        private SharedProject GenerateSingleProject()
        {
            SharedProject newProject;
            int projectNameIndex = UnityEngine.Random.Range(0, GenerationData.Names.Length);
            string projectName = GenerationData.Names[projectNameIndex];

            newProject = new SharedProject(projectName);
            newProject.UsedTechnologies = GenerateProjectTechnologies();
            newProject.CompletionTime = UnityEngine.Random.Range(PROJECT_COMPLETION_TIME_MIN, PROJECT_COMPLETION_TIME_MAX);
            newProject.CompletionBonus = CalculateProjectCompleteBonus(newProject);
            newProject.ID = SimulationManager.GenerateID();
            newProject.NameIndex = projectNameIndex;
            //Every project name is associated with one icon
            newProject.IconIndex = projectNameIndex;
            newProject.Icon = GenerationData.Icons[projectNameIndex];

            return newProject;
        }

        private int CalculateProjectCompleteBonus(SharedProject newProject)
        {
            int projectCompleteBonus = PROJECT_BONUS_BASE;

            foreach (ProjectTechnology techonology in newProject.UsedTechnologies)
            {
                projectCompleteBonus += PROJECT_BONUS_PER_TECHNOLOGY;
            }

            //Used to calculate completion bonus taking project's completion time
            //into consideration
            float completionTimeMultiplier = 3f / Utils.MapRange(newProject.CompletionTime,
                                                                 PROJECT_COMPLETION_TIME_MIN,
                                                                 PROJECT_COMPLETION_TIME_MAX,
                                                                 1f,
                                                                 3f);
            projectCompleteBonus = (int)(completionTimeMultiplier * projectCompleteBonus);

            return projectCompleteBonus;
        }

        /// <summary>
        /// Generates projects on market and sends it to other clients
        /// </summary>
        private void GenerateAndSendProjects()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            string className = this.GetType().Name;
            string debugInfo = string.Format("[{0}] generating {1} projects...",
                className, MaxProjectsOnMarket - Projects.Count);
            Debug.Log(debugInfo);
#endif
            SharedProject[] generatedProjects = new SharedProject[MaxProjectsOnMarket];

            for (int i = 0; i < MaxProjectsOnMarket; i++)
            {
                generatedProjects[i] = GenerateSingleProject();
            }

            this.photonView.RPC("OnProjectsGeneratedRPC", PhotonTargets.All, generatedProjects);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            debugInfo = string.Format("[{0}] generated {1} projects",
               className, Projects.Count);
            Debug.Log(debugInfo);
#endif
        }

        private List<ProjectTechnology> GenerateProjectTechnologies()
        {
            List<ProjectTechnology> projectTechnologies = new List<ProjectTechnology>();
            int numberOfTechnologies = UnityEngine.Random.Range(1, MAX_NUMBER_OF_PROJECT_ABILITIES);

            for (int i = 0; i < numberOfTechnologies; i++)
            {
                ProjectTechnology projectAbility =
                    (ProjectTechnology)UnityEngine.Random.Range(0, NUMBER_OF_PROJECT_TECHNOLOGIES);

                if (false == projectTechnologies.Contains(projectAbility))
                {
                    projectTechnologies.Add(projectAbility);
                }
            }

            return projectTechnologies;
        }

        /// <summary>
        /// Called when master client has generated projects in market at the
        /// beggining of simulation.
        /// </summary>
        [PunRPC]
        private void OnProjectsGeneratedRPC(SharedProject[] generatedProjects)
        {
            foreach (SharedProject proj in generatedProjects)
            {
                Projects.Add(proj.ID, proj);
                ProjectAdded?.Invoke(proj);
            }

            if (false == PhotonNetwork.isMasterClient && false == IsDataReceived)
            {
                if (this.Projects.Count == MaxProjectsOnMarket)
                {
                    IsDataReceived = true;
                    DataReceived?.Invoke();
                }
            }
        }

        [PunRPC]
        private void OnProjectAddedRPC(SharedProject projectToAdd)
        {
            Projects.Add(projectToAdd.ID, projectToAdd);
            ProjectAdded?.Invoke(projectToAdd);
        }

        /// <summary>
        /// Called by client that requested project from market.
        /// </summary>
        /// <param name="requestedProjectID">ID of requested project</param>
        /// <param name="photonPlayerID">Id of photon player that sends request</param>
        [PunRPC]
        private void OnProjectRequestRPC(int requestedProjectID, int photonPlayerID)
        {
            //Concept of requesting project is introduced so master client can decide
            //which client should receive project in case two RPCs from different clients
            //arrive in short time

            PhotonPlayer targetPlayer = Utils.PhotonPlayerFromID(photonPlayerID);

            //Check if project wasn't removed before by another request
            if (true == Projects.ContainsKey(requestedProjectID))
            {
                SharedProject requestedProject = Projects[requestedProjectID];
                this.photonView.RPC("OnProjectRequestCfmRPC", targetPlayer, requestedProjectID);
                this.photonView.RPC("OnProjectRemovedRPC", PhotonTargets.All, requestedProjectID);
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            else
            {
                Debug.LogWarningFormat("[{0}] Project (ID {1}) requested from Player: {2} (ID {3}) but project" +
                                        "does not exists in market anymore",
                                        this.GetType().Name,
                                        requestedProjectID,
                                        targetPlayer.NickName,
                                        targetPlayer.ID);
            }
#endif
        }

        /// <summary>
        /// Called when project requested through OnProjectRequestRPC is given to this client.
        /// </summary>
        /// <param name="requestedProjectID">ID of given project</param>
        [PunRPC]
        private void OnProjectRequestCfmRPC(int requestedProjectID)
        {
            SharedProject requestedProject = Projects[requestedProjectID];
            LocalProject requestedLocalProject = new LocalProject(requestedProject);
            SimulationManagerComponent.ControlledCompany.AddProject(requestedLocalProject);
        }

        [PunRPC]
        private void OnProjectRemovedRPC(int projectToRemoveID)
        {
            SharedProject removedProject = Projects[projectToRemoveID];
            Projects.Remove(removedProject.ID);
            ProjectRemoved?.Invoke(removedProject);
        }

        [PunRPC]
        private void SetMaxProjectsOnMarketRPC(int numberOfProjects)
        {
            MaxProjectsOnMarket = numberOfProjects;
        }

        /// <summary>
        /// Removes all projects from Projects dictionary.
        /// </summary>
        [PunRPC]
        private void ClearProjectsRPC()
        {
            SharedProject[] removedProjects = Projects.Values.ToArray();
            Projects.Clear();

            foreach (SharedProject project in removedProjects)
            {
                ProjectRemoved?.Invoke(project);
            }
        }

        private void Start()
        {
            SimulationManagerComponent = GetComponent<SimulationManager>();
            ApplicationManagerComponent =
                GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ApplicationManager>();

            //Master client will generate all the workers on market
            //then send it to other clients
            if (true == PhotonNetwork.isMasterClient)
            {
                GameTimeComponent = GetComponent<GameTime>();
                GameTimeComponent.DayChanged += OnGameTimeDayChanged;
                MaxProjectsOnMarket = CalculateMaxProjectsOnMarket();
                this.photonView.RPC("SetMaxProjectsOnMarketRPC", PhotonTargets.Others, MaxProjectsOnMarket);
                GenerateAndSendProjects();
            }
        }

        /*Public methods*/

        /// <summary>
        /// Sends request to master client to remove project from market
        /// and give it to player that requests it. This client will receive
        /// RPC call when reuqest is confirmed (might not be confirmed due to
        /// other client reuest being honored).
        /// </summary>
        public void RequestProject(SharedProject requestedProject)
        {
            this.photonView.RPC("OnProjectRequestRPC",
                                PhotonNetwork.masterClient,
                                requestedProject.ID,
                                PhotonNetwork.player.ID);
        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            base.OnPhotonPlayerDisconnected(otherPlayer);

            MaxProjectsOnMarket = CalculateMaxProjectsOnMarket();
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            base.OnMasterClientSwitched(newMasterClient);

            if (true == PhotonNetwork.isMasterClient && 0 != PhotonNetwork.otherPlayers.Length)
            {
                int maxProjects = CalculateMaxProjectsOnMarket();
                this.photonView.RPC("SetMaxProjectsOnMarketRPC", PhotonTargets.All, MaxProjectsOnMarket);
                GameTimeComponent = GetComponent<GameTime>();
                GameTimeComponent.DayChanged += OnGameTimeDayChanged;

                if (false == ApplicationManagerComponent.IsSessionActive)
                {
                    this.photonView.RPC("ClearProjectsRPC", PhotonTargets.All);
                    GenerateAndSendProjects();
                }
            }
        }
    }
}