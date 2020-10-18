using UnityEngine;

/// <summary>
/// This class hold player defined preferences like nickname
/// and company name that will be used during game
/// </summary>
// Derive from MonoBehaviour because PlayerPrefs.GetString cannot be used in constructor
public static class PlayerInfo
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

    public static string CompanyName { get; private set; }
    public static string Nickname { get; private set; }
    public static bool CredentialsCompleted
    {
        get
        {
            return false == string.IsNullOrWhiteSpace(CompanyName)
                && false == string.IsNullOrWhiteSpace(Nickname);
        }
    }

    /*Private methods*/

    /*Public methods*/

    public static void Apply(string companyName, string nickname)
    {
        CompanyName = companyName;
        Nickname = nickname;

        PlayerPrefs.SetString(COMPANY_NAME_KEY, companyName);
        PlayerPrefs.SetString(NICKNAME_KEY, nickname);
        PlayerPrefs.Save();

        PhotonNetwork.player.NickName = Nickname;
    }

    public static void Load()
    {
        CompanyName = PlayerPrefs.GetString(COMPANY_NAME_KEY, DEFAULT_KEY_VALUE);
        Nickname = PlayerPrefs.GetString(NICKNAME_KEY, DEFAULT_KEY_VALUE);
        PhotonNetwork.player.NickName = Nickname;
    }
}
