using UnityEngine;
using UnityEngine.UI;

public class UICreateRoom : Photon.PunBehaviour
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
    private bool MinimalBalanceValid = true;
    private bool RoomNameValid = false;
    [SerializeField]
    private MainGameManager GameManagerComponent;
    [SerializeField]
    private GameObject RoomLobbyPanel;
    [SerializeField]
    private InputField InputFieldRoomName;
    [SerializeField]
    private InputField InputFieldTargetBalance;
    [SerializeField]
    private InputField InputFieldInitialBalance;
    [SerializeField]
    private InputField InputFieldMinimalBalance;
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
    private Text TextRoomName;
    [SerializeField]
    private Text TextNumberOfPlayers;
    [SerializeField]
    private Text TextTargetBalance;
    [SerializeField]
    private Text TextInitialBalance;
    [SerializeField]
    private Text TextMinimalBalance;

    //Below are defined "HelperText" fields that will display
    //information with hints how to fill in input field 
    //when coresponding input field will contain invalid values
    [SerializeField]
    private Text InputFieldTargetBalanceHelperText;
    [SerializeField]
    private Text InputFieldInitialBalanceHelperText;
    [SerializeField]
    private Text InputFieldMinimalBalanceHelperText;

    /*Public consts fields*/

    /*Public fields*/

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
        SliderInitialBalance.value = SliderTargetBalance.value - 50000;
        SliderNumberOfPlayers.minValue = MainGameManager.MIN_NUMBER_OF_PLAYERS_PER_ROOM;
        SliderNumberOfPlayers.maxValue = MainGameManager.MAX_NUMBER_OF_PLAYERS_PER_ROOM;
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

        //Initial balance cannot be higher or equal to target balance
        //otherwise all players would lose game at start
        SliderMinimalBalance.maxValue = SliderInitialBalance.value - 1;

        InitialBalanceValid = true;
        InputFieldInitialBalanceHelperText.gameObject.SetActive(false);
        CheckInput();
    }

    public void OnSliderMinimalBalanceValueChanged(float value)
    {
        TextMinimalBalance.text = "Minimal balance " + SliderMinimalBalance.value + " $";
        InputFieldMinimalBalance.text = SliderMinimalBalance.value.ToString() + " $";
        MinimalBalanceValid = true;
        InputFieldMinimalBalanceHelperText.gameObject.SetActive(false);
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
                                                      InputFieldMinimalBalanceHelperText,
                                                      SliderMinimalBalance,
                                                      SimulationSettings.MIN_MINIMAL_BALANCE,
                                                      (int)(SliderInitialBalance.value - 1)); //Minimal value cannot be bigger than initial
        if (true == InitialBalanceValid)
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

    public void OnCreateRoomButtonClicked()
    {
        GameManagerComponent.SettingsOfSimulation.InitialBalance = (int)SliderInitialBalance.value;
        GameManagerComponent.SettingsOfSimulation.TargetBalance = (int)SliderTargetBalance.value;
        GameManagerComponent.SettingsOfSimulation.MinimalBalance = (int)SliderMinimalBalance.value;

        RoomOptions options = new RoomOptions() { MaxPlayers = (byte)SliderNumberOfPlayers.value };

        PhotonNetwork.CreateRoom(InputFieldRoomName.text, options, PhotonNetwork.lobby);

        ButtonCreateRoom.interactable = false;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        RoomLobbyPanel.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public override void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        base.OnPhotonJoinRoomFailed(codeAndMsg);

        ButtonCreateRoom.interactable = true;
    }
}
