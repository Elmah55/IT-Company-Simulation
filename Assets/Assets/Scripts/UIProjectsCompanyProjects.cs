using System.Collections;
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
    private Dictionary<GameObject, Worker> WorkersButtons;
    /// <summary>
    /// Keep track of which button is currently selected for both list
    /// views (available workers and assinged workers)
    /// </summary>
    private GameObject AssignedWorkerSelectedButton;
    private GameObject AvailableWorkerSelectedButton;
    /// <summary>
    /// Stored to restore button's colors to default value
    /// </summary>
    private ColorBlock AvailableWorkerSelectedButtonColors;
    private ColorBlock AssignedWorkerSelectedButtonColors;
    private Worker SelectedAvailableWorker;
    private Worker SelectedAssignedWorker;

    /*Public consts fields*/

    /*Public fields*/

    public MainSimulationManager SimulationManagerComponent;
    /// <summary>
    /// List of all projects in company will be placed here
    /// </summary>
    public Dropdown ProjectsListDropdown;
    public InputField ProjectInfoDaysSinceStartText;
    public InputField ProjectInfoUsedTechnologies;
    public UIControlListView AvailableWorkersControlList;
    public UIControlListView AssignedWorkersControlList;
    public Button AssignWorkerButton;
    public Button UnassignWorkerButton;
    public Button StartProjectButton;
    public Button StopProjectButton;
    public GameObject ListViewButtonPrefab;
    public Slider ProjectProgressBar;

    /*Private methods*/

    /// <summary>
    /// Creates button that will be added to list view
    /// with workers
    /// </summary>
    private GameObject CreateWorkerButton(Worker workerData, UnityAction listener)
    {
        GameObject createdButton = GameObject.Instantiate(ListViewButtonPrefab);
        Button buttonComponent = createdButton.GetComponent<Button>();

        Text buttonTextComponent = createdButton.GetComponentInChildren<Text>();
        buttonTextComponent.text = string.Format("{0} {1}", workerData.Name, workerData.Surename);

        buttonComponent.onClick.AddListener(OnAvailableWorkersListButtonClicked);

        return createdButton;
    }

    private void AddAvailableWorkersListViewButtons()
    {
        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            //Add only workers that dont have assigned any project
            if (null == companyWorker.AssignedProject)
            {
                GameObject createdButton = CreateWorkerButton(companyWorker, OnAvailableWorkersListButtonClicked);

                AvailableWorkersControlList.AddControl(createdButton);
                WorkersButtons.Add(createdButton, companyWorker);
            }
        }
    }

    private void AddAssignedWorkersListViewButtons()
    {
        //Clear controls in case there were some added
        //for other project
        AssignedWorkersControlList.RemoveAllControls();

        foreach (Worker projectWorker in SelectedProjectScrum.BindedProject.Workers)
        {
            GameObject createdButton = CreateWorkerButton(projectWorker, OnAssignedWorkersListButtonClicked);

            AssignedWorkersControlList.AddControl(createdButton);
            WorkersButtons.Add(createdButton, projectWorker);
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

    private void SelectWorkerListButton(ref ColorBlock savedColors, ref GameObject savedSelectedButton)
    {
        //We know its worker list button because its clicked event has been just called
        GameObject selectedButton = EventSystem.current.currentSelectedGameObject;

        if (savedSelectedButton != selectedButton)
        {
            if (null != savedSelectedButton)
            {
                Button previouslySelectedButtonComponent = savedSelectedButton.GetComponent<Button>();
                //Restore default values for button that was previously selected
                previouslySelectedButtonComponent.colors = savedColors;
            }

            Button buttonComponent = selectedButton.GetComponent<Button>();
            ColorBlock buttonColors = buttonComponent.colors;
            savedColors = buttonColors;
            //We remember button as selected long as any other worker button
            //won't be selected. That's why color will always stay even when
            //button is not reckognized as selected anymore by unity UI engine
            buttonColors.normalColor = SELECTED_WORKER_BUTTON_COLOR;
            buttonColors.selectedColor = SELECTED_WORKER_BUTTON_COLOR;
            buttonComponent.colors = buttonColors;

            savedSelectedButton = selectedButton;
        }
    }

    private void DeselectWorkerListButton(GameObject selectedButton, ref ColorBlock savedColors)
    {
        Button buttonComponent = selectedButton.GetComponent<Button>();
        buttonComponent.colors = savedColors;
    }

    private void OnAvailableWorkersListButtonClicked()
    {
        SelectWorkerListButton(
            ref AvailableWorkerSelectedButtonColors, ref AvailableWorkerSelectedButton);
        SelectedAvailableWorker = WorkersButtons[AvailableWorkerSelectedButton];

        if (null != SelectedProjectScrum)
        {
            AssignWorkerButton.interactable = true;
        }
    }

    private void OnAssignedWorkersListButtonClicked()
    {
        SelectWorkerListButton(
            ref AssignedWorkerSelectedButtonColors, ref AssignedWorkerSelectedButton);
        SelectedAssignedWorker = WorkersButtons[AssignedWorkerSelectedButton];

        if (null != SelectedProjectScrum)
        {
            UnassignWorkerButton.interactable = true;
        }
    }

    private void InitializeProjectButtons()
    {
        StartProjectButton.interactable = (false == SelectedProjectScrum.BindedProject.Active);
        StopProjectButton.interactable = SelectedProjectScrum.BindedProject.Active;
    }

    private void SetProjectProgressBar()
    {
        OnProjectProgressChanged(SelectedProjectScrum.BindedProject);
        SelectedProjectScrum.BindedProject.ProgressUpdated += OnProjectProgressChanged;
    }

    private void SetProjectInfo()
    {
        OnProjectDaysSinceStartChanged(SelectedProjectScrum.BindedProject);
        SelectedProjectScrum.BindedProject.DaysSinceStartUpdated += OnProjectDaysSinceStartChanged;
        ProjectInfoUsedTechnologies.text = string.Empty;

        foreach (ProjectTechnology technology in SelectedProjectScrum.BindedProject.UsedTechnologies)
        {
            string technologyName = (EnumToString.ProjectTechnologiesStrings[technology]) + " ";
            ProjectInfoUsedTechnologies.text += technologyName;
        }
    }

    private void Initialize()
    {
        WorkersButtons = new Dictionary<GameObject, Worker>();
        WorkersButtons = new Dictionary<GameObject, Worker>();
        SimulationManagerComponent.ControlledCompany.ProjectAdded += AddSingleProjectToDropdown;

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
    private void MoveWorker(UIControlListView listFrom, UIControlListView listTo,
        ref GameObject selectedButton, ref ColorBlock selectedButtonSavedColors,
        ref Worker selectedWorker, UnityAction newButtonListener, Button moveWorkerButton)
    {
        listFrom.RemoveControl(selectedButton, false);
        listTo.AddControl(selectedButton);

        DeselectWorkerListButton(selectedButton, ref selectedButtonSavedColors);
        Button buttonComponent = selectedButton.GetComponent<Button>();
        buttonComponent.onClick.RemoveAllListeners();
        buttonComponent.onClick.AddListener(newButtonListener);

        selectedButton = null;
        selectedWorker = null;
        moveWorkerButton.interactable = false;
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
        MoveWorker(AvailableWorkersControlList, AssignedWorkersControlList, ref AvailableWorkerSelectedButton,
            ref AvailableWorkerSelectedButtonColors, ref SelectedAvailableWorker, OnAssignedWorkersListButtonClicked, AssignWorkerButton);

    }

    public void UnassignWorker()
    {
        SelectedProjectScrum.BindedProject.Workers.Remove(SelectedAssignedWorker);
        SelectedAssignedWorker.AssignedProject = null;
        MoveWorker(AssignedWorkersControlList, AvailableWorkersControlList, ref AssignedWorkerSelectedButton,
            ref AssignedWorkerSelectedButtonColors, ref SelectedAssignedWorker, OnAvailableWorkersListButtonClicked, UnassignWorkerButton);
    }

    public void OnProjectsListDropdownValueChanged(int index)
    {
        if (null != SelectedProjectScrum)
        {
            //Unsubscribe progress bar event from previously selected project
            SelectedProjectScrum.BindedProject.ProgressUpdated -= OnProjectProgressChanged;
            SelectedProjectScrum.BindedProject.DaysSinceStartUpdated -= OnProjectDaysSinceStartChanged;
        }

        SelectedProjectScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses[ProjectsListDropdown.value];

        AddAssignedWorkersListViewButtons();
        InitializeProjectButtons();
        SetProjectProgressBar();
        SetProjectInfo();

        if (null != AvailableWorkerSelectedButton)
        {
            Button buttonComponent = AvailableWorkerSelectedButton.GetComponent<Button>();
            buttonComponent.interactable = true;
        }

        if (null != AssignedWorkerSelectedButton)
        {
            Button buttonComponent = AssignedWorkerSelectedButton.GetComponent<Button>();
            buttonComponent.interactable = true;
        }
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

    public void OnProjectProgressChanged(Project proj)
    {
        ProjectProgressBar.value = proj.Progress;
    }

    private void OnProjectDaysSinceStartChanged(Project proj)
    {
        ProjectInfoDaysSinceStartText.text = proj.DaysSinceStart.ToString();
    }
}
