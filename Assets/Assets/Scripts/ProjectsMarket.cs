using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class handles represents projects market. Each of players can take project
/// from market. Each of project can only be progressed by one player.
/// Projects market is shared across all players
/// </summary>
public class ProjectsMarket : MonoBehaviour
{
    /*Private consts fields*/

    private readonly int NUMBER_OF_PROJECT_TECHNOLOGIES =
        Enum.GetValues(typeof(ProjectTechnology)).Length;
    /// <summary>
    /// How many projects should be added to market
    /// per one player in game
    /// </summary>
    private const int PROJECTS_ON_MARKET_PER_PLAYER = 2;
    /// <summary>
    /// How many abilites can one project have at a time
    /// </summary>
    private const int MAX_NUMBER_OF_PROJECT_ABILITIES = 3;
    /// <summary>
    /// How much money will be added to bonus for completing
    /// project per one technology in project
    /// </summary>
    private const int PROJECT_BONUS_PER_TECHNOLOGY = 15000;
    /// <summary>
    /// Base amount of money for completing project
    /// </summary>
    private const int PROJECT_BONUS_BASE = 20000;

    /*Private fields*/

    /// <summary>
    /// How many projects can be on market at one time
    /// </summary>
    private int MaxProjectsOnMarket;
    /// <summary>
    /// Used to assing unique ID for each project
    /// </summary>
    private int ProjectID;
    private PhotonView PhotonViewComponent;

    /*Public consts fields*/

    /*Public fields*/

    public event ProjectAction ProjectRemoved;
    public event ProjectAction ProjectAdded;
    public List<Project> Projects;

    /*Private methods*/

    private int CalculateMaxProjectsOnMarket()
    {
        int value = PhotonNetwork.room.PlayerCount * PROJECTS_ON_MARKET_PER_PLAYER;
        return value;
    }

    private Project GenerateSingleProject()
    {
        Project newProject;
        int projectNameIndex = UnityEngine.Random.Range(0, ProjectData.Names.Count);
        string projectName =
            ProjectData.Names[projectNameIndex];

        newProject = new Project(projectName);
        newProject.UsedTechnologies = GenerateProjectTechnologies();
        newProject.CompleteBonus = CalculateProjectCompleteBonus(newProject);
        newProject.ID = ProjectID++;
        newProject.ProjectNameIndex = projectNameIndex;

        if (ProjectID < newProject.ID)
        {
            throw new OverflowException("Maximum number of generated projects exceeded");
        }

        return newProject;
    }

    private int CalculateProjectCompleteBonus(Project newProject)
    {
        int projectCompleteBonus = PROJECT_BONUS_BASE;

        foreach (ProjectTechnology techonology in newProject.UsedTechnologies)
        {
            projectCompleteBonus += PROJECT_BONUS_PER_TECHNOLOGY;
        }

        return projectCompleteBonus;
    }

    private void GenerateProjects()
    {
        while (MaxProjectsOnMarket != Projects.Count)
        {
            Project newProject = GenerateSingleProject();
            Projects.Add(newProject);
        }
    }

    private List<ProjectTechnology> GenerateProjectTechnologies()
    {
        List<ProjectTechnology> projectTechnologies = new List<ProjectTechnology>();
        int numberOfTechnologies = UnityEngine.Random.Range(0, MAX_NUMBER_OF_PROJECT_ABILITIES);

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
    private void AddProject(Project projectToAdd)
    {
        Projects.Add(projectToAdd);
        ProjectAdded?.Invoke(projectToAdd);
    }

    [PunRPC]
    private void RemoveProjectInternal(int projectToRemoveID)
    {
        Project removedProject = null;

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

    private void Start()
    {
        PhotonViewComponent = GetComponent<PhotonView>();
        Projects = new List<Project>();
        //Register type for sending projects available on market to other players
        PhotonPeer.RegisterType(typeof(Project), NetworkingData.PROJECT_BYTE_CODE, Project.Serialize, Project.Deserialize);

        //Master client will generate all the workers on market
        //then send it to other clients
        if (true == PhotonNetwork.isMasterClient)
        {
            MaxProjectsOnMarket = CalculateMaxProjectsOnMarket();
            GenerateProjects();

            foreach (Project singleProject in Projects)
            {
                PhotonViewComponent.RPC("AddProject", PhotonTargets.Others, singleProject);
            }
        }
    }

    /*Public methods*/

    public void RemoveProject(Project projectToRemove)
    {
        PhotonViewComponent.RPC("RemoveProjectInternal", PhotonTargets.All, projectToRemove.ID);
    }
}
