using UnityEngine;
using TMPro;
using ITCompanySimulation.Multiplayer;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Class for handling UI related to in-game chat.
    /// </summary>
    public class Chat : MonoBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Input field that contains message that player wants
        /// to send
        /// </summary>
        [SerializeField]
        private TMP_InputField InputFieldMessage;
        /// <summary>
        /// All chat messages will be stored in this text
        /// component
        /// </summary>
        [SerializeField]
        private TextMeshProUGUI TextChatDisplay;
        private IMultiplayerChat ChatComponent;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnEnable()
        {
            InputFieldMessage.text = string.Empty;
            TextChatDisplay.text = string.Empty;
        }

        private void Awake()
        {
            ChatComponent = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ChatManager>();
            ChatComponent.MessageReceived += OnMessageReceived;
        }

        private void Update()
        {
            if (true == Input.GetKeyDown(KeyCode.Return) &&
               (false == string.IsNullOrWhiteSpace(InputFieldMessage.text)))
            {
                bool result = ChatComponent.SendChatMessage(InputFieldMessage.text);
                InputFieldMessage.text = string.Empty;
                InputFieldMessage.ActivateInputField();
            }
        }

        private void OnMessageReceived(string senderNickname, string message)
        {
            string displayedMessage = string.Format("{0}: {1}\n",
                                                    senderNickname,
                                                    message);
            TextChatDisplay.text += displayedMessage;
        }

        /*Public methods*/
    }
}
