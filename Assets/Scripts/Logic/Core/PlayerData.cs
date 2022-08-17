using ITCompanySimulation.Character;
using System.Collections.Generic;
using System;
using System.Text;

namespace ITCompanySimulation.Core
{
    /// <summary>
    /// Class that store data of other player in simulation. This data will be shared
    /// between all clients in the room.
    /// </summary>
    public class PlayerData
    {
        /*Private consts fields*/

        /*Private fields*/

        private int m_CompanyBalance;

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Dictionary containing all workers in player's company. Worker ID as key and SharedWorker
        /// instance as value.
        /// </summary>
        public Dictionary<int, SharedWorker> Workers { get; private set; } = new Dictionary<int, SharedWorker>();
        /// <summary>
        /// Name of company that player controls.
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Simulation related statisticss of this player.
        /// </summary>
        public int CompanyBalance
        {
            get
            {
                return m_CompanyBalance;
            }

            set
            {
                m_CompanyBalance = value;
                CompanyBalanceUpdated?.Invoke(this);
            }
        }
        /// <summary>
        /// Player that this data is related to.
        /// </summary>
        public PhotonPlayer Player { get; set; }
        public event Action<PlayerData> CompanyBalanceUpdated;

        /*Private methods*/

        /*Public methods*/

        public static byte[] Serialize(object playerDataObject)
        {
            PlayerData playerDataInstance = (PlayerData)playerDataObject;
            byte[] companyNameLengthBytes = BitConverter.GetBytes(playerDataInstance.CompanyName.Length * sizeof(char));
            byte[] companyNameBytes = Encoding.Unicode.GetBytes(playerDataInstance.CompanyName);
            byte[] companyBalanceBytes = BitConverter.GetBytes(playerDataInstance.CompanyBalance);

            int playerDataBytesLength =
                companyNameLengthBytes.Length +
                companyNameBytes.Length +
                companyBalanceBytes.Length;
            int offset = 0;
            byte[] playerDataBytes = new byte[playerDataBytesLength];

            Array.Copy(companyNameLengthBytes, 0, playerDataBytes, offset, companyNameLengthBytes.Length);
            offset += companyNameLengthBytes.Length;
            Array.Copy(companyNameBytes, 0, playerDataBytes, offset, companyNameBytes.Length);
            offset += companyNameBytes.Length;
            Array.Copy(companyBalanceBytes, 0, playerDataBytes, offset, companyBalanceBytes.Length);

            return playerDataBytes;
        }

        public static object Deserialize(byte[] playerDataBytes)
        {
            int offset = 0;
            int companyNameLength = BitConverter.ToInt32(playerDataBytes, offset);
            offset += sizeof(int);
            string companyName = Encoding.Unicode.GetString(playerDataBytes, offset, companyNameLength);
            offset += companyNameLength;
            int companyBalance = BitConverter.ToInt32(playerDataBytes, offset);

            PlayerData deserializedPlayerData = new PlayerData();
            deserializedPlayerData.CompanyName = companyName;
            deserializedPlayerData.CompanyBalance = companyBalance;

            return deserializedPlayerData;

        }
    }
}
