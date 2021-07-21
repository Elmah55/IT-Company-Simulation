using UnityEngine.Events;

namespace ITCompanySimulation.Multiplayer
{
    public interface IMultiplayerChat
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Invoked when message from other client is received.
        /// </summary>
        event PhotonChatMessageAction MessageReceived;
        /// <summary>
        /// Invoked when private message from other client is received
        /// (message that only this client will receive).
        /// </summary>
        event PhotonChatMessageAction PrivateMessageReceived;
        event UnityAction Connected;
        event UnityAction Disconnected;
        bool IsConnected { get; }

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Sends message to other clients that joined chat
        /// </summary>
        /// <param name="msg">Message that will be sent</param>
        bool SendChatMessage(string msg);
        /// <summary>
        /// Sends message only to one specified player
        /// </summary>
        /// <param name="targetPlayerNickname"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool SendPrivateChatMessage(string targetPlayerNickname, string msg);
    }
}
