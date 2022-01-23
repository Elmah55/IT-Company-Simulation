using ITCompanySimulation.Core;
using UnityEngine;
using TMPro;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Script controlling UI element with simulaion
    /// activity log.
    /// </summary>
    public class UIActivityLog : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        private SimulationManager SimulationManagerComponent;
        [SerializeField]
        private TextMeshProUGUI TextActivityLog;


        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public void Init()
        {
            SimulationManagerComponent =
                GameObject.FindGameObjectWithTag("ScriptsGameObject").GetComponent<SimulationManager>();
            SimulationManagerComponent.NotificatorComponent.NotificationReceived += OnNotificationReceived;
        }

        private void OnNotificationReceived(SimulationEventNotification notification)
        {
            //Use bold font for notification with high priority
            string notificationTxt = notification.Priority == SimulationEventNotificationPriority.High ?
                ("<b>" + notification.Text + "</b>") : (notification.Text);
            string acitvityLogTxt = string.Format("{0}.{1}.{2} - {3}\n",
                                                  notification.Timestamp.Day,
                                                  notification.Timestamp.Month,
                                                  notification.Timestamp.Year,
                                                  notificationTxt);
            TextActivityLog.text += acitvityLogTxt;
        }
    }
}
