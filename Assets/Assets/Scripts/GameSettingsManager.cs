using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsManager : MonoBehaviour
{
    public InputField TextCompanyName;
    public InputField TextNickname;

    /*Keys values for accessing settings from
    PlayerPrefs class*/
    private const string DEFAULT_KEY_VALUE = "";
    private const string COMPANY_NAME_KEY = "1";
    private const string NICKNAME_KEY = "2";

    public void ApplySettings()
    {
        PlayerPrefs.SetString(COMPANY_NAME_KEY, TextCompanyName.text);
        PlayerPrefs.SetString(NICKNAME_KEY, TextNickname.text);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        TextCompanyName.text = PlayerPrefs.GetString(COMPANY_NAME_KEY, DEFAULT_KEY_VALUE);
        TextNickname.text = PlayerPrefs.GetString(NICKNAME_KEY, DEFAULT_KEY_VALUE);
    }
}
