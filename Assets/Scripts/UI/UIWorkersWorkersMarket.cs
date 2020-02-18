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
    private Button ListViewButtonPrefab;
    [SerializeField]
    private ControlListView ListViewMarketWorkers;
    [SerializeField]
    private ControlListView ListViewCompanyWorkers;
    [SerializeField]
    private ControlListView ListViewWorkerAbilities;
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private WorkersMarket WorkersMarketComponent;
    [SerializeField]
    private Button ButtonHireWorker;
    [SerializeField]
    private Button ButtonFireWorker;
    [SerializeField]
    private InputField InputFieldNameAndSurename;
    [SerializeField]
    private InputField InputFieldExpierienceTime;
    [SerializeField]
    private InputField InputFieldSalary;
    private Button SelectedWorkerButton;
    private IButtonSelector WorkersButtonSelector = new ButtonSelector();

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
            WorkersButtonSelector.AddButton(newListViewButton);
        }
    }

    private void UpdateWorkerAbilitiesListView(ControlListView listView, Worker workerToDisplay)
    {
        listView.RemoveAllControls();

        foreach (KeyValuePair<ProjectTechnology, float> ability in workerToDisplay.Abilites)
        {
            Button workerAbilityButton = CreateWorkerAbilityButton(ability);
            listView.AddControl(workerAbilityButton.gameObject);
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

    private Button CreateWorkerAbilityButton(KeyValuePair<ProjectTechnology, float> ability)
    {
        Button workerAbilityButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);
        Text buttonTextComponent = workerAbilityButton.gameObject.GetComponentInChildren<Text>(workerAbilityButton);
        string buttonText = string.Format("{0} {1}", ability.Key, ability.Value.ToString("0.00"));
        buttonTextComponent.text = buttonText;

        return workerAbilityButton;
    }

    private void UpdateWorkerInfo(Worker selectedWorker)
    {
        //Name and surename
        InputFieldNameAndSurename.text = string.Format("{0} {1}", selectedWorker.Name, selectedWorker.Surename);

        //Expierience time
        InputFieldExpierienceTime.text = string.Format("{0} days", selectedWorker.ExperienceTime);

        //Salary
        InputFieldSalary.text = string.Format("{0} $", selectedWorker.Salary);

        //Abilites
        UpdateWorkerAbilitiesListView(ListViewWorkerAbilities, selectedWorker);
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
            ButtonFireWorker.interactable = false;
            ButtonHireWorker.interactable = true;
        }
        else
        {
            ButtonFireWorker.interactable = true;
            ButtonHireWorker.interactable = false;
        }

        if (PlayerCompany.MAX_WORKERS_PER_COMPANY == SimulationManagerComponent.ControlledCompany.Workers.Count)
        {
            ButtonHireWorker.interactable = false;
        }
    }

    private void OnMarketWorkerAdded(Worker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        ListViewMarketWorkers.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.AddButton(newWorkerButton);
    }

    private void OnMarketWorkerRemoved(Worker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        ListViewMarketWorkers.RemoveControl(workerButton.gameObject);
        WorkersButtonSelector.RemoveButton(workerButton);
        ButtonWorkerDictionary.Remove(workerButton);
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        ListViewCompanyWorkers.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.AddButton(newWorkerButton);
    }

    private void OnCompanyWorkerRemoved(Worker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        ListViewCompanyWorkers.RemoveControl(workerButton.gameObject);
        WorkersButtonSelector.RemoveButton(workerButton);
        ButtonWorkerDictionary.Remove(workerButton);
    }

    private void Start()
    {
        InitializeWorkersListView(ListViewMarketWorkers, WorkersMarketComponent.Workers);
        InitializeWorkersListView(ListViewCompanyWorkers, SimulationManagerComponent.ControlledCompany.Workers);
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