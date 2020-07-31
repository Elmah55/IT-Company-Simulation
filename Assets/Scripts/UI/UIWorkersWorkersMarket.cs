using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using ITCompanySimulation.Character;

public class UIWorkersWorkersMarket : UIWorkers
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Used to map button in workers list to appriopriate worker
    /// </summary>
    private Dictionary<Button, SharedWorker> ButtonWorkerDictionary = new Dictionary<Button, SharedWorker>();
    [SerializeField]
    private ControlListView ListViewMarketWorkers;
    [SerializeField]
    private ControlListView ListViewCompanyWorkers;
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private WorkersMarket WorkersMarketComponent;
    [SerializeField]
    private Button ButtonHireWorker;
    [SerializeField]
    private Button ButtonFireWorker;
    private IButtonSelector WorkersButtonSelector = new ButtonSelector();
    private SharedWorker SelectedWorker;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void InitializeWorkersListView(ControlListView listView, List<SharedWorker> workers)
    {
        foreach (SharedWorker singleWorker in workers)
        {
            Button newListViewButton = CreateWorkerButton(singleWorker);

            listView.AddControl(newListViewButton.gameObject);
            ButtonWorkerDictionary.Add(newListViewButton, singleWorker);
            WorkersButtonSelector.AddButton(newListViewButton);
        }
    }

    private Button CreateWorkerButton(SharedWorker workerObject)
    {
        Button newWorkerButton = GameObject.Instantiate<Button>(ListViewButtonPrefab);

        Text buttonTextComponent = newWorkerButton.GetComponentInChildren<Text>();
        string buttonText = string.Format("{0} {1} / {2} days / {3} $",
            workerObject.Name, workerObject.Surename, workerObject.ExperienceTime, workerObject.Salary);
        buttonTextComponent.text = buttonText;

        return newWorkerButton;
    }

    private void OnSelectedWorkerButtonChanged(Button workerButton)
    {
        if (null != workerButton)
        {
            SharedWorker selectedWorker = ButtonWorkerDictionary[WorkersButtonSelector.GetSelectedButton()];

            InputFieldDaysInCompany.gameObject.SetActive(selectedWorker is LocalWorker);

            UpdateActionButtonsState(selectedWorker);
            UpdateWorkerInfo(selectedWorker);
        }
        else
        {
            ClearWorkerInfo();
            UpdateActionButtonsState(null);
        }
    }

    /// <summary>
    /// This method will enable or disable buttons used
    /// for hiring of firing workers based on which worker
    /// is selected (market worker or company worker)
    /// </summary>
    private void UpdateActionButtonsState(SharedWorker selectedWorker)
    {
        if (null != selectedWorker)
        {
            //Does not have company assigned so its market worker
            if (false == (selectedWorker is LocalWorker))
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
        else
        {
            ButtonFireWorker.interactable = false;
            ButtonHireWorker.interactable = false;
        }
    }

    private void OnMarketWorkerAdded(SharedWorker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        ListViewMarketWorkers.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.AddButton(newWorkerButton);
    }

    private void OnMarketWorkerRemoved(SharedWorker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        ListViewMarketWorkers.RemoveControl(workerButton.gameObject);
        ButtonWorkerDictionary.Remove(workerButton);
        WorkersButtonSelector.RemoveButton(workerButton);
    }

    private void OnCompanyWorkerAdded(SharedWorker addedWorker)
    {
        Button newWorkerButton = CreateWorkerButton(addedWorker);
        ListViewCompanyWorkers.AddControl(newWorkerButton.gameObject);
        ButtonWorkerDictionary.Add(newWorkerButton, addedWorker);
        WorkersButtonSelector.AddButton(newWorkerButton);
    }

    private void OnCompanyWorkerRemoved(SharedWorker removedWorker)
    {
        Button workerButton = ButtonWorkerDictionary.First(x => x.Value == removedWorker).Key;
        ListViewCompanyWorkers.RemoveControl(workerButton.gameObject);
        ButtonWorkerDictionary.Remove(workerButton);
        WorkersButtonSelector.RemoveButton(workerButton);
    }

    private void Start()
    {
        InitializeWorkersListView(ListViewMarketWorkers, WorkersMarketComponent.Workers);
        InitializeWorkersListView(ListViewCompanyWorkers, SimulationManagerComponent.ControlledCompany.Workers.Cast<SharedWorker>().ToList());

        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnCompanyWorkerAdded;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnCompanyWorkerRemoved;

        WorkersMarketComponent.WorkerAdded += OnMarketWorkerAdded;
        WorkersMarketComponent.WorkerRemoved += OnMarketWorkerRemoved;

        WorkersButtonSelector.SelectedButtonChanged += OnSelectedWorkerButtonChanged;
    }

    private void SubscribeToWorkerEvents(LocalWorker selectedWorker)
    {
        selectedWorker.SalaryChanged += UpdateWorkerInfo;
        selectedWorker.DaysInCompanyChanged += UpdateWorkerInfo;
    }

    private void UnsubscribeFromWorkerEvent(LocalWorker deselectedWorker)
    {
        deselectedWorker.SalaryChanged -= UpdateWorkerInfo;
        deselectedWorker.DaysInCompanyChanged -= UpdateWorkerInfo;
    }

    /*Public methods*/

    public void OnHireWorkerButtonClicked()
    {
        SharedWorker selectedWorker = ButtonWorkerDictionary[WorkersButtonSelector.GetSelectedButton()];
        WorkersMarketComponent.RemoveWorker(selectedWorker);

        LocalWorker newLocalWorker = selectedWorker as LocalWorker;

        if (null == newLocalWorker)
        {
            newLocalWorker = new LocalWorker(selectedWorker);
        }

        SimulationManagerComponent.ControlledCompany.AddWorker(newLocalWorker);
    }

    public void OnFireWorkerButtonClicked()
    {
        SharedWorker selectedWorker = ButtonWorkerDictionary[WorkersButtonSelector.GetSelectedButton()];
        SimulationManagerComponent.ControlledCompany.RemoveWorker((LocalWorker)selectedWorker);
        WorkersMarketComponent.AddWorker(selectedWorker);
    }
}