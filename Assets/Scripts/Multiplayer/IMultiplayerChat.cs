namespace ITCompanySimulation.Multiplayer
{
    public interface IMultiplayerChat
    {
        /*Private consts fields*/

        /*Private fields*/

        /*Public consts fields*/

        /*Public fields*/

        /// <summary>
        /// Invoked when message from other client is received
        /// </summary>
        event PhotonChatMessageAction MessageReceived;
        bool Connected { get; }

        /*Private methods*/

        /*Public methods*/

        /// <summary>
        /// Sends message to other clients that joined chat
        /// </summary>
        /// <param name="msg"></param>
        bool SendChatMessage(string msg);
    }
}
