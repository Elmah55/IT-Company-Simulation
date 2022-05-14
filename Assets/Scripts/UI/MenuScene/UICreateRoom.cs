using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using ITCompanySimulation.Multiplayer;
using ITCompanySimulation.Core;
using ITCompanySimulation.Settings;
using Photon;

namespace ITCompanySimulation.UI
{
    public class UICreateRoom : PunBehaviour
    {
        /*Private consts fields*/

        /*Private fields*/

        /// <summary>
        /// Used to store previous color of input field in case it
        /// no longer needs to be InputFieldInvalidValueColor. Only
        /// one field of this type is required since all input fields
        /// should have same normal color
        /// </summary>
        private Color InputFieldNormalColor;
        /// <summary>
        /// Stores color of input field that will be set when
        /// players wants to create the room but values in input fields
        /// are invalid (e.g string is provided instead of integer)
        /// </summary>
        [SerializeField]
        private Color InputFieldInvalidColor;
        private bool TargetBalanceValid = true;
        private bool InitialBalanceValid = true;
        private bool MinimalBalanceValid = true;
        private bool RoomNameValid = false;
        [SerializeField]
        private ApplicationManager ApplicationManagerComponent;
        [SerializeField]
        private GameObject RoomLobbyPanel;
        [SerializeField]
        private TMP_InputField InputFieldRoomName;
        [SerializeField]
        private TMP_InputField InputFieldTargetBalance;
        [SerializeField]
        private TMP_InputField InputFieldInitialBalance;
        [SerializeField]
        private TMP_InputField InputFieldMinimalBalance;
        [SerializeField]
        private Button ButtonCreateRoom;
        [SerializeField]
        private Slider SliderNumberOfPlayers;
        [SerializeField]
        private Slider SliderTargetBalance;
        [SerializeField]
        private Slider SliderInitialBalance;
        [SerializeField]
        private Slider SliderMinimalBalance;
        [SerializeField]
        private TextMeshProUGUI TextRoomName;
        [SerializeField]
        private TextMeshProUGUI TextNumberOfPlayers;
        [SerializeField]
        private TextMeshProUGUI TextTargetBalance;
        [SerializeField]
        private TextMeshProUGUI TextInitialBalance;
        [SerializeField]
        private TextMeshProUGUI TextMinimalBalance;
        [SerializeField]
        private InfoWindow InfoWindowComponent;

        //Below are defined "HelperText" fields that will display
        //information with hints how to fill in input field 
        //when coresponding input field will contain invalid values
        [SerializeField]
        private TextMeshProUGUI TextInputFieldTargetBalanceHelper;
        [SerializeField]
        private TextMeshProUGUI TextInputFieldInitialBalanceHelper;
        [SerializeField]
        private TextMeshProUGUI TextInputFieldMinimalBalanceHelper;

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        private void Start()
        {
            InputFieldRoomName.characterLimit = PlayerInfoSettings.COMPANY_NAME_MAX_LENGHT;
            InputFieldNormalColor = InputFieldRoomName.GetComponent<Image>().color;

            SliderTargetBalance.minValue = SimulationSettings.MIN_TARGET_BALANCE;
            SliderTargetBalance.maxValue = SimulationSettings.MAX_TARGET_BALANCE;
            SliderTargetBalance.value = SliderTargetBalance.maxValue / 2.0f;
            SliderInitialBalance.minValue = SimulationSettings.MIN_INITIAL_BALANCE;
            SliderInitialBalance.maxValue = SliderTargetBalance.value - 1;
            SliderInitialBalance.value = SliderTargetBalance.value - 50000;
            SliderNumberOfPlayers.minValue = ApplicationManager.MIN_NUMBER_OF_PLAYERS_PER_ROOM;
            SliderNumberOfPlayers.maxValue = ApplicationManager.MAX_NUMBER_OF_PLAYERS_PER_ROOM;
            SliderMinimalBalance.maxValue = SimulationSettings.MAX_MINIMAL_BALANCE;
            SliderMinimalBalance.minValue = SimulationSettings.MIN_MINIMAL_BALANCE;
            SliderMinimalBalance.value = SliderInitialBalance.value - 100000;

            TextNumberOfPlayers.text =
                "Maximum number of players " + SliderNumberOfPlayers.value;
            TextTargetBalance.text =
                "Target balance " + SliderTargetBalance.value + " $";
            TextInitialBalance.text =
                "Initial balance " + SliderInitialBalance.value + " $";
            TextMinimalBalance.text =
                "Minimal balance " + SliderMinimalBalance.value + " $";
        }

        /// <summary>
        /// Checks input from given input field and displays information to player
        /// or sets input withing proper range when input is invalid
        /// </summary>
        /// <returns>True if input is correct otherwise false</returns>
        private bool CheckAndSetNumericInput(TMP_InputField input, TextMeshProUGUI helperText, Slider inputSlider, int minValue, int maxValue)
        {
            bool result = false;
            string regexPattern = @"^ *(-?[0-9]+) *\$? *$";
            string helperTextString = string.Empty;
            Match regexMatch = Regex.Match(input.text, regexPattern);

            if (true == regexMatch.Success)
            {
                int value;
                string valueStr = regexMatch.Groups[1].Value;
                if (true == int.TryParse(valueStr, out value))
                {
                    value = Mathf.Clamp(value, minValue, maxValue);
                    input.text = value.ToString();
                    inputSlider.value = value;
                    input.GetComponent<Image>().color = InputFieldNormalColor;
                    result = true;
                }
                else
                {
                    helperTextString = "Provided value is out of range";
                }
            }
            else
            {
                helperTextString = "Enter numeric value";
            }

            if (false == result)
            {
                input.GetComponent<Image>().color = InputFieldInvalidColor;
            }

            helperText.text = helperTextString;

            return result;
        }

        /// <summary>
        /// Checks input from room name input field and displays information to player
        /// when input is invalid
        /// </summary>
        /// <returns>True if input is correct otherwise false</returns>
        private bool CheckAndSetRoomName()
        {
            bool result = false;

            if (string.Empty == InputFieldRoomName.text)
            {
                TextRoomName.text = "Room name";

                ButtonCreateRoom.interactable = false;
            }
            else if (InputFieldRoomName.text.Length <= PlayerInfoSettings.COMPANY_NAME_MAX_LENGHT)
            {
                int charactersLeft = PlayerInfoSettings.COMPANY_NAME_MAX_LENGHT - InputFieldRoomName.text.Length;
                string msg = string.Format("Room name ({0} characters left)", charactersLeft);
                TextRoomName.text = msg;
                result = true;
                ButtonCreateRoom.interactable = true;
            }

            return result;
        }

        private void CheckInput()
        {
            bool result = InitialBalanceValid && TargetBalanceValid && RoomNameValid && MinimalBalanceValid;
            ButtonCreateRoom.interactable = result;

            //No need to check max number of players since its value is always
            //taken from slider's value
        }

        /*Public methods*/

        public void OnSliderNumberOfPlayersValueChanged(float value)
        {
            TextNumberOfPlayers.text = "Maximum number of players " + SliderNumberOfPlayers.value;
        }

        public void OnSliderTargetBalanceValueChanged(float value)
        {
            TextTargetBalance.text = "Target balance " + SliderTargetBalance.value + " $";
            InputFieldTargetBalance.text = SliderTargetBalance.value.ToString() + " $";
            InputFieldTargetBalance.GetComponent<Image>().color = InputFieldNormalColor;

            //Initial balance cannot be higher or equal to target balance
            //otherwise all players would win game at start
            SliderInitialBalance.maxValue = SliderTargetBalance.value - 1;

            TargetBalanceValid = true;
            TextInputFieldTargetBalanceHelper.text = string.Empty;
            CheckInput();
        }

        public void OnSliderInitialBalanceValueChanged(float value)
        {
            TextInitialBalance.text = "Initial balance " + SliderInitialBalance.value + " $";
            InputFieldInitialBalance.text = SliderInitialBalance.value.ToString() + " $";
            InputFieldInitialBalance.GetComponent<Image>().color = InputFieldNormalColor;

            //Initial balance cannot be higher or equal to target balance
            //otherwise all players would lose game at start
            SliderMinimalBalance.maxValue = SliderInitialBalance.value - 1;

            InitialBalanceValid = true;
            TextInputFieldInitialBalanceHelper.text = string.Empty;
            CheckInput();
        }

        public void OnSliderMinimalBalanceValueChanged(float value)
        {
            TextMinimalBalance.text = "Minimal balance " + SliderMinimalBalance.value + " $";
            InputFieldMinimalBalance.text = SliderMinimalBalance.value.ToString() + " $";
            InputFieldMinimalBalance.GetComponent<Image>().color = InputFieldNormalColor;
            MinimalBalanceValid = true;
            TextInputFieldMinimalBalanceHelper.text = string.Empty;
            CheckInput();
        }

        public void OnInputFieldTargetBalanceEndEdit(string value)
        {
            TargetBalanceValid = CheckAndSetNumericInput(InputFieldTargetBalance,
                                                         TextInputFieldTargetBalanceHelper,
                                                         SliderTargetBalance,
                                                         SimulationSettings.MIN_TARGET_BALANCE,
                                                         SimulationSettings.MAX_TARGET_BALANCE);

            if (true == TargetBalanceValid)
            {
                TextTargetBalance.text = "Target balance " + InputFieldTargetBalance.text + " $";
                CheckInput();
            }
        }

        public void OnInputFieldInitialBalanceEndEdit(string value)
        {
            InitialBalanceValid = CheckAndSetNumericInput(InputFieldInitialBalance,
                                                          TextInputFieldInitialBalanceHelper,
                                                          SliderInitialBalance,
                                                          SimulationSettings.MIN_INITIAL_BALANCE,
                                                          (int)(SliderTargetBalance.value - 1)); //Initial value cannot be bigger than target
            if (true == InitialBalanceValid)
            {
                TextInitialBalance.text = "Initial balance " + InputFieldInitialBalance.text + " $";
                CheckInput();
            }
        }

        public void OnInputFiledMinimalBalanceEndEdit(string value)
        {
            MinimalBalanceValid = CheckAndSetNumericInput(InputFieldMinimalBalance,
                                                          TextInputFieldMinimalBalanceHelper,
                                                          SliderMinimalBalance,
                                                          SimulationSettings.MIN_MINIMAL_BALANCE,
                                                          (int)(SliderInitialBalance.value - 1)); //Minimal value cannot be bigger than initial
            if (true == MinimalBalanceValid)
            {
                TextMinimalBalance.text = "Minimal balance " + InputFieldMinimalBalance.text + " $";
                CheckInput();
            }
        }

        public void OnInputFieldRoomNameValueChanged(string newText)
        {
            RoomNameValid = CheckAndSetRoomName();

            if (true == RoomNameValid)
            {
                CheckInput();
            }
        }

        public void OnButtonCreateRoomClicked()
        {
            SimulationSettings.InitialBalance = (int)SliderInitialBalance.value;
            SimulationSettings.TargetBalance = (int)SliderTargetBalance.value;
            SimulationSettings.MinimalBalance = (int)SliderMinimalBalance.value;

            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)SliderNumberOfPlayers.value;
            options.CleanupCacheOnLeave = true;

            //Photon requires to use string keys
            Hashtable roomProperties = new Hashtable();
            roomProperties.Add(
               RoomCustomPropertiesKey.SettingsOfSimulationInitialBalance.ToString(), SimulationSettings.InitialBalance);
            roomProperties.Add(
                RoomCustomPropertiesKey.SettingsOfSimulationTargetBalance.ToString(), SimulationSettings.TargetBalance);
            roomProperties.Add(
                RoomCustomPropertiesKey.SettingsOfSimulationMinimalBalance.ToString(), SimulationSettings.MinimalBalance);
            options.CustomRoomProperties = roomProperties;

            ButtonCreateRoom.interactable = false;
            PhotonNetwork.CreateRoom(InputFieldRoomName.text, options, PhotonNetwork.lobby);
            UIRoom.SetPhotonPlayerRoomLobbyState(RoomLobbyPlayerState.NotReady);
        }

        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();

            RoomLobbyPanel.SetActive(true);
            this.gameObject.SetActive(false);
        }

        public override void OnPhotonCreateRoomFailed(object[] codeAndMsg)
        {
            base.OnPhotonCreateRoomFailed(codeAndMsg);
            ButtonCreateRoom.interactable = true;
            string errorMsg = string.Format("Failed to create room\n" +
                                            "{0}\n" +
                                            "Error code: {1}",
                                            codeAndMsg[1],
                                            codeAndMsg[0]);
            InfoWindowComponent.ShowOk(errorMsg, null);
        }
    }
}
