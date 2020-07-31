using ITCompanySimulation.Character;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    private Text TextDate;
    /// <summary>
    /// Displays how many days have passed since start of simulation
    /// </summary>
    [SerializeField]
    Text TextDaysSinceStart;
    /// <summary>
    /// Displays number of workers in company controlled
    /// by player
    /// </summary>
    [SerializeField]
    private Text TextWorkersCount;
    [SerializeField]
    private Text TextCompanyBalance;
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
        TextCompanyBalance.text = GetCompanyBalanceText(SimulationManagerComponent.ControlledCompany.Balance);
        TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
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
        return string.Format("Money: {0} $ ({1} $) ",
                             companyBalance,
                             SimulationManagerComponent.GameManagerComponent.SettingsOfSimulation.TargetBalance);
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

    private void OnControlledCompanyWorkerAddedOrRemoved(SharedWorker companyWorker)
    {
        TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
    }

    private void OnControlledCompanyBalanceChanged(int newBalance)
    {
        TextCompanyBalance.text = GetCompanyBalanceText(newBalance);
    }

    private void OnGameTimeDayChanged()
    {
        TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
        TextDate.text = GameTimeComponent.CurrentTime.ToLongDateString();
    }

    /*Public methods*/
}
