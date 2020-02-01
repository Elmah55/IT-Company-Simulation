using UnityEngine;
using UnityEngine.UI;

public class UICreateRoom : MonoBehaviour
{
    /*Private consts fields*/

    /// <summary>
    /// Stores color of input field that will be set when
    /// players wants to create the room but values in input fields
    /// are invalid (e.g string is provided instead of integer)
    /// </summary>
    private readonly ColorBlock INPUT_FIELD_INVALID_VALUE_COLOR =
        new ColorBlock()
        {
            normalColor = new Color(217.0f, 73.0f, 73.0f, 255.0f),
            selectedColor = new Color(217.0f, 73.0f, 73.0f, 255.0f),
            colorMultiplier = 1.0f
        };

    /*Private fields*/

    /// <summary>
    /// Used to store previous color of input field in case it
    /// no longer needs to be InputFieldInvalidValueColor. Only
    /// one field of this type is required since all input fields
    /// should have same normal color
    /// </summary>
    private ColorBlock InputFieldNormalColor;
    private bool TargetBalanceValid = true;
    private bool InitialBalanceValid = true;
    private bool RoomNameValid = false;

    /*Public consts fields*/

    /*Public fields*/

    public SimulationSettings SimulationSettingsComponent;
    public GameObject RoomLobbyPanel;
    public InputField InputFieldRoomName;
    public InputField InputFieldTargetBalance;
    public InputField InputFieldInitialBalance;
    public Button ButtonCreateRoom;
    public Slider SliderNumberOfPlayers;
    public Slider SliderTargetBalance;
    public Slider SliderInitialBalance;
    public Text TextRoomName;
    public Text TextNumberOfPlayers;
    public Text TextTargetBalance;
    public Text TextInitialBalance;

    //Below are defined "HelperText" fields that will display
    //information with hints how to fill in input field 
    //when coresponding input field will contain invalid values
    public Text InputFieldTargetBalanceHelperText;
    public Text InputFieldInitialBalanceHelperText;

    /*Private methods*/

    private void Start()
    {
        InputFieldRoomName.characterLimit = PlayerInfo.COMPANY_NAME_MAX_LENGHT;
        InputFieldNormalColor = InputFieldRoomName.colors;

        SliderTargetBalance.minValue = SimulationSettings.MIN_TARGET_BALANCE;
        SliderTargetBalance.maxValue = SimulationSettings.MAX_TARGET_BALANCE;
        SliderTargetBalance.value = SliderTargetBalance.maxValue / 2.0f;
        SliderInitialBalance.minValue = SimulationSettings.MIN_INITIAL_BALANCE;
        SliderInitialBalance.maxValue = SliderTargetBalance.value - 1;
        SliderInitialBalance.value = SliderInitialBalance.maxValue / 2.0f;
        SliderNumberOfPlayers.minValue = MainGameManager.MIN_NUMBER_OF_PLAYERS_PER_ROOM;
        SliderNumberOfPlayers.maxValue = MainGameManager.MAX_NUMBER_OF_PLAYERS_PER_ROOM;

        TextNumberOfPlayers.text =
            "Maximum number of players " + SliderNumberOfPlayers.value.ToString();
        TextTargetBalance.text =
            "Target balance " + SliderTargetBalance.value + " $";
        TextInitialBalance.text =
            "Initial balance " + SliderInitialBalance.value + " $";
    }

    /// <summary>
    /// Checks input from given input field and displays information to player
    /// or sets input withing proper range when input is invalid
    /// </summary>
    /// <returns>True if input is correct otherwise false</returns>
    private bool CheckAndSetNumericInput(InputField input, Text helperText, Slider inputSlider, int minValue, int maxValue)
    {
        bool result = false;

        int value;
        if (true == int.TryParse(input.text, out value))
        {
            value = Mathf.Clamp(value, minValue, maxValue);
            input.text = value.ToString();
            inputSlider.value = value;
            helperText.gameObject.SetActive(false);
            result = true;
        }
        else
        {
            //TODO Fix settings colors
            //input.colors = INPUT_FIELD_INVALID_VALUE_COLOR;
            helperText.gameObject.SetActive(true);
        }

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
        else if (InputFieldRoomName.text.Length <= PlayerInfo.COMPANY_NAME_MAX_LENGHT)
        {
            int charactersLeft = PlayerInfo.COMPANY_NAME_MAX_LENGHT - InputFieldRoomName.text.Length;
            string msg = string.Format("Room name ({0} characters left)", charactersLeft);
            TextRoomName.text = msg;
            result = true;
            ButtonCreateRoom.interactable = true;
        }

        foreach (RoomInfo info in PhotonNetwork.GetRoomList())
        {
            if (info.Name == InputFieldRoomName.text)
            {
                TextRoomName.text = "Room with this name already exsists";
                ButtonCreateRoom.interactable = false;
                break;
            }
        }
        return result;
    }

    private void CheckInput()
    {
        bool result = InitialBalanceValid && TargetBalanceValid && RoomNameValid;
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

        //Initial balance cannot be higher or equal to target balance
        //otherwise all players would win game at start
        SliderInitialBalance.maxValue = SliderTargetBalance.value - 1;

        TargetBalanceValid = true;
        InputFieldTargetBalanceHelperText.gameObject.SetActive(false);
        CheckInput();
    }

    public void OnSliderInitialBalanceValueChanged(float value)
    {
        TextInitialBalance.text = "Initial balance " + SliderInitialBalance.value + " $";
        InputFieldInitialBalance.text = SliderInitialBalance.value.ToString() + " $";
        InitialBalanceValid = true;
        InputFieldInitialBalanceHelperText.gameObject.SetActive(false);
        CheckInput();
    }

    public void OnInputFieldTargetBalanceEndEdit(string value)
    {
        TargetBalanceValid = CheckAndSetNumericInput(InputFieldTargetBalance,
                                                     InputFieldTargetBalanceHelperText,
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
                                                      InputFieldInitialBalanceHelperText,
                                                      SliderInitialBalance,
                                                      SimulationSettings.MIN_INITIAL_BALANCE,
                                                      SimulationSettings.MAX_INITIAL_BALANCE);
        if (true == InitialBalanceValid)
        {
            TextInitialBalance.text = "Initial balance " + InputFieldInitialBalance.text + " $";
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

    public void OnCreateRoomButtonClicked()
    {
        SimulationSettingsComponent.InitialBalance = (int)SliderInitialBalance.value;
        SimulationSettingsComponent.TargetBalance = (int)SliderTargetBalance.value;
        RoomOptions options = new RoomOptions() { MaxPlayers = (byte)SliderNumberOfPlayers.value };

        PhotonNetwork.CreateRoom(InputFieldRoomName.text, options, PhotonNetwork.lobby);

        RoomLobbyPanel.SetActive(true);
        this.gameObject.SetActive(false);
    }
}
