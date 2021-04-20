using ITCompanySimulation.Core;
using ITCompanySimulation.Developing;
using ITCompanySimulation.UI;
using ITCompanySimulation.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
    private ResourceHolder ResourceHolderComponent;

    /*Public consts fields*/

    /*Public fields*/

    public event SharedProjectAction ProjectRemoved;
    public event SharedProjectAction ProjectAdded;
    public event UnityAction DataReceived;

    public List<SharedProject> Projects { get; private set; } = new List<SharedProject>();
    public bool IsDataReceived { get; private set; }

    /*Private methods*/

    private void OnGameTimeDayChanged()
    {
        if (true == PhotonNetwork.isMasterClient && Projects.Count < MaxProjectsOnMarket)
        {
            int randomNumber = UnityEngine.Random.Range(1, 101);

            if (randomNumber <= PROJECT_ADD_PROBABILITY_DAILY)
            {
                SharedProject newProject = GenerateSingleProject();
                this.photonView.RPC("AddProjectRPC", PhotonTargets.All, newProject);
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
        int projectNameIndex = UnityEngine.Random.Range(0, ProjectData.Names.Count);
        string projectName =
            ProjectData.Names[projectNameIndex];

        newProject = new SharedProject(projectName);
        newProject.UsedTechnologies = GenerateProjectTechnologies();
        newProject.CompletionTime = UnityEngine.Random.Range(PROJECT_COMPLETION_TIME_MIN, PROJECT_COMPLETION_TIME_MAX);
        newProject.CompletionBonus = CalculateProjectCompleteBonus(newProject);
        newProject.ID = SimulationManager.GenerateID();
        newProject.NameIndex = projectNameIndex;
        //Every project name is associated with one icon
        newProject.IconIndex = projectNameIndex;
        newProject.Icon = ResourceHolderComponent.ProjectsIcons[projectNameIndex];

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

        while (MaxProjectsOnMarket > Projects.Count)
        {
            SharedProject newProject = GenerateSingleProject();
            this.photonView.RPC("AddProjectRPC", PhotonTargets.All, newProject);
        }

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

    [PunRPC]
    private void AddProjectRPC(SharedProject projectToAdd)
    {
        Projects.Add(projectToAdd);
        ProjectAdded?.Invoke(projectToAdd);

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
    private void RemoveProjectInternalRPC(int projectToRemoveID)
    {
        SharedProject removedProject = null;

        for (int i = 0; i < Projects.Count; i++)
        {
            if (projectToRemoveID == Projects[i].ID)
            {
                removedProject = Projects[i];
                Projects.RemoveAt(i);
                break;
            }
        }

        ProjectRemoved?.Invoke(removedProject);
    }

    [PunRPC]
    private void SetMaxProjectsOnMarketRPC(int numberOfProjects)
    {
        MaxProjectsOnMarket = numberOfProjects;
    }

    /// <summary>
    /// Truncates projects list so its length matches provided parameter
    /// Does nothing if provided parameter is equal or greater than number of projects
    /// </summary>
    [PunRPC]
    private void TruncateProjectsRPC(int count)
    {
        if (count < this.Projects.Count)
        {
            while (this.Projects.Count != count)
            {
                this.Projects.RemoveAt(this.Projects.Count - 1);
            }
        }
    }

    private void Start()
    {
        ResourceHolderComponent = GetComponent<ResourceHolder>();

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

    public void RemoveProject(SharedProject projectToRemove)
    {
        this.photonView.RPC("RemoveProjectInternalRPC", PhotonTargets.All, projectToRemove.ID);
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
            MaxProjectsOnMarket = CalculateMaxProjectsOnMarket();
            this.photonView.RPC("SetMaxProjectsOnMarketRPC", PhotonTargets.Others, MaxProjectsOnMarket);
            this.photonView.RPC("TruncateProjectsRPC", PhotonTargets.Others, MaxProjectsOnMarket);
            GenerateAndSendProjects();
        }
    }
}
