using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIWorkers : MonoBehaviour
{
    /*Private consts fields*/

    private readonly Color SELECTED_WORKER_BUTTON_COLOR = Color.gray;

    /*Private fields*/

    /// <summary>
    /// Used to map button in workers list to appriopriate worker
    /// </summary>
    private Dictionary<GameObject, Worker> ButtonWorkerDictionary;
    private GameObject SelectedWorkerButton;
    /// <summary>
    /// Stored to restore button's colors to default value
    /// </summary>
    private ColorBlock WorkerSelectedButtonSavedColors;

    /*Public consts fields*/

    /*Public fields*/

    public GameObject ListViewButtonPrefab;
    public ControlListView MarketWorkersListView;
    public ControlListView CompanyWorkersListView;
    public MainSimulationManager SimulationManagerComponent;
    public WorkersMarket WorkersMarketComponent;
    public Button HireWorkerButton;
    public Button FireWorkerButton;

    /*Private methods*/

    private void InitializeWorkersListView(ControlListView listView, List<Worker> workers)
    {
        foreach (Worker singleWorker in workers)
        {
            GameObject newListViewButton = CreateWorkerButton(singleWorker);

            listView.AddControl(newListViewButton);
            ButtonWorkerDictionary.Add(newListViewButton, singleWorker);
        }
    }

    private GameObject CreateWorkerButton(Worker workerObject)
    {
        GameObject newWorkerButton = GameObject.Instantiate(ListViewButtonPrefab);
        Button buttonComponent = newWorkerButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnWorkerButtonClicked);

        Text buttonTextComponent = buttonComponent.GetComponentInChildren<Text>();
        string buttonText = string.Format("{0} {1} / {2} days / {3} $",
            workerObject.Name, workerObject.Surename, workerObject.ExperienceTime, workerObject.Salary);
        buttonTextComponent.text = buttonText;

        return newWorkerButton;
    }

    private void OnWorkerButtonClicked()
    {
        //We know its worker list button because its clicked event has been just called
        GameObject newSelectedButton = EventSystem.current.currentSelectedGameObject;

        if (newSelectedButton != SelectedWorkerButton)
        {
            if (null != SelectedWorkerButton)
            {
                Button previouslySelectedButtonComponent = SelectedWorkerButton.GetComponent<Button>();
                //Restore default values for button that was previously selected
                previouslySelectedButtonComponent.colors = WorkerSelectedButtonSavedColors;
            }

            Button buttonComponent = newSelectedButton.GetComponent<Button>();
            ColorBlock buttonColors = buttonComponent.colors;
            WorkerSelectedButtonSavedColors = buttonColors;
            //We remember button as selected long as any other worker button
            //won't be selected. That's why color will always stay even when
            //button is not reckognized as selected anymore by unity UI engine
            buttonColors.normalColor = SELECTED_WORKER_BUTTON_COLOR;
            buttonColors.selectedColor = SELECTED_WORKER_BUTTON_COLOR;
            buttonComponent.colors = buttonColors;

            SelectedWorkerButton = newSelectedButton;

            UpdateActionButtonsState();
        }
    }

    /// <summary>
    /// This method will enable or disable buttons used
    /// for hiring of firing workers based on which worker
    /// is selected (market worker or company worker)
    /// </summary>
    private void UpdateActionButtonsState()
    {
        Worker selectedWorker = ButtonWorkerDictionary[SelectedWorkerButton];

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
        GameObject newWorkerButton = CreateWorkerButton(addedWorker);
        MarketWorkersListView.AddControl(newWorkerButton);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
    }

    private void OnMarketWorkerRemoved(Worker removedWorker)
    {
        GameObject workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        MarketWorkersListView.RemoveControl(workerButton);
    }

    private void OnCompanyWorkerAdded(Worker addedWorker)
    {
        GameObject newWorkerButton = CreateWorkerButton(addedWorker);
        CompanyWorkersListView.AddControl(newWorkerButton);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
    }

    private void OnCompanyWorkerRemoved(Worker removedWorker)
    {
        GameObject workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        CompanyWorkersListView.RemoveControl(workerButton);
    }

    private void Start()
    {
        ButtonWorkerDictionary = new Dictionary<GameObject, Worker>();
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