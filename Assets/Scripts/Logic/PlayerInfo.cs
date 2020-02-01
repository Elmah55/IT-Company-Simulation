using UnityEngine;

/// <summary>
/// This class hold player defined preferences like nickname
/// and company name that will be used during game
/// </summary>
public class PlayerInfo : MonoBehaviour
{
    /*Private consts fields*/

    /*Keys values for accessing settings from
    PlayerPrefs class*/
    private const string DEFAULT_KEY_VALUE = "";
    private const string COMPANY_NAME_KEY = "1";
    private const string NICKNAME_KEY = "2";

    /*Private fields*/

    /*Public consts fields*/

    public const int COMPANY_NAME_MAX_LENGHT = 40;

    /*Public fields*/

    public string CompanyName { get; private set; }
    public string Nickname { get; private set; }

    /*Private methods*/

    private void Start()
    {
        Load();
    }

    /*Public methods*/

    public void Apply(string companyName, string nickname)
    {
        CompanyName = companyName;
        Nickname = nickname;

        PlayerPrefs.SetString(COMPANY_NAME_KEY, companyName);
        PlayerPrefs.SetString(NICKNAME_KEY, nickname);
        PlayerPrefs.Save();

        PhotonNetwork.player.NickName = this.Nickname;
    }

    public void Load()
    {
        CompanyName = PlayerPrefs.GetString(COMPANY_NAME_KEY, DEFAULT_KEY_VALUE);
        Nickname = PlayerPrefs.GetString(NICKNAME_KEY, DEFAULT_KEY_VALUE);
        PhotonNetwork.player.NickName = this.Nickname;
    }
}
