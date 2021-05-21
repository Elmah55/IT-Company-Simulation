using UnityEngine;
using System;

namespace ITCompanySimulation.Settings
{
    /// <summary>
    /// This class hold player defined preferences like nickname
    /// and company name that will be used during game
    /// </summary>
    public static class PlayerInfo
    {
        /*Private consts fields*/

        /// <summary>
        /// Default value for string values in PlayerPrefs
        /// </summary>
        private const string DEFAULT_STRING_KEY_VALUE = "";
        /// <summary>
        /// Default value for float values in PlayerPrefs
        /// </summary>
        private const float DEFAULT_FLOAT_KEY_VALUE = 0.0f;

        /*Keys values for accessing settings from
        PlayerPrefs class*/
        private const string COMPANY_NAME_KEY = "CompanyName";
        private const string NICKNAME_KEY = "PlayerNickname";

        /*Private fields*/

        private static string m_CompanyName;
        private static string m_Nickname;

        /*Public consts fields*/

        public const int COMPANY_NAME_MAX_LENGHT = 40;
        public const int PLAYER_NICKNAME_MAX_LENGTH = 40;

        /*Public fields*/

        public static string CompanyName
        {
            get
            {
                return m_CompanyName;
            }

            set
            {
                if (value.Length > COMPANY_NAME_MAX_LENGHT)
                {
                    throw new InvalidOperationException(
                        "Exceeded company name length. Max allowed length is " + COMPANY_NAME_MAX_LENGHT);
                }

                m_CompanyName = value;
            }
        }
        public static string Nickname
        {
            get
            {
                return m_Nickname;
            }

            set
            {
                if (value.Length > PLAYER_NICKNAME_MAX_LENGTH)
                {
                    throw new InvalidOperationException(
                        "Exceeded player nickname length. Max allowed length is " + PLAYER_NICKNAME_MAX_LENGTH);
                }

                m_Nickname = value;
            }
        }
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
            CompanyName = PlayerPrefs.GetString(COMPANY_NAME_KEY, DEFAULT_STRING_KEY_VALUE);
            Nickname = PlayerPrefs.GetString(NICKNAME_KEY, DEFAULT_STRING_KEY_VALUE);
            PhotonNetwork.player.NickName = Nickname;
        }
    }
}
