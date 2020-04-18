using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProjectsCompanyProjects : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private Scrum SelectedProjectScrum;
    /// <summary>
    /// List of workers buttons with each button mapped to its worker
    /// </summary>
    private Dictionary<Button, Worker> ButtonWorkerMap = new Dictionary<Button, Worker>();
    private Worker SelectedAvailableWorker;
    private Worker SelectedAssignedWorker;
    private IButtonSelector AvailableWorkersButtonSelector = new ButtonSelector();
    private IButtonSelector AssignedWorkersButtonSelector = new ButtonSelector();
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// List of all projects in company will be placed here
    /// </summary>
    [SerializeField]
    private Dropdown ProjectsListDropdown;
    [SerializeField]
    private InputField ProjectInfoDaysSinceStartText;
    [SerializeField]
    private InputField ProjectInfoUsedTechnologies;
    [SerializeField]
    private ControlListView AvailableWorkersControlList;
    [SerializeField]
    private ControlListView AssignedWorkersControlList;
    [SerializeField]
    private Button AssignWorkerButton;
    [SerializeField]
    private Button UnassignWorkerButton;
    [SerializeField]
    private Button StartProjectButton;
    [SerializeField]
    private Button StopProjectButton;
    [SerializeField]
    private Button ListViewButtonPrefab;
    [SerializeField]
    private Slider ProjectProgressBar;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    /// <summary>
    /// Creates button that will be added to list view
    /// with workers
    /// </summary>
    private Button CreateWorkerButton(Worker workerData)
    {
        Button createdButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);

        Text buttonTextComponent = createdButton.GetComponentInChildren<Text>();
        buttonTextComponent.text = string.Format("{0} {1}", workerData.Name, workerData.Surename);

        return createdButton;
    }

    private void AddAvailableWorkersListViewButtons()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            //Add only workers that dont have assigned any project
            //Available worker is considered worker without project assigned
            if (null == companyWorker.AssignedProject)
            {
                AddWorkerListViewButton(companyWorker);
            }
        }
    }

    private void RemoveWorkerListViewButton(Worker companyWorker)
    {
        KeyValuePair<Button, Worker> buttonWorkerPair =
            ButtonWorkerMap.First(x => x.Value == companyWorker);
        ButtonWorkerMap.Remove(buttonWorkerPair.Key);
        RemoveWorkerListViewButton(buttonWorkerPair.Key, companyWorker);
    }

    private void RemoveWorkerListViewButton(Button workerButton, Worker companyWorker)
    {
        if (null == companyWorker.AssignedProject)
        {
            AvailableWorkersControlList.RemoveControl(workerButton.gameObject);
            AvailableWorkersButtonSelector.RemoveButton(workerButton);
        }
        else
        {
            AssignedWorkersControlList.RemoveControl(workerButton.gameObject);
            AssignedWorkersButtonSelector.RemoveButton(workerButton);
        }
    }

    /// <summary>
    /// Add worker's button to available workers list view. It will be always added
    /// to available workers list view since worker that was just added to company
    /// will not have any project assigned
    /// </summary>
    private void AddWorkerListViewButton(Worker companyWorker)
    {
        Button createdButton = CreateWorkerButton(companyWorker);

        AvailableWorkersControlList.AddControl(createdButton.gameObject);
        ButtonWorkerMap.Add(createdButton, companyWorker);
        AvailableWorkersButtonSelector.AddButton(createdButton);
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        //Worker that just has been added to company doesn't
        //have assigned project so its available worker
        AddWorkerListViewButton(addedWorker);
    }

    private void AddAssignedWorkersListViewButtons()
    {
        //Clear controls in case there were some added
        //for other project
        AssignedWorkersControlList.RemoveAllControls();

        foreach (Worker projectWorker in SelectedProjectScrum.BindedProject.Workers)
        {
            Button createdButton = CreateWorkerButton(projectWorker);

            AssignedWorkersControlList.AddControl(createdButton.gameObject);
            ButtonWorkerMap.Add(createdButton, projectWorker);
        }
    }

    /// <summary>
    /// Adds all projects in company to dropdown control
    /// </summary>
    private void AddProjectsToDropdown()
    {
        if (SimulationManagerComponent.ControlledCompany.ScrumProcesses.Count > 0)
        {
            ProjectsListDropdown.options.Clear();

            foreach (Scrum scrumProcess in SimulationManagerComponent.ControlledCompany.ScrumProcesses)
            {
                Project companyProject = scrumProcess.BindedProject;
                AddSingleProjectToDropdown(companyProject);
            }

            SelectedProjectScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses[ProjectsListDropdown.value];
        }
    }

    private void AddSingleProjectToDropdown(Project proj)
    {
        string projectOptionText = GetProjectListDropdownOptionText(proj);
        Dropdown.OptionData projectOption = new Dropdown.OptionData(proj.Name);
        ProjectsListDropdown.options.Add(projectOption);
    }

    private string GetProjectListDropdownOptionText(Project proj)
    {
        string projectOptionText = string.Format("{0} {1}",
                                         proj.Name,
                                         proj.IsCompleted ? "(Completed)" : string.Empty);

        return projectOptionText;
    }

    private void OnAvailableWorkersListSelectedButtonChanged(Button clickedButton)
    {
        if (null != SelectedProjectScrum && null != clickedButton)
        {
            SelectedAvailableWorker = ButtonWorkerMap[clickedButton];
            AssignWorkerButton.interactable = true;
        }
        else
        {
            AssignWorkerButton.interactable = false;
        }
    }

    private void OnAssignedWorkersListSelectedButtonChanged(Button clickedButton)
    {
        if (null != SelectedProjectScrum && null != clickedButton)
        {
            SelectedAssignedWorker = ButtonWorkerMap[clickedButton];
            UnassignWorkerButton.interactable = true;
        }
        else
        {
            UnassignWorkerButton.interactable = false;
        }

    }

    private void InitializeProjectButtons()
    {
        StartProjectButton.interactable = (false == SelectedProjectScrum.BindedProject.Active)
            && (false == SelectedProjectScrum.BindedProject.IsCompleted);
        StopProjectButton.interactable = SelectedProjectScrum.BindedProject.Active;
    }

    private void SetProjectProgressBar()
    {
        OnSelectedProjectProgressChanged(SelectedProjectScrum.BindedProject);
        SelectedProjectScrum.BindedProject.ProgressUpdated += OnSelectedProjectProgressChanged;
    }

    private void SetProjectInfo()
    {
        OnSelectedProjectDaysSinceStartChanged(SelectedProjectScrum.BindedProject);
        SelectedProjectScrum.BindedProject.DaysSinceStartUpdated += OnSelectedProjectDaysSinceStartChanged;
        ProjectInfoUsedTechnologies.text = string.Empty;

        foreach (ProjectTechnology technology in SelectedProjectScrum.BindedProject.UsedTechnologies)
        {
            string technologyName = (EnumToString.ProjectTechnologiesStrings[technology]) + " ";
            ProjectInfoUsedTechnologies.text += technologyName;
        }
    }

    private void Initialize()
    {
        SimulationManagerComponent.ControlledCompany.ProjectAdded += AddSingleProjectToDropdown;
        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += RemoveWorkerListViewButton;
        AssignedWorkersButtonSelector.SelectedButtonChanged += OnAssignedWorkersListSelectedButtonChanged;
        AvailableWorkersButtonSelector.SelectedButtonChanged += OnAvailableWorkersListSelectedButtonChanged;

        AddAvailableWorkersListViewButtons();
        AddProjectsToDropdown();

        if (ProjectsListDropdown.options.Count > 0)
        {
            OnProjectsListDropdownValueChanged(ProjectsListDropdown.value);
        }
    }

    /// <summary>
    /// Moves workers list button between assigned and unassigned workers list
    /// </summary>
    private void MoveWorkersListButton(ControlListView listViewFrom, ControlListView listViewTo,
                            IButtonSelector buttonSelectorFrom, IButtonSelector buttonSelectorTo,
                            Button workersListButton)
    {
        listViewFrom.RemoveControl(workersListButton.gameObject, false);
        listViewTo.AddControl(workersListButton.gameObject);

        buttonSelectorFrom.RemoveButton(workersListButton);
        buttonSelectorTo.AddButton(workersListButton);
    }

    private void OnSelectedProjectProgressChanged(Project proj)
    {
        ProjectProgressBar.value = proj.Progress;
    }

    private void OnSelectedProjectDaysSinceStartChanged(Project proj)
    {
        ProjectInfoDaysSinceStartText.text = proj.DaysSinceStart.ToString();
    }

    private void OnSelectedProjectCompleted(Project proj)
    {
        StartProjectButton.interactable = false;
        StopProjectButton.interactable = false;

        foreach (Worker projectWorker in proj.Workers)
        {
            Button workerButton = ButtonWorkerMap.First(x => x.Value.ID == projectWorker.ID).Key;
            MoveWorkersListButton(AssignedWorkersControlList, AvailableWorkersControlList, AssignedWorkersButtonSelector,
                AvailableWorkersButtonSelector, workerButton);
        }

        ProjectsListDropdown.options[ProjectsListDropdown.value].text = GetProjectListDropdownOptionText(proj);
    }

    private void OnSelectedProjectWorkerRemoved(Worker companyWorker)
    {
        Button selectedWorkerListButton = AssignedWorkersButtonSelector.GetSelectedButton();

        //Check if worker left project because he left company
        if (null != companyWorker.WorkingCompany)
        {
            MoveWorkersListButton(AssignedWorkersControlList,
                                  AvailableWorkersControlList,
                                  AssignedWorkersButtonSelector,
                                  AvailableWorkersButtonSelector,
                                  selectedWorkerListButton);
        }
        else
        {
            RemoveWorkerListViewButton(companyWorker);
        }
    }

    private void OnSelectedProjectWorkerAdded(Worker companyWorker)
    {
        Button selectedWorkerListButton = AvailableWorkersButtonSelector.GetSelectedButton();
        MoveWorkersListButton(AvailableWorkersControlList,
                              AssignedWorkersControlList,
                              AvailableWorkersButtonSelector,
                              AssignedWorkersButtonSelector,
                              selectedWorkerListButton);
    }

    private void OnSelectedProjectStopped(Project proj)
    {
        StartProjectButton.interactable = true;
        StopProjectButton.interactable = false;
    }

    private void OnSelectedProjectStarted(Project proj)
    {
        StartProjectButton.interactable = false;
        StopProjectButton.interactable = true;
    }

    private void Start()
    {
        Initialize();
    }

    /*Public methods*/

    public void OnAssignWorkerButtonClick()
    {
        SelectedProjectScrum.BindedProject.AddWorker(SelectedAvailableWorker);
        SelectedAvailableWorker = null;
    }

    public void OnUnassignWorkerButtonClick()
    {
        SelectedProjectScrum.BindedProject.RemoveWorker(SelectedAssignedWorker);
        SelectedAssignedWorker = null;
    }

    public void OnProjectsListDropdownValueChanged(int index)
    {
        if (null != SelectedProjectScrum)
        {
            //Unsubscribe progress bar event from previously selected project
            SelectedProjectScrum.BindedProject.ProgressUpdated -= OnSelectedProjectProgressChanged;
            SelectedProjectScrum.BindedProject.DaysSinceStartUpdated -= OnSelectedProjectDaysSinceStartChanged;
            SelectedProjectScrum.BindedProject.Completed -= OnSelectedProjectCompleted;
            SelectedProjectScrum.BindedProject.WorkerAdded -= OnSelectedProjectWorkerAdded;
            SelectedProjectScrum.BindedProject.WorkerRemoved -= OnSelectedProjectWorkerRemoved;
            SelectedProjectScrum.BindedProject.Stopped -= OnSelectedProjectStopped;
            SelectedProjectScrum.BindedProject.Started -= OnSelectedProjectStarted;

        }

        SelectedProjectScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses[ProjectsListDropdown.value];

        SelectedProjectScrum.BindedProject.Completed += OnSelectedProjectCompleted;
        SelectedProjectScrum.BindedProject.WorkerAdded += OnSelectedProjectWorkerAdded;
        SelectedProjectScrum.BindedProject.WorkerRemoved += OnSelectedProjectWorkerRemoved;
        SelectedProjectScrum.BindedProject.Started += OnSelectedProjectStarted;
        SelectedProjectScrum.BindedProject.Stopped += OnSelectedProjectStopped;

        AddAssignedWorkersListViewButtons();
        InitializeProjectButtons();
        SetProjectProgressBar();
        SetProjectInfo();
    }

    public void StartSelectedProject()
    {
        SelectedProjectScrum.StartProject();
    }

    public void StopSelectedProject()
    {
        SelectedProjectScrum.StopProject();
    }
}
