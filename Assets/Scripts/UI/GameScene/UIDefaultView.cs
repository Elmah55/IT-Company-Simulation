using ITCompanySimulation.Character;
using ITCompanySimulation.Company;
using ITCompanySimulation.Core;
using ITCompanySimulation.Settings;
using ITCompanySimulation.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class handles UI for default view. This is the view that is displayed
    /// after loading game scene.
    /// </summary>
    public class UIDefaultView : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// Determines how long notification should be displayed after
        /// receiving it. This is unscaled time.
        /// </summary>
        private const float PANEL_NOTIFICATIONS_DISPLAY_TIME = 5f;

        /*Private fields*/

        /// <summary>
        /// Displays current date in game.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI TextDate;
        /// <summary>
        /// Displays how many days have passed since start of simulation.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI TextDaysSinceStart;
        /// <summary>
        /// Displays number of workers in company controlled.
        /// by player
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI TextWorkersCount;
        [SerializeField]
        private TextMeshProUGUI TextCompanyBalance;
        [SerializeField]
        private ProgressBar ProgressBarBalance;
        [SerializeField]
        private GameTime GameTimeComponent;
        [SerializeField]
        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private TMP_Dropdown DropdownNotificationList;
        /// <summary>
        /// Panel containing buttons that open other UI panels
        /// </summary>
        [SerializeField]
        private GameObject PanelWindowsButtons;
        /// <summary>
        /// Panel that will display latest notification.
        /// </summary>
        [SerializeField]
        private GameObject PanelNotifications;
        [SerializeField]
        private TextMeshProUGUI TextPanelNotification;
        [SerializeField]
        private Image ImagePanelNotification;
        /// <summary>
        /// Timestamp indicating when notification panel was made visible.
        /// </summary>
        private float TimeSincePanelNotificationsVisible;
        private bool IsPanelNotificationsVisible;
        /// <summary>
        /// Image displayed in notification panel when notification priority
        /// is normal.
        /// </summary>
        [SerializeField]
        private Sprite SpriteNotificationPriorityNormal;
        /// <summary>
        /// Image displayed in notification panel when notification priority
        /// is high.
        /// </summary>
        [SerializeField]
        private Sprite SpriteNotificationPriorityHigh;


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
            SetBalanceTexts();
            TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
            SetProgressBarBalance(SimulationManagerComponent.ControlledCompany.Balance);
        }

        private void Update()
        {
            //Hide notifications panel if it was visible for specified period of time
            if (false == LeanTween.isTweening(PanelNotifications) && true == IsPanelNotificationsVisible)
            {
                if ((Time.unscaledTime - TimeSincePanelNotificationsVisible) >= PANEL_NOTIFICATIONS_DISPLAY_TIME)
                {
                    Vector2 panelNewLocation = new Vector2(0f, 580f);
                    LeanTween.moveLocal(PanelNotifications, panelNewLocation, 1f)
                        .setIgnoreTimeScale(true)
                        .setOnComplete(() =>
                        {
                            IsPanelNotificationsVisible = false;
                        });
                }
            }
        }

        private void SetBalanceTexts()
        {
            TextCompanyBalance.text = GetCompanyBalanceText(SimulationManagerComponent.ControlledCompany.Balance,
                                                            SimulationSettings.TargetBalance);
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

            //Run animation to display notification on a dedicated panel for PANEL_NOTIFICATIONS_DISPLAY_TIME
            //period of time
            if (false == LeanTween.isTweening(PanelNotifications) && false == IsPanelNotificationsVisible)
            {
                switch (notification.Priority)
                {
                    case SimulationEventNotificationPriority.Normal:
                        ImagePanelNotification.sprite = SpriteNotificationPriorityNormal;
                        break;
                    case SimulationEventNotificationPriority.High:
                        ImagePanelNotification.sprite = SpriteNotificationPriorityHigh;
                        break;
                    default:
                        ImagePanelNotification.sprite = SpriteNotificationPriorityNormal;
                        break;
                }

                TextPanelNotification.text = notification.Text;
                Vector2 panelNewLocation = new Vector2(0f, 510f);
                LeanTween.moveLocal(PanelNotifications, panelNewLocation, 1f)
                    .setIgnoreTimeScale(true)
                    .setOnComplete(() =>
                    {
                        IsPanelNotificationsVisible = true;
                        TimeSincePanelNotificationsVisible = Time.unscaledTime;
                    });
            }
        }

        private string GetCompanyBalanceText(int companyBalance, int targetBalance)
        {
            return string.Format("Balance: {0} $ / {1} $",
                                 companyBalance,
                                 targetBalance);
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

        private void OnControlledCompanyBalanceChanged(int newBalance, int balanceDelta)
        {
            TextCompanyBalance.text = GetCompanyBalanceText(newBalance, SimulationSettings.TargetBalance);
            SetProgressBarBalance(newBalance);
        }

        private void OnGameTimeDayChanged()
        {
            TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
            TextDate.text = GameTimeComponent.CurrentTime.ToLongDateString();
        }

        private void SetProgressBarBalance(float value)
        {
            //Map balance value to progess bar value
            float progressBarValue = Utils.MapRange(value,
                                                    0f,
                                                    SimulationSettings.TargetBalance,
                                                    ProgressBarBalance.MinimumValue,
                                                    ProgressBarBalance.MaximumValue);
            ProgressBarBalance.Value = progressBarValue;
        }

        /*Public methods*/

        /// <summary>
        /// Should be called by other windows on closing.
        /// </summary>
        public void OnWindowClosed()
        {
            PanelWindowsButtons.SetActive(true);
        }

        /// <summary>
        /// Should be called when other window is being open.
        /// </summary>
        public void OnWindowOpen()
        {
            PanelWindowsButtons.SetActive(false);
        }
    }
}
