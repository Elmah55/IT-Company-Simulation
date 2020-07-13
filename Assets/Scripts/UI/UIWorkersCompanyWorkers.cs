using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkersCompanyWorkers : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    [SerializeField]
    private IButtonSelector WorkersButtonsSelector = new ButtonSelector();
    [SerializeField]
    private Button ListViewWorkersButtonPrefab;
    [SerializeField]
    private ControlListView ListViewWorkers;
    [SerializeField]
    private Button ButtonFireWorker;
    [SerializeField]
    private Button ButtonGiveSalaryRaise;
    [SerializeField]
    private Slider SliderSalaryRaiseAmount;
    [SerializeField]
    private Text TextSliderSalaryRaiseAmount;
    [SerializeField]
    private Text TextWorkerInfo;
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private Color32 DefaultWorkerButtonColor;
    //Used to update list view and button selector
    //when worker is added or removed from company
    private Dictionary<Worker, Button> WorkerButtonMap = new Dictionary<Worker, Button>();
    private Worker SelectedWorker;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private Button CreateWorkerButton(Worker companyWorker)
    {
        Button newWorkerButton = GameObject.Instantiate<Button>(ListViewWorkersButtonPrefab);
        Text buttonText = newWorkerButton.GetComponentInChildren<Text>();
        ColorBlock buttonColors = newWorkerButton.colors;
        buttonColors.normalColor = DefaultWorkerButtonColor;
        newWorkerButton.colors = buttonColors;
        buttonText.text = CreateWorkerButtonText(companyWorker);
        return newWorkerButton;
    }

    private string CreateWorkerButtonText(Worker companyWorker)
    {
        return string.Format("{0} {1} Satisfaction: {2}%",
                             companyWorker.Name, 
                             companyWorker.Surename,
                             companyWorker.Satiscation.ToString("0.00"));
    }

    private void InitWorkersView()
    {
        //First remove workers that were displayed previously since worker
        //collection might have changed
        foreach (KeyValuePair<Worker, Button> worker in WorkerButtonMap)
        {
            RemoveWorkersListViewButton(worker.Key, false);
        }

        WorkerButtonMap.Clear();

        List<Worker> companyWorkers = SimulationManagerComponent.ControlledCompany.Workers;

        foreach (Worker companyWorker in companyWorkers)
        {
            CreateWorkersListViewButton(companyWorker);
        }

        //This is called to update all UI elements associated with this slider
        OnSalaryRaiseAmountSliderValueChanged(SliderSalaryRaiseAmount.value);
    }

    private void SubscribeToWorkerEvents(Worker companyWorker)
    {
        companyWorker.AbilityUpdated += OnCompanyWorkerAbilityUpdated;
        companyWorker.SatisfactionChanged += UpdateWorkerInfo;
        companyWorker.DaysInCompanyChanged += UpdateWorkerInfo;
    }

    private void UnsubscribeFromWorkerEvents(Worker companyWorker)
    {
        companyWorker.AbilityUpdated -= OnCompanyWorkerAbilityUpdated;
        companyWorker.SatisfactionChanged -= UpdateWorkerInfo;
        companyWorker.DaysInCompanyChanged -= UpdateWorkerInfo;
    }

    private void OnCompanyWorkerAbilityUpdated(Worker companyWorker, ProjectTechnology workerAbility, float workerAbilityValue)
    {
        UpdateWorkerInfo(companyWorker);
    }

    private void CreateWorkersListViewButton(Worker companyWorker)
    {
        Button workerButton = CreateWorkerButton(companyWorker);
        WorkersButtonsSelector.AddButton(workerButton);
        ListViewWorkers.AddControl(workerButton.gameObject);
        WorkerButtonMap.Add(companyWorker, workerButton);
    }

    private void RemoveWorkersListViewButton(Worker buttonWorker, bool removeFromMap = true)
    {
        Button buttonToRemove = WorkerButtonMap[buttonWorker];
        WorkersButtonsSelector.RemoveButton(buttonToRemove);
        ListViewWorkers.RemoveControl(buttonToRemove.gameObject);

        if (true == removeFromMap)
        {
            WorkerButtonMap.Remove(buttonWorker);
        }
    }

    private string CreateWorkerInfoString(Worker companyWorker)
    {
        string workerInfo = string.Format("Name: {0}\n" +
                                          "Surename: {1}\n" +
                                          "Abilities: ",
                                          companyWorker.Name, companyWorker.Surename);

        foreach (KeyValuePair<ProjectTechnology, float> workerAbility in companyWorker.Abilites)
        {
            workerInfo += string.Format("{0} {1} | ", workerAbility.Key, workerAbility.Value.ToString("0.00"));
        }

        workerInfo += string.Format("\nSalary: {0}$\n" +
                                    "Satisfaction: {1}%\n" +
                                    "Days in company: {2}\n",
                                    companyWorker.Salary, companyWorker.Satiscation.ToString("0.00"), 
                                    companyWorker.DaysInCompany);

        return workerInfo;
    }

    private void OnWorkersButtonsSelectorSelectedButtonChanged(Button obj)
    {
        if (null != obj)
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromWorkerEvents(SelectedWorker);
            }

            Worker companyWorker = WorkerButtonMap.First(x => x.Value == obj).Key;
            SelectedWorker = companyWorker;
            SubscribeToWorkerEvents(SelectedWorker);
            ButtonFireWorker.interactable = true;
            ButtonGiveSalaryRaise.interactable = true;
            SliderSalaryRaiseAmount.interactable = true;
            UpdateWorkerInfo(companyWorker);
        }
        else
        {
            if (null != SelectedWorker)
            {
                UnsubscribeFromWorkerEvents(SelectedWorker);
            }

            SelectedWorker = null;
            ButtonFireWorker.interactable = false;
            ButtonGiveSalaryRaise.interactable = false;
            SliderSalaryRaiseAmount.interactable = false;
            TextWorkerInfo.text = string.Empty;
        }
    }

    private void OnControlledCompanyWorkerRemoved(Worker companyWorker)
    {
        RemoveWorkersListViewButton(companyWorker);
        companyWorker.SatisfactionChanged -= OnControlledCompanyWorkerSatisfactionChanged;
    }

    private void OnControlledCompanyWorkerAdded(Worker companyWorker)
    {
        CreateWorkersListViewButton(companyWorker);
        companyWorker.SatisfactionChanged += OnControlledCompanyWorkerSatisfactionChanged;
    }


    private void UpdateWorkerInfo(Worker companyWorker)
    {
        TextWorkerInfo.text = CreateWorkerInfoString(companyWorker);
    }

    private void Start()
    {
        SliderSalaryRaiseAmount.onValueChanged.AddListener((val) => { OnSalaryRaiseAmountSliderValueChanged(val); });
        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAdded;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerRemoved;
        WorkersButtonsSelector.SelectedButtonChanged += OnWorkersButtonsSelectorSelectedButtonChanged;

        foreach (Worker companyWorker in SimulationManagerComponent.ControlledCompany.Workers)
        {
            companyWorker.SatisfactionChanged += OnControlledCompanyWorkerSatisfactionChanged;
        }
    }

    private void OnControlledCompanyWorkerSatisfactionChanged(Worker companyWorker)
    {
        Button workerButton = WorkerButtonMap[companyWorker];
        Text buttonTextComponent = workerButton.GetComponentInChildren<Text>();
        buttonTextComponent.text = CreateWorkerButtonText(companyWorker);

        //Change color of button to express worker satisfaction
        if (null != WorkersButtonsSelector.GetSelectedButton() &&
            workerButton.GetInstanceID() != WorkersButtonsSelector.GetSelectedButton().GetInstanceID())
        {
            ColorBlock buttonColors = workerButton.colors;
            float gColorComponentChange = (50.0f - companyWorker.Satiscation) * (-2.0f);

            Color newNormalColor = new Color32()
            {
                r = DefaultWorkerButtonColor.r,
                g = (byte)(DefaultWorkerButtonColor.g + (byte)gColorComponentChange),
                b = DefaultWorkerButtonColor.b,
                a = DefaultWorkerButtonColor.a
            };

            buttonColors.normalColor = newNormalColor;
            workerButton.colors = buttonColors;
        }
    }

    private void OnEnable()
    {
        InitWorkersView();
    }

    /*Public methods*/

    public void OnFireWorkerButtonClicked()
    {
        Button selectedButton = WorkersButtonsSelector.GetSelectedButton();
        Worker workerToRemove = WorkerButtonMap.First(x => x.Value == selectedButton).Key;
        SimulationManagerComponent.ControlledCompany.RemoveWorker(workerToRemove);
    }

    public void OnGiveSalaryRaiseButtonClicked()
    {
        int salaryRaiseAmount = (int)SliderSalaryRaiseAmount.value;
        Button selectedButton = WorkersButtonsSelector.GetSelectedButton();
        Worker companyWorker = WorkerButtonMap.First(x => x.Value == selectedButton).Key;
        companyWorker.SetSalary(companyWorker.Salary + salaryRaiseAmount);
    }

    public void OnSalaryRaiseAmountSliderValueChanged(float value)
    {
        TextSliderSalaryRaiseAmount.text = string.Format("{0} $", value);
    }
}
