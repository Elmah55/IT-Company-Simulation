using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIProjectsCompanyProjects : MonoBehaviour
{
    /*Private consts fields*/

    private readonly Color SELECTED_WORKER_BUTTON_COLOR = Color.gray;

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
    private Button CreateWorkerButton(Worker workerData, UnityAction listener)
    {
        Button createdButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);

        Text buttonTextComponent = createdButton.GetComponentInChildren<Text>();
        buttonTextComponent.text = string.Format("{0} {1}", workerData.Name, workerData.Surename);

        createdButton.onClick.AddListener(OnAvailableWorkersListButtonClicked);

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
                AddAvailableSingleWorkerListVewButton(companyWorker);
            }
        }
    }

    private void RemoveWorkerListViewButton(Worker companyWorker)
    {
        KeyValuePair<Button, Worker> buttonWorkerPair =
            ButtonWorkerMap.First(x => x.Value == companyWorker);
        ButtonWorkerMap.Remove(buttonWorkerPair.Key);

        if (null == buttonWorkerPair.Value.AssignedProject)
        {
            AvailableWorkersControlList.RemoveControl(buttonWorkerPair.Key.gameObject);
        }
        else
        {
            AssignedWorkersControlList.RemoveControl(buttonWorkerPair.Key.gameObject);
        }

    }

    private void AddAvailableSingleWorkerListVewButton(Worker companyWorker)
    {
        Button createdButton = CreateWorkerButton(companyWorker, OnAvailableWorkersListButtonClicked);

        AvailableWorkersControlList.AddControl(createdButton.gameObject);
        ButtonWorkerMap.Add(createdButton, companyWorker);
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        //Worker that just has been added to company doesn't
        //have assigned project so its available worker
        AddAvailableSingleWorkerListVewButton(addedWorker);
    }

    private void AddAssignedWorkersListViewButtons()
    {
        //Clear controls in case there were some added
        //for other project
        AssignedWorkersControlList.RemoveAllControls();

        foreach (Worker projectWorker in SelectedProjectScrum.BindedProject.Workers)
        {
            Button createdButton = CreateWorkerButton(projectWorker, OnAssignedWorkersListButtonClicked);

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

    private void AddSingleProjectToDropdown(Project projectInstance)
    {
        Dropdown.OptionData projectOption = new Dropdown.OptionData(projectInstance.Name);
        ProjectsListDropdown.options.Add(projectOption);
    }

    private void OnAvailableWorkersListButtonClicked()
    {
        if (null != SelectedProjectScrum)
        {
            AssignWorkerButton.interactable = true;
        }
    }

    private void OnAssignedWorkersListButtonClicked()
    {
        if (null != SelectedProjectScrum)
        {
            UnassignWorkerButton.interactable = true;
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

        AddAvailableWorkersListViewButtons();
        AddProjectsToDropdown();

        if (ProjectsListDropdown.options.Count > 0)
        {
            OnProjectsListDropdownValueChanged(ProjectsListDropdown.value);
        }
    }

    /// <summary>
    /// Moves worker between assigned and unassigned workers list
    /// </summary>
    private void MoveWorker(ControlListView listViewForm, ControlListView listViewTo,
                            IButtonSelector buttonSelectorFrom, IButtonSelector buttonSelectorTo,
                            ref Worker selectedWorker, UnityAction newButtonListener, Button moveWorkerButton)
    {
        Button selectedButton = buttonSelectorFrom.GetSelectedButton();
        listViewForm.RemoveControl(selectedButton.gameObject, false);
        listViewTo.AddControl(selectedButton.gameObject);

        selectedButton.onClick.RemoveAllListeners();
        selectedButton.onClick.AddListener(newButtonListener);

        buttonSelectorFrom.RemoveButton(selectedButton);
        buttonSelectorTo.AddButton(selectedButton);

        selectedWorker = null;
        moveWorkerButton.interactable = false;
    }

    private void OnSelectedProjectProgressChanged(Project proj)
    {
        ProjectProgressBar.value = proj.Progress;
    }

    private void OnSelectedProjectDaysSinceStartChanged(Project proj)
    {
        ProjectInfoDaysSinceStartText.text = proj.DaysSinceStart.ToString();
    }

    /*Public methods*/

    public void Start()
    {
        Initialize();
    }

    public void AssignWorker()
    {
        SelectedAvailableWorker.AssignedProject = SelectedProjectScrum.BindedProject;
        SelectedProjectScrum.BindedProject.Workers.Add(SelectedAvailableWorker);
        MoveWorker(AvailableWorkersControlList,
                   AssignedWorkersControlList,
                   AvailableWorkersButtonSelector,
                   AssignedWorkersButtonSelector,
                   ref SelectedAvailableWorker,
                   OnAssignedWorkersListButtonClicked,
                   AssignWorkerButton);

    }

    public void UnassignWorker()
    {
        SelectedProjectScrum.BindedProject.Workers.Remove(SelectedAssignedWorker);
        SelectedAssignedWorker.AssignedProject = null;
        MoveWorker(AssignedWorkersControlList,
                   AvailableWorkersControlList,
                   AssignedWorkersButtonSelector,
                   AvailableWorkersButtonSelector,
                   ref SelectedAssignedWorker,
                   OnAvailableWorkersListButtonClicked,
                   UnassignWorkerButton);
    }

    public void OnProjectsListDropdownValueChanged(int index)
    {
        if (null != SelectedProjectScrum)
        {
            //Unsubscribe progress bar event from previously selected project
            SelectedProjectScrum.BindedProject.ProgressUpdated -= OnSelectedProjectProgressChanged;
            SelectedProjectScrum.BindedProject.DaysSinceStartUpdated -= OnSelectedProjectDaysSinceStartChanged;
        }

        SelectedProjectScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses[ProjectsListDropdown.value];

        AddAssignedWorkersListViewButtons();
        InitializeProjectButtons();
        SetProjectProgressBar();
        SetProjectInfo();
    }

    public void StartSelectedProject()
    {
        SelectedProjectScrum.StartProject();
        StartProjectButton.interactable = false;
        StopProjectButton.interactable = true;
    }

    public void StopSelectedProject()
    {
        SelectedProjectScrum.StopProject();
        StopProjectButton.interactable = false;
        StartProjectButton.interactable = true;
    }
}
