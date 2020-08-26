using ExitGames.Client.Photon;
using ITCompanySimulation.Multiplayer;

namespace ITCompanySimulation.UI
{
    public static class UIRoom
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /*Private methods*/

        /*Public methods*/

        public static void SetPhotonPlayerRoomLobbyState(RoomLobbyPlayerState state)
        {
            if (false == PhotonNetwork.offlineMode)
            {
                Hashtable customProperties = PhotonNetwork.player.CustomProperties;
                customProperties.Clear();
                customProperties.Add(PlayerCustomPropertiesKey.RoomLobbyPlayerState.ToString(), state);
                PhotonNetwork.player.SetCustomProperties(customProperties);
            }
        }
    }
}
