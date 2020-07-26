using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UIProjectsCompanyProjects : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    private Scrum SelectedProjectScrum;
    /// <summary>
    /// List of workers buttons with each button mapped to its worker
    /// </summary>
    private Dictionary<Button, Worker> ButtonWorkerMap = new Dictionary<Button, Worker>();
    /// <summary>
    /// This will map ID of project to index in projects dropdown
    /// </summary>
    private Dictionary<int, int> ProjectIDDropdownIndexMap = new Dictionary<int, int>();
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
    private InputField InputFieldProjectInfoDaysSinceStartText;
    [SerializeField]
    private InputField InputFieldProjectInfoUsedTechnologies;
    [SerializeField]
    private InputField InputFieldProjectInfoEstimatedCompletionTime;
    [SerializeField]
    private InputField InputFieldScrumInfoSprintStage;
    [SerializeField]
    private InputField InputFieldScrumInfoSprintNumber;
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
    [SerializeField]
    private TextMeshProUGUI TooltipText;
    /// <summary>
    /// Used to map mouse pointer position to UI coordinates
    /// </summary>
    [SerializeField]
    RectTransform ParentRectTransform;

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

        //Set up tooltip component
        EventTrigger workerButtonEventTrigger = createdButton.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry newTrigger = new EventTrigger.Entry();
        newTrigger.eventID = EventTriggerType.PointerEnter;
        newTrigger.callback.AddListener((BaseEventData evtData) =>
        {
            PointerEventData ptrEvtData = (PointerEventData)evtData;
            Button buttonUnderPointer =
            ptrEvtData.pointerCurrentRaycast.gameObject.transform.parent.gameObject.GetComponent<Button>();
            SetWorkerButtonTooltipText(buttonUnderPointer);
            TooltipText.transform.parent.gameObject.SetActive(true);
        });
        workerButtonEventTrigger.triggers.Add(newTrigger);

        newTrigger = new EventTrigger.Entry();
        newTrigger.eventID = EventTriggerType.PointerExit;
        newTrigger.callback.AddListener((BaseEventData evtData) =>
        {
            TooltipText.transform.parent.gameObject.SetActive(false);
        });
        workerButtonEventTrigger.triggers.Add(newTrigger);

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
        //Check if button was not removed when player left company.
        //First event will be called when player leaves company and
        //then when he leaves company he leaves projects 1st so another
        //event will be fired
        KeyValuePair<Button, Worker> buttonWorkerPair =
            ButtonWorkerMap.FirstOrDefault(x => x.Value == companyWorker);

        if (false == buttonWorkerPair.Equals(default(KeyValuePair<Button, Worker>)))
        {
            ButtonWorkerMap.Remove(buttonWorkerPair.Key);
            RemoveWorkerListViewButton(buttonWorkerPair.Key);
        }
    }

    private void RemoveWorkerListViewButton(Button workerButton)
    {
        if (null != workerButton)
        {
            if (true == AvailableWorkersControlList.Controls.Contains(workerButton.gameObject))
            {
                AvailableWorkersButtonSelector.RemoveButton(workerButton);
                AvailableWorkersControlList.RemoveControl(workerButton.gameObject);
            }
            else if (true == AssignedWorkersControlList.Controls.Contains(workerButton.gameObject))
            {
                AssignedWorkersButtonSelector.RemoveButton(workerButton);
                AssignedWorkersControlList.RemoveControl(workerButton.gameObject);
            }
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
            AssignedWorkersButtonSelector.AddButton(createdButton);
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
        proj.Completed += OnProjectCompleted;
        Dropdown.OptionData projectOption = new Dropdown.OptionData(proj.Name);
        ProjectsListDropdown.options.Add(projectOption);
        ProjectIDDropdownIndexMap.Add(proj.ID, ProjectsListDropdown.options.Count - 1);
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

    private void SetProjectButtons()
    {
        StartProjectButton.interactable = false == SelectedProjectScrum.BindedProject.Active
            && false == SelectedProjectScrum.BindedProject.IsCompleted
            && 0 != SelectedProjectScrum.BindedProject.Workers.Count;
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
        InputFieldProjectInfoUsedTechnologies.text = string.Empty;

        foreach (ProjectTechnology technology in SelectedProjectScrum.BindedProject.UsedTechnologies)
        {
            string technologyName = (EnumToString.ProjectTechnologiesStrings[technology]) + " ";
            InputFieldProjectInfoUsedTechnologies.text += technologyName;
        }

        SetProjectEstimatedCompletionTime();
    }

    /// <summary>
    /// Set project info that has changed when workers in project
    /// are modified
    /// </summary>
    private void SetProjectInfoWorkersChanged()
    {
        SetProjectEstimatedCompletionTime();
    }

    private void SetProjectEstimatedCompletionTime()
    {
        int estimatedCompletionTime = SelectedProjectScrum.GetProjectEstimatedCompletionTime();
        InputFieldProjectInfoEstimatedCompletionTime.text =
            (-1 == estimatedCompletionTime) ? string.Empty : estimatedCompletionTime.ToString() + " days";
    }

    private void SetScrumInfo()
    {
        SelectedProjectScrum.SprintNumberChanged += OnSelectedProjectScrumSprintNumberChanged;
        SelectedProjectScrum.SprintStageChanged += OnSelectedProjectScrumSprintStageChanged;

        InputFieldScrumInfoSprintNumber.text = SelectedProjectScrum.SprintNumber.ToString();
        InputFieldScrumInfoSprintStage.text = SelectedProjectScrum.CurrentSprintStage.ToString();
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
        SetProjectInfoWorkersChanged();
    }

    private void OnSelectedProjectDaysSinceStartChanged(Project proj)
    {
        InputFieldProjectInfoDaysSinceStartText.text = proj.DaysSinceStart.ToString();
    }

    private void OnProjectCompleted(Project proj)
    {
        StartProjectButton.interactable = false;
        StopProjectButton.interactable = false;

        int projectDropdownIndex = ProjectIDDropdownIndexMap[proj.ID];
        ProjectsListDropdown.options[projectDropdownIndex].text = GetProjectListDropdownOptionText(proj);
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

        SetProjectInfoWorkersChanged();
        SetProjectButtons();
    }

    private void OnSelectedProjectWorkerAdded(Worker companyWorker)
    {
        Button selectedWorkerListButton = AvailableWorkersButtonSelector.GetSelectedButton();
        MoveWorkersListButton(AvailableWorkersControlList,
                              AssignedWorkersControlList,
                              AvailableWorkersButtonSelector,
                              AssignedWorkersButtonSelector,
                              selectedWorkerListButton);
        SetProjectInfoWorkersChanged();
        SetProjectButtons();
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

    private void OnSelectedProjectScrumSprintStageChanged(Scrum scrumObj)
    {
        InputFieldScrumInfoSprintStage.text = scrumObj.CurrentSprintStage.ToString();
    }

    private void OnSelectedProjectScrumSprintNumberChanged(Scrum scrumObj)
    {
        InputFieldScrumInfoSprintNumber.text = scrumObj.SprintNumber.ToString();
    }

    private void SetWorkerButtonTooltipText(Button buttonUnderPointer)
    {
        Worker buttonWorker = ButtonWorkerMap.First(x => x.Key.GetInstanceID() == buttonUnderPointer.GetInstanceID()).Value;

        string tooltipString = "Abilities\n";
        foreach (KeyValuePair<ProjectTechnology, float> workerAbility in buttonWorker.Abilites)
        {
            tooltipString += string.Format("{0} {1}\n",
                                         EnumToString.ProjectTechnologiesStrings[workerAbility.Key],
                                         workerAbility.Value.ToString("0.00"));
        }

        TooltipText.SetText(tooltipString);
    }

    private void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        if (null == SelectedProjectScrum && ProjectsListDropdown.options.Count > 0)
        {
            OnProjectsListDropdownValueChanged(ProjectsListDropdown.value);
            OnAvailableWorkersListSelectedButtonChanged(AvailableWorkersButtonSelector.GetSelectedButton());
            OnAssignedWorkersListSelectedButtonChanged(AssignedWorkersButtonSelector.GetSelectedButton());
        }
    }

    private void Update()
    {
        if (true == TooltipText.transform.parent.gameObject.GetActive())
        {
            RectTransform tooltipTransform = transform.parent.parent.gameObject.GetComponent<RectTransform>();
            Vector3 mousePos = Input.mousePosition;
            Vector2 tooltipPostion = new Vector2(mousePos.x + 20f, mousePos.y);
            Vector2 finalPostion;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(ParentRectTransform, tooltipPostion, null, out finalPostion);

            TooltipText.gameObject.transform.parent.transform.localPosition = finalPostion;
        }
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
            //Unsubscribe events from previously selected project
            SelectedProjectScrum.BindedProject.ProgressUpdated -= OnSelectedProjectProgressChanged;
            SelectedProjectScrum.BindedProject.DaysSinceStartUpdated -= OnSelectedProjectDaysSinceStartChanged;
            SelectedProjectScrum.BindedProject.WorkerAdded -= OnSelectedProjectWorkerAdded;
            SelectedProjectScrum.BindedProject.WorkerRemoved -= OnSelectedProjectWorkerRemoved;
            SelectedProjectScrum.BindedProject.Stopped -= OnSelectedProjectStopped;
            SelectedProjectScrum.BindedProject.Started -= OnSelectedProjectStarted;
            SelectedProjectScrum.SprintNumberChanged -= OnSelectedProjectScrumSprintNumberChanged;
            SelectedProjectScrum.SprintStageChanged -= OnSelectedProjectScrumSprintStageChanged;
        }

        SelectedProjectScrum = SimulationManagerComponent.ControlledCompany.ScrumProcesses[ProjectsListDropdown.value];

        SelectedProjectScrum.BindedProject.WorkerAdded += OnSelectedProjectWorkerAdded;
        SelectedProjectScrum.BindedProject.WorkerRemoved += OnSelectedProjectWorkerRemoved;
        SelectedProjectScrum.BindedProject.Started += OnSelectedProjectStarted;
        SelectedProjectScrum.BindedProject.Stopped += OnSelectedProjectStopped;

        AddAssignedWorkersListViewButtons();
        SetProjectButtons();
        SetProjectProgressBar();
        SetProjectInfo();
        SetScrumInfo();
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
