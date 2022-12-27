using ITCompanySimulation.Character;
using ITCompanySimulation.Company;
using ITCompanySimulation.Core;
using ITCompanySimulation.Settings;
using ITCompanySimulation.Utilities;
using ITCompanySimulation.Multiplayer;
using ITCompanySimulation.Event;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ITCompanySimulation.Project;
using System;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// This class handles UI for default view. This is the view that is displayed
    /// after loading game scene.
    /// </summary>
    public class UIDefaultView : MonoBehaviour, IDisposable
    {
        /*Private consts fields*/

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
        [SerializeField]
        private ChatWindow ChatWindowComponent;
        [SerializeField]
        private NotificationDisplay ButtonActivityLogNotificationDisplay;
        [SerializeField]
        private UIActivityLog UIActivityLogComponent;
        /// <summary>
        /// Notification display component of button that opens chat window.
        /// </summary>
        [SerializeField]
        private NotificationDisplay NotificationDisplayChatWindowButton;
        private MultiplayerChatManager ChatComponent;
        /// <summary>
        /// Sound effect played when comapny's project is completed.
        /// </summary>
        [SerializeField]
        private AudioClip SoundEffectProjectCompleted;
        [SerializeField]
        private UISoundRequestEvent UISoundPlayRequest;
        /// <summary>
        /// Text used for animation when company's balance is increased.
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI TextCompanyBalanceAdded;
        private bool ChatWindowVisible;

        /*Public consts fields*/

        /*Public fields*/

        [Tooltip("For how many seconds panel displaying notification should be visible" +
        " after notification was received.")]
        public float PanelNotificationDisplayTime;

        /*Private methods*/

        private void Start()
        {
            //Game time
            GameTimeComponent.DayChanged += OnGameTimeDayChanged;
            TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
            TextDate.text = GameTimeComponent.CurrentDate.ToLongDateString();

            //Company
            SimulationManagerComponent.ControlledCompany.BalanceChanged += OnControlledCompanyBalanceChanged;
            SimulationManagerComponent.ControlledCompany.WorkerAdded += OnControlledCompanyWorkerAddedOrRemoved;
            SimulationManagerComponent.ControlledCompany.WorkerRemoved += OnControlledCompanyWorkerAddedOrRemoved;
            SimulationManagerComponent.ControlledCompany.ProjectAdded += OnControlledCompanyProjectAdded;
            SimulationManagerComponent.NotificatorComponent.NotificationReceived += OnNotificationReceived;
            SetBalanceTexts();
            TextWorkersCount.text = GetWorkersCountText(SimulationManagerComponent.ControlledCompany.Workers.Count);
            SetProgressBarBalance(SimulationManagerComponent.ControlledCompany.Balance);

            //Get chat component reference and subscribe to events
            ChatComponent = MultiplayerChatManager.Instance;
            ChatComponent.MessageReceived += OnChatMessageReceived;
            ChatComponent.PrivateMessageReceived += OnChatMessageReceived;

            //Initialization for UI components that are inactive when scene is loaded
            UIActivityLogComponent.Init();

            ApplicationManager.RegisterObjectForCleanup(this);
        }

        private void OnChatMessageReceived(ChatMessage message)
        {
            //No need to notify player about incoming message when chat window
            //is already open
            if (false == ChatWindowVisible)
            {
                NotificationDisplayChatWindowButton.Notify();
            }
        }

        private void SetBalanceTexts()
        {
            TextCompanyBalance.text = GetCompanyBalanceText(SimulationManagerComponent.ControlledCompany.Balance,
                                                            SimulationSettings.TargetBalance);
        }

        private void OnNotificationReceived(SimulationEventNotification notification)
        {
            //No need to set notification on button when activity log UI is active
            if (false == UIActivityLogComponent.gameObject.activeSelf)
            {
                ButtonActivityLogNotificationDisplay.Notify();
            }

            //Run animation to display notification on a dedicated panel for specified
            //period of time
            if (false == LeanTween.isTweening(PanelNotifications))
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
                RectTransform notificationPanelTransform = (RectTransform)PanelNotifications.transform;
                float panelOldLocationY = notificationPanelTransform.localPosition.y;
                //Move panel along Y axis based on panel's height so whole panel can be visible in canvas
                float panelNewLocationY = notificationPanelTransform.localPosition.y - notificationPanelTransform.sizeDelta.y;
                float animationTime = 1f;
                LTDescr showPanelTween = LeanTween.moveLocalY(PanelNotifications, panelNewLocationY, animationTime)
                    .setIgnoreTimeScale(true);
                LTDescr hidePanelTween = LeanTween.moveLocalY(PanelNotifications, panelOldLocationY, animationTime)
                    .setIgnoreTimeScale(true);
                LTSeq panelTweenSeq = LeanTween.sequence();
                panelTweenSeq.append(showPanelTween);
                panelTweenSeq.append(PanelNotificationDisplayTime);
                panelTweenSeq.append(hidePanelTween);
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

            if ((balanceDelta > 0) && (false == LeanTween.isTweening(TextCompanyBalanceAdded.gameObject)))
            {
                //Run animation displaying amount by which balance was increased 
                Transform textTransform = TextCompanyBalanceAdded.gameObject.transform;
                Vector2 initialPosition = textTransform.localPosition;
                Vector2 targetPosition = new Vector2(initialPosition.x, initialPosition.y + 250f);
                Color initialColor = TextCompanyBalanceAdded.color;

                TextCompanyBalanceAdded.text = string.Format("+{0} $", balanceDelta);
                TextCompanyBalanceAdded.gameObject.SetActive(true);
                LeanTween.moveLocalY(TextCompanyBalanceAdded.gameObject, targetPosition.y, 3f)
                    .setIgnoreTimeScale(true)
                    .setOnComplete(() =>
                    {
                        TextCompanyBalanceAdded.gameObject.SetActive(false);
                        TextCompanyBalanceAdded.color = initialColor;
                        textTransform.localPosition = initialPosition;
                    })
                    .setOnUpdate((float value) =>
                    {
                        if (value >= 0.5f)
                        {
                            //Start fading text
                            Color updatedColor = TextCompanyBalanceAdded.color;
                            float colorAlpha = 1f - Utils.MapRange(value, 0.5f, 1f, 0f, 1f);
                            updatedColor.a = colorAlpha;
                            TextCompanyBalanceAdded.color = updatedColor;
                        }
                    });
            }
        }

        private void OnControlledCompanyProjectAdded(Scrum scrumObj)
        {
            scrumObj.BindedProject.Completed += OnCompanyProjectCompleted;
        }

        private void OnCompanyProjectCompleted(LocalProject proj)
        {
            UISoundRequestEventArgs args = new UISoundRequestEventArgs(SoundEffectProjectCompleted);
            UISoundPlayRequest.RaiseEvent(args);
        }

        private void OnGameTimeDayChanged()
        {
            TextDaysSinceStart.text = GetDaysSinceStartText(GameTimeComponent.DaysSinceStart);
            TextDate.text = GameTimeComponent.CurrentDate.ToLongDateString();
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

        /// <summary>
        /// Toggles chat window off and on.
        /// </summary>
        public void ToggleChatWindow()
        {
            RectTransform chatWindowTransform = (RectTransform)ChatWindowComponent.gameObject.transform;

            if (false == LeanTween.isTweening(ChatWindowComponent.gameObject))
            {
                if (false == ChatWindowVisible)
                {
                    ChatWindowVisible = true;
                    LeanTween.moveLocalX(ChatWindowComponent.gameObject, chatWindowTransform.localPosition.x + chatWindowTransform.sizeDelta.x, 1f)
                        .setIgnoreTimeScale(true)
                        .setEaseOutCirc();
                }
                else
                {
                    ChatWindowVisible = false;
                    LeanTween.moveLocalX(ChatWindowComponent.gameObject, chatWindowTransform.localPosition.x - chatWindowTransform.sizeDelta.x, 1f)
                        .setIgnoreTimeScale(true)
                        .setEaseOutCirc();
                } 
            }
        }

        public void Dispose()
        {
            ChatComponent.PrivateMessageReceived -= OnChatMessageReceived;
            ChatComponent.MessageReceived -= OnChatMessageReceived;
        }
    }
}