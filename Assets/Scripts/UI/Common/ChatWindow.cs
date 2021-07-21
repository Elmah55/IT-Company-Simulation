using UnityEngine;
using TMPro;
using ITCompanySimulation.Multiplayer;
using System.Text.RegularExpressions;
using ITCompanySimulation.Utilities;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Class for handling UI related to in-game chat.
    /// </summary>
    public class ChatWindow : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// How many of previous inputs should be stored.
        /// </summary>
        private const int INPUTS_HISTORY_STORAGE_COUNT = 50;

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
        [SerializeField]
        [Tooltip("Key that will activate input field of chat")]
        private KeyCode ActivateChatInputFieldKey;
        /// <summary>
        /// Key for fetching older input from saved inputs
        /// </summary>
        private KeyCode InputHistoryGetOlderKey = KeyCode.UpArrow;
        /// <summary>
        /// Key for fetching newer input from saved inputs
        /// </summary>
        private KeyCode InputHistoryGetNewerKey = KeyCode.DownArrow;
        /// <summary>
        /// Stores nickname of player that is target of private message
        /// because nickname of target player cannot be accessed in private
        /// message received callback.
        /// </summary>
        private string PrivateMsgTarget;
        /// <summary>
        /// Regex for match command for sending private message.
        /// Private message command: "/msg <targetPlayer> <message>"
        /// </summary>
        private Regex PrivateMsgRegex = new Regex(@"^\/msg (\S+) (.*)$");
        /// <summary>
        /// Buffer with stored inputs so they can be reused later if needed.
        /// </summary>
        private CircularBuffer<string> StoredInputs = new CircularBuffer<string>(INPUTS_HISTORY_STORAGE_COUNT);
        /// <summary>
        /// Index of input that is being fetched currently.
        /// </summary>
        private int StoredInputsCurrentIndex = 0;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void OnEnable()
        {
            InputFieldMessage.text = string.Empty;
            TextChatDisplay.text = string.Empty;
        }

        private void OnDestroy()
        {
            ChatComponent.MessageReceived -= OnChatMessageReceived;
            ChatComponent.PrivateMessageReceived -= OnChatPrivateMessageReceived;
            ChatComponent.Disconnected -= OnChatDisconnected;
            ChatComponent.Connected -= OnChatConnected;
        }

        private void Awake()
        {
            ChatComponent = GameObject.FindGameObjectWithTag("ApplicationManager").GetComponent<ChatManager>();
            ChatComponent.MessageReceived += OnChatMessageReceived;
            ChatComponent.PrivateMessageReceived += OnChatPrivateMessageReceived;
            ChatComponent.Disconnected += OnChatDisconnected;
            ChatComponent.Connected += OnChatConnected;

            if (false == ChatComponent.IsConnected)
            {
                OnChatConnectionFailed();
            }
        }

        private void OnChatConnected()
        {
            this.gameObject.SetActive(true);
        }

        private void OnChatDisconnected()
        {
            OnChatConnectionFailed();
        }

        private void HandleInputEntered(string input)
        {
            //Input starts with "/" try to send private message
            if (true == input.StartsWith("/"))
            {
                Match regexMatch = PrivateMsgRegex.Match(input);

                if (true == regexMatch.Success)
                {
                    string targetPlayer = regexMatch.Groups[1].Captures[0].ToString();
                    string msg = regexMatch.Groups[2].Captures[0].ToString();
                    bool result = ChatComponent.SendPrivateChatMessage(targetPlayer, msg);

                    if (false == result)
                    {
                        TextChatDisplay.text += string.Format("Could not send message to player \"{0}\"",
                                                              targetPlayer);
                    }
                    else
                    {
                        //Store nickname of target player to use it in private message callback later
                        PrivateMsgTarget = targetPlayer;
                    }
                }
                else
                {
                    TextChatDisplay.text += "Unkown command\n";
                }
            }
            //No command executing, just send chat message
            else
            {
                bool result = ChatComponent.SendChatMessage(input);
            }

            ClearInput();
        }

        /// <summary>
        /// Should be called after input was entered and sent.
        /// </summary>
        private void ClearInput()
        {
            InputFieldMessage.text = string.Empty;
            InputFieldMessage.ActivateInputField();
            //Start from most recent element.
            StoredInputsCurrentIndex = StoredInputs.Size;
        }

        /// <summary>
        /// Stores input so it can be reused later if needed.
        /// </summary>
        private void StoreInput(string input)
        {
            StoredInputs.Add(input);
        }

        private void Update()
        {
            if (true == Input.GetKeyDown(KeyCode.Return) &&
               (false == string.IsNullOrWhiteSpace(InputFieldMessage.text)))
            {
                StoreInput(InputFieldMessage.text);
                HandleInputEntered(InputFieldMessage.text);
            }

            if (true == Input.GetKeyDown(ActivateChatInputFieldKey))
            {
                InputFieldMessage.ActivateInputField();
            }

            if (StoredInputs.Size > 0)
            {
                if ((true == Input.GetKeyDown(InputHistoryGetNewerKey)) &&
                    (true == InputFieldMessage.IsActive()) &&
                    (StoredInputsCurrentIndex < StoredInputs.Size))
                {
                    StoredInputsCurrentIndex++;

                    //No newer element rest input field
                    if (StoredInputs.Size == StoredInputsCurrentIndex)
                    {
                        ClearInput();
                    }
                    else
                    {
                        InputFieldMessage.text = StoredInputs[StoredInputsCurrentIndex];
                    }
                }

                if ((true == Input.GetKeyDown(InputHistoryGetOlderKey)) &&
                    (true == InputFieldMessage.IsActive()) &&
                    (StoredInputsCurrentIndex >= 1))
                {
                    StoredInputsCurrentIndex--;
                    InputFieldMessage.text = StoredInputs[StoredInputsCurrentIndex];
                }
            }
        }

        private void OnChatMessageReceived(string senderNickname, string message)
        {
            string displayedMessage = string.Format("{0}: {1}\n",
                                                    senderNickname,
                                                    message);
            TextChatDisplay.text += displayedMessage;
        }

        private void OnChatPrivateMessageReceived(string senderNickname, string message)
        {
            string displayedMessage;

            //Callback will be invoked also on client that sent message
            if (null != PrivateMsgTarget && senderNickname == PhotonNetwork.player.NickName)
            {
                displayedMessage = string.Format("(You -> {0}): {1}\n",
                                 PrivateMsgTarget,
                                 message);
                PrivateMsgTarget = null;
            }
            else
            {
                displayedMessage = string.Format("({0} -> You): {1}\n",
                                                 senderNickname,
                                                 message);
            }

            TextChatDisplay.text += displayedMessage;
        }

        private void OnChatConnectionFailed()
        {
            //Disable this UI element if chat connection failed
            this.gameObject.SetActive(false);
        }

        /*Public methods*/
    }
}
