using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIWorkersWorkersMarket : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Used to map button in workers list to appriopriate worker
    /// </summary>
    private Dictionary<Button, Worker> ButtonWorkerDictionary = new Dictionary<Button, Worker>();
    [SerializeField]
    public Button ListViewButtonPrefab;
    [SerializeField]
    public ControlListView MarketWorkersListView;
    [SerializeField]
    public ControlListView CompanyWorkersListView;
    [SerializeField]
    public MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    public WorkersMarket WorkersMarketComponent;
    [SerializeField]
    public Button HireWorkerButton;
    [SerializeField]
    public Button FireWorkerButton;
    [SerializeField]
    private InputField InputFieldNameAndSurename;
    [SerializeField]
    private Dropdown DropdownAbilities;
    [SerializeField]
    private InputField InputFieldExpierienceTime;
    [SerializeField]
    private InputField InputFieldSalary;
    private Button SelectedWorkerButton;
    private ButtonSelector WorkersButtonSelector = new ButtonSelector();

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void InitializeWorkersListView(ControlListView listView, List<Worker> workers)
    {
        foreach (Worker singleWorker in workers)
        {
            Button newListViewButton = CreateWorkerButton(singleWorker);

            listView.AddControl(newListViewButton.gameObject);
            ButtonWorkerDictionary.Add(newListViewButton, singleWorker);
            WorkersButtonSelector.Buttons.Add(newListViewButton);
        }
    }

    private Button CreateWorkerButton(Worker workerObject)
    {
        Button newWorkerButton = GameObject.Instantiate(ListViewButtonPrefab);
        newWorkerButton.onClick.AddListener(OnWorkerButtonClicked);

        Text buttonTextComponent = newWorkerButton.GetComponentInChildren<Text>();
        string buttonText = string.Format("{0} {1} / {2} days / {3} $",
            workerObject.Name, workerObject.Surename, workerObject.ExperienceTime, workerObject.Salary);
        buttonTextComponent.text = buttonText;

        return newWorkerButton;
    }

    private void OnWorkerButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        Button newSelectedButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();

        if (newSelectedButton != SelectedWorkerButton)
        {
            SelectedWorkerButton = newSelectedButton;
            Worker selectedWorker = ButtonWorkerDictionary[SelectedWorkerButton];
            UpdateActionButtonsState(selectedWorker);
            UpdateWorkerInfo(selectedWorker);
        }
    }

    private void UpdateWorkerInfo(Worker selectedWorker)
    {
        //Name and surename
        InputFieldNameAndSurename.text = string.Format("{0} {1}", selectedWorker.Name, selectedWorker.Surename);

        //Abitilies
        DropdownAbilities.ClearOptions();
        List<string> dropdownOptions = new List<string>();

        foreach (KeyValuePair<ProjectTechnology, float> technology in selectedWorker.Abilites)
        {
            string abilityName = EnumToString.ProjectTechnologiesStrings[technology.Key];
            string dropdownOption = string.Format("{0} {1}", abilityName, technology.Value.ToString("0.00"));
            dropdownOptions.Add(dropdownOption);
        }

        DropdownAbilities.AddOptions(dropdownOptions);

        //Expierience time
        InputFieldExpierienceTime.text = string.Format("{0} days", selectedWorker.ExperienceTime);

        //Salary
        InputFieldSalary.text = string.Format("{0} $", selectedWorker.Salary);
    }

    /// <summary>
    /// This method will enable or disable buttons used
    /// for hiring of firing workers based on which worker
    /// is selected (market worker or company worker)
    /// </summary>
    private void UpdateActionButtonsState(Worker selectedWorker)
    {
        //Does not have company assigned so its market worker
        if (null == selectedWorker.WorkingCompany)
        {
            FireWorkerButton.interactable = false;
            HireWorkerButton.interactable = true;
        }
        else
        {
            FireWorkerButton.interactable = true;
            HireWorkerButton.interactable = false;
        }

        if (PlayerCompany.MAX_WORKERS_PER_COMPANY == SimulationManagerComponent.ControlledCompany.Workers.Count)
        {
            HireWorkerButton.interactable = false;
        }
    }

    private void OnMarketWorkerAdded(Worker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        MarketWorkersListView.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.Buttons.Add(newWorkerButton);
    }

    private void OnMarketWorkerRemoved(Worker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        MarketWorkersListView.RemoveControl(workerButton.gameObject);
        WorkersButtonSelector.Buttons.Remove(workerButton);
        ButtonWorkerDictionary.Remove(workerButton);
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        CompanyWorkersListView.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.Buttons.Add(newWorkerButton);
    }

    private void OnCompanyWorkerRemoved(Worker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        CompanyWorkersListView.RemoveControl(workerButton.gameObject);
        WorkersButtonSelector.Buttons.Remove(workerButton);
        ButtonWorkerDictionary.Remove(workerButton);
    }

    private void Start()
    {
        InitializeWorkersListView(MarketWorkersListView, WorkersMarketComponent.Workers);
        InitializeWorkersListView(CompanyWorkersListView, SimulationManagerComponent.ControlledCompany.Workers);
        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnCompanyWorkerRemoved;
        WorkersMarketComponent.WorkerAdded += OnMarketWorkerAdded;
        WorkersMarketComponent.WorkerRemoved += OnMarketWorkerRemoved;
    }

    /*Public methods*/

    public void OnHireWorkerButtonClicked()
    {
        Worker selectedWorker = ButtonWorkerDictionary[SelectedWorkerButton];
        WorkersMarketComponent.RemoveWorker(selectedWorker);
        SimulationManagerComponent.ControlledCompany.AddWorker(selectedWorker);
    }

    public void OnFireWorkerButtonClicked()
    {
        Worker selectedWorker = ButtonWorkerDictionary[SelectedWorkerButton];
        SimulationManagerComponent.ControlledCompany.RemoveWorker(selectedWorker);
        WorkersMarketComponent.AddWorker(selectedWorker);
    }
}