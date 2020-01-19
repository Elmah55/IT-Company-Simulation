using UnityEngine;

/// <summary>
/// This class hold player defined preferences like nickname
/// and company name that will be used during game
/// </summary>
public class PlayerInfoSettings : MonoBehaviour
{
    /*Private consts fields*/

    /*Keys values for accessing settings from
    PlayerPrefs class*/
    private const string DEFAULT_KEY_VALUE = "";
    private const string COMPANY_NAME_KEY = "1";
    private const string NICKNAME_KEY = "2";

    /*Private fields*/

    /*Public consts fields*/

    /*Public fields*/

    public string CompanyName { get; private set; }
    public string Nickname { get; private set; }

    /*Private methods*/

    /*Public methods*/

    public void ApplySettings(string companyName, string nickname)
    {
        CompanyName = companyName;
        Nickname = nickname;

        PlayerPrefs.SetString(COMPANY_NAME_KEY, companyName);
        PlayerPrefs.SetString(NICKNAME_KEY, nickname);
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        CompanyName = PlayerPrefs.GetString(COMPANY_NAME_KEY, DEFAULT_KEY_VALUE);
        Nickname = PlayerPrefs.GetString(NICKNAME_KEY, DEFAULT_KEY_VALUE);
    }
}
