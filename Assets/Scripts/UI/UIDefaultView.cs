using ITCompanySimulation.Character;
using ITCompanySimulation.Core;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// This class handles UI for default view. This is the main view and is
/// active as first in game scene
/// </summary>
public class UIDefaultView : MonoBehaviour
{
    /*Private consts fields*/

    /*Private fields*/

    /// <summary>
    /// Displays current date in game
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI TextDate;
    /// <summary>
    /// Displays how many days have passed since start of simulation
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI TextDaysSinceStart;
    /// <summary>
    /// Displays number of workers in company controlled
    /// by player
    /// </summary>
    [SerializeField]
    private TextMeshProUGUI TextWorkersCount;
    [SerializeField]
    private TextMeshProUGUI TextCompanyBalanceCurrent;
    [SerializeField]
    private TextMeshProUGUI TextCompanyBalanceMinimal;
    [SerializeField]
    private TextMeshProUGUI TextCompanyBalanceTarget;
    [SerializeField]
    private GameTime GameTimeComponent;
    [SerializeField]
    private MainSimulationManager SimulationManagerComponent;
    [SerializeField]
    private TMP_Dropdown DropdownNotificationList;

    /*Public consts fields*/

    /*Public fields*/

    /*Private methods*/

    private void Start()
    {
        //Game time
        GameTimeComponent.DayChanged += OnGameTimeDayChanged;
        TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
        TextDate.text = GameTimeComponent.CurrentTime.ToLongDateString();

        //Company
        SimulationManagerComponent.ControlledCompany.BalanceChanged += OnControlledCompanyBalanceChanged;
        SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAddedOrRemoved;
        SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerAddedOrRemoved;
        SimulationManagerComponent.NotificatorComponent.NotificationReceived += OnNotificationReceived;
        SimulationManagerComponent.SettingsUpdated += OnSimulationSettingsUpdated;
        SetBalanceTexts();
        TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
    }

    private void OnSimulationSettingsUpdated(SimulationSettings obj)
    {
        SetBalanceTexts();
    }

    private void SetBalanceTexts()
    {
        TextCompanyBalanceCurrent.text = GetCompanyBalanceText(SimulationManagerComponent.ControlledCompany.Balance);
        TextCompanyBalanceMinimal.text = GetMinimalBalanceText(SimulationManagerComponent.Settings.MinimalBalance);
        TextCompanyBalanceTarget.text = GetTargetBalanceText(SimulationManagerComponent.Settings.TargetBalance);
    }

    private void OnNotificationReceived(SimulationEventNotification notification)
    {
        List<TMP_Dropdown.OptionData> dropdownOptions = new List<TMP_Dropdown.OptionData>();
        string option = string.Format("{0}.{1}.{2} {3}",
                                      notification.Timestamp.Day,
                                      notification.Timestamp.Month,
                                      notification.Timestamp.Year,
                                      notification.Text);
        dropdownOptions.Add(new TMP_Dropdown.OptionData(option));
        dropdownOptions.AddRange(DropdownNotificationList.options);
        DropdownNotificationList.options = dropdownOptions;
    }

    private string GetCompanyBalanceText(int companyBalance)
    {
        return string.Format("Balance: {0} $",
                             companyBalance);
    }

    private string GetTargetBalanceText(int target)
    {
        return string.Format("Target: {0} $",
                             target);
    }

    private string GetMinimalBalanceText(int minimal)
    {
        return string.Format("Minimal: {0} $",
                             minimal);
    }

    private string GetWorkersCountText(int workersCount)
    {
        return string.Format("Workers: {0}/{1}",
                             workersCount,
                             PlayerCompany.MAX_WORKERS_PER_COMPANY);
    }

    private string GetDaysSinceStartText(int days)
    {
        return "Day " + days;
    }

    private void OnControlledCompanyWorkerAddedOrRemoved(LocalWorker companyWorker)
    {
        TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
    }

    private void OnControlledCompanyBalanceChanged(int newBalance)
    {
        TextCompanyBalanceCurrent.text = GetCompanyBalanceText(newBalance);
    }

    private void OnGameTimeDayChanged()
    {
        TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
        TextDate.text = GameTimeComponent.CurrentTime.ToLongDateString();
    }

    /*Public methods*/
}
