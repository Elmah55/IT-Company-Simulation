using UnityEngine;
using TMPro;
using ITCompanySimulation.Multiplayer;
using System.Text.RegularExpressions;
using ITCompanySimulation.Utilities;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ITCompanySimulation.UI
{
    /// <summary>
    /// Class for handling UI related to in-game chat.
    /// </summary>
    public class ChatWindow : MonoBehaviour
    {
        /*Private consts fields*/

        /// <summary>
        /// Value used to take into account floating point error of
        /// scroll rect vertical position.
        /// </summary>
        private const float SCROLL_RECT_TRESHOLD_VALUE = 0.01f;

        /*Private fields*/

        /// <summary>
        /// Input field that contains message that player wants
        /// to send
        /// </summary>
        [SerializeField]
        private TMP_InputField InputFieldMessage;
        private MultiplayerChatManager ChatManagerComponent;
        [SerializeField]
        [Tooltip("Key that will activate input field of chat")]
        private KeyCode ActivateChatInputFieldKey;
        /// <summary>
        /// Object that has layout group and is parent of all text elements with
        /// chat messages.
        /// </summary>
        [SerializeField]
        private HorizontalOrVerticalLayoutGroup TextLayout;
        [SerializeField]
        private ScrollRect ChatScrollRect;
        [SerializeField]
        private TextMeshProUGUI TextObjectPrefab;
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
        private CircularBuffer<string> StoredInputs;
        /// <summary>
        /// Index of input that is being fetched currently.
        /// </summary>
        private int StoredInputsCurrentIndex = 0;
        /// <summary>
        /// Number of chat text prefabs instantiated.
        /// </summary>
        private int NumberOfTextObjectsInstantiated;
        private int OldestMessageIndex;
        private int NewestMessageIndex = -1;
        /// <summary>
        /// Stores all messages of this chat window (including ones that are not
        /// displayed at the moment.
        /// </summary>
        private List<string> ChatMessages = new List<string>();
        [SerializeField]
        private Scrollbar ScrollBarChatWindow;

        /*Public consts fields*/

        /*Public fields*/

        [Tooltip("Number of user chat inputs that should be stored.")]
        [Range(1, 500)]
        public int InputsHistoryStorageSize = 50;
        [Tooltip("Number of text objects that will be instantiated for this chat window." +
            " After reaching this limit existing text objects will be reused to display messages.")]
        [Range(2, 100)]
        public int NumberOfTextObjects = 20;

        /*Private methods*/

        private void Awake()
        {
            StoredInputs = new CircularBuffer<string>(InputsHistoryStorageSize);
        }

        private void OnEnable()
        {
            ChatManagerComponent = MultiplayerChatManager.Instance;
            ChatManagerComponent.MessageReceived += OnChatMessageReceived;
            ChatManagerComponent.PrivateMessageReceived += OnChatPrivateMessageReceived;
            ChatManagerComponent.Disconnected += OnChatDisconnected;
            ChatManagerComponent.Connected += OnChatConnected;

            if (false == ChatManagerComponent.IsConnected)
            {
                OnChatConnectionFailed();
            }
        }

        private void OnDisable()
        {
            ChatManagerComponent.MessageReceived -= OnChatMessageReceived;
            ChatManagerComponent.PrivateMessageReceived -= OnChatPrivateMessageReceived;
            ChatManagerComponent.Disconnected -= OnChatDisconnected;
            ChatManagerComponent.Connected -= OnChatConnected;
        }

        private void OnChatConnected()
        {
            InputFieldMessage.text = string.Empty;
            InputFieldMessage.enabled = true;
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
                    bool result = ChatManagerComponent.SendPrivateChatMessage(targetPlayer, msg);

                    if (false == result)
                    {
                        string txt = string.Format("Could not send message to player \"{0}\"",
                                                    targetPlayer);
                        AddTextToChat(txt, true);
                    }
                    else
                    {
                        //Store nickname of target player to use it in private message callback later
                        PrivateMsgTarget = targetPlayer;
                    }
                }
                else
                {
                    AddTextToChat("Unkown command", true);
                }
            }
            //No command executing, just send chat message
            else
            {
                ChatManagerComponent.SendChatMessage(input);
            }

            ClearInput();
        }

        private void AddTextToChat(string txt, bool scrollToBottom)
        {
            ChatMessages.Add(txt);

            if (NumberOfTextObjectsInstantiated < NumberOfTextObjects)
            {
                TextMeshProUGUI newTextObject = GameObject.Instantiate(TextObjectPrefab, TextLayout.transform);
                NumberOfTextObjectsInstantiated++;
                NewestMessageIndex++;
                newTextObject.text = txt;
                newTextObject.gameObject.SetActive(true);
                newTextObject.transform.SetAsLastSibling();
            }

            if (true == scrollToBottom)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)TextLayout.transform);
                ChatScrollRect.verticalNormalizedPosition = 0f;
                HandleTextLayout();
            }
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
            ReadInput();
            HandleTextLayout();
        }

        private void HandleTextLayout()
        {
            if (TextLayout.transform.childCount > 3)
            {
                const int numberOfReusedTextObjects = 1;

                //User scrolled to top, load older messages
                if ((ChatScrollRect.verticalNormalizedPosition > (1f - SCROLL_RECT_TRESHOLD_VALUE))
                    && (OldestMessageIndex > 0))
                {
                    for (int i = 0; i < numberOfReusedTextObjects && OldestMessageIndex > 0; i++)
                    {
                        //Reuse text object most recent to display older message
                        TextMeshProUGUI textComponent =
                            TextLayout.transform.GetChild(TextLayout.transform.childCount - 1).GetComponent<TextMeshProUGUI>();
                        OldestMessageIndex--;
                        NewestMessageIndex--;
                        textComponent.text = ChatMessages[OldestMessageIndex];
                        textComponent.transform.SetSiblingIndex(1);
                    }

                    if (OldestMessageIndex != 0)
                    {
                        ChatScrollRect.verticalNormalizedPosition = 0.9f;
                        ScrollBarChatWindow.value = 0.9f;
                    }
                    else
                    {
                        ChatScrollRect.verticalNormalizedPosition = 1f;
                    }
                }

                //User scrolled to bottom, load newer messages
                if ((ChatScrollRect.verticalNormalizedPosition < SCROLL_RECT_TRESHOLD_VALUE)
                    && (NewestMessageIndex < ChatMessages.Count - 1))
                {
                    for (int i = 0; i < numberOfReusedTextObjects && NewestMessageIndex < ChatMessages.Count - 1; i++)
                    {
                        //Reuse text object to display newer message
                        TextMeshProUGUI textComponent = TextLayout.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                        OldestMessageIndex++;
                        NewestMessageIndex++;
                        textComponent.text = ChatMessages[NewestMessageIndex];
                        textComponent.transform.SetAsLastSibling();
                    }

                    if (NewestMessageIndex != ChatMessages.Count - 1)
                    {
                        ChatScrollRect.verticalNormalizedPosition = 0.1f;
                        ScrollBarChatWindow.value = 0.1f;
                    }
                    else
                    {
                        ChatScrollRect.verticalNormalizedPosition = 0f;
                    }
                }
            }
        }

        private void ReadInput()
        {
            if (true == Input.GetKeyDown(KeyCode.Return)
                && (false == string.IsNullOrWhiteSpace(InputFieldMessage.text)))
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

                    //No newer element, clear input field
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

        private void OnChatMessageReceived(ChatMessage message)
        {
            AddTextToChat(message.ToString(), true);
        }

        private void OnChatPrivateMessageReceived(ChatMessage message)
        {
            string displayedMessage;

            //Callback will be invoked also on client that sent message
            if (null != PrivateMsgTarget && message.Sender == PhotonNetwork.player.NickName)
            {
                displayedMessage = string.Format("(You -> {0}): {1}",
                                 PrivateMsgTarget,
                                 message.Message);
                PrivateMsgTarget = null;
            }
            //Message received from other client
            else
            {
                displayedMessage = string.Format("({0} -> You): {1}",
                                                 message.Sender,
                                                 message.Message);
            }

            AddTextToChat(displayedMessage, true);
        }

        private void OnChatConnectionFailed()
        {
            InputFieldMessage.text = "Chat offline";
            InputFieldMessage.enabled = false;
        }

        /*Public methods*/

        /// <summary>
        /// Clears all displayed messages and chat history.
        /// </summary>
        public void ClearChat()
        {
            ChatMessages.Clear();
            int textObjectsCount = TextLayout.transform.childCount;

            //Destroy all text objects except text prefab (index 0)
            for (int i = 1; i < textObjectsCount; i++)
            {
                GameObject objectToDestroy = TextLayout.transform.GetChild(i).gameObject;
                GameObject.Destroy(objectToDestroy);
            }

            NumberOfTextObjectsInstantiated = 0;
            OldestMessageIndex = 0;
            NewestMessageIndex = -1;

            ScrollBarChatWindow.Rebuild(CanvasUpdate.Layout);
        }
    }
}