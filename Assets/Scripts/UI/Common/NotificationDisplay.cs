using TMPro;
using UnityEngine;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Components for displaying pending notifactions
    /// </summary>
    public class NotificationDisplay : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        [Tooltip("Text that is used to display pending notification. For example: " +
                 "When there is one pending notification \"1\" will be displayed next to the UI element. " +
                 "If there are ten or more notifications \"9+\" will be displayed.")]
        [SerializeField]
        private TextMeshProUGUI NotificationText;
        [Tooltip("Game object that is activated when notification appears " +
                 "or is deactivated when there is no pending notifications.")]
        [SerializeField]
        private GameObject NotificationObject;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Number of notifications that are currently pending.
        /// </summary>
        public int PendingNotifications { get; private set; }

        /*Private methods*/

        private void Start()
        {
            ClearAllNotifications();
        }

        /*Public methods*/

        public void Notify()
        {
            PendingNotifications++;
            string notificationMsg = PendingNotifications > 9 ? "9+" : PendingNotifications.ToString();
            NotificationText.text = notificationMsg;
            NotificationObject.gameObject.SetActive(true);
        }

        public void ClearAllNotifications()
        {
            NotificationObject.SetActive(false);
            PendingNotifications = 0;
        }
    }
}
