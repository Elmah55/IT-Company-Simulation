﻿using ExitGames.Client.Photon;
using Photon.Chat;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Photon;
using System.Collections.Generic;

namespace ITCompanySimulation.Multiplayer
{
    public class MultiplayerChatManager : PunBehaviour, IChatClientListener
    {
        /*Private consts fields*/

        /*Private fields*/

        private ChatClient Client;
        /// <summary>
        /// Name of channel that client is subscribed to.
        /// Null if no channel is subscribed
        /// </summary>
        private string Channel;

        /*Public consts fields*/

        /*Public fields*/

        public bool IsConnected { get; private set; }
        public event PhotonChatMessageAction MessageReceived;
        public event PhotonChatMessageAction PrivateMessageReceived;
        public event UnityAction Connected;
        public event UnityAction Disconnected;
        /// <summary>
        /// List of all sent and received messages. Cleared when channel
        /// is changed or rejoined.
        /// </summary>
        public List<ChatMessage> Messages { get; private set; } = new List<ChatMessage>();
        /// <summary>
        /// Only instance of multiplayer chat manager.
        /// </summary>
        public static MultiplayerChatManager Instance { get; private set; }

        /*Private methods*/

        private void Awake()
        {
            if (null != Instance)
            {
                Debug.LogErrorFormat("[{0}] Only one instance of {0} should exist but is instantiated multiple times.",
                     this.GetType().Name);
            }

            Instance = this;
        }

        private void Init()
        {
            Client = new ChatClient(this);
            Photon.Chat.AuthenticationValues auth = new Photon.Chat.AuthenticationValues(PhotonNetwork.player.NickName);
            Client.Connect(PhotonNetwork.PhotonServerSettings.ChatAppID, PhotonNetwork.versionPUN, auth);
        }

        private void Update()
        {
            if (null != Client)
            {
                Client.Service();
            }
        }

        /*Public methods*/

        public bool SendChatMessage(string msg)
        {
            bool result = false;

            if (null != Client)
            {
                result = Client.PublishMessage(Channel, msg);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (false == result)
                {
                    Debug.LogErrorFormat("[{0}] Failed to send message on channel: {1}",
                                 this.GetType().Name,
                                 Channel);
                }
#endif
            }

            return result;
        }

        public bool SendPrivateChatMessage(string targetPlayerNickname, string msg)
        {
            bool result = false;

            if (null != Client)
            {
                result = Client.SendPrivateMessage(targetPlayerNickname, msg);

#if DEVELOPMENT_BUILD || UNITY_EDITOR
                if (false == result)
                {
                    Debug.LogErrorFormat("[{0}] Failed to send private message. Channel: {1} Target player: {2}",
                                         this.GetType().Name,
                                         Channel,
                                         targetPlayerNickname);
                }
#endif 
            }

            return result;
        }

        public void DebugReturn(DebugLevel level, string message) { }

        public void OnChatStateChange(ChatState state) { }

        public void OnConnected()
        {
            IsConnected = true;
            Connected?.Invoke();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogFormat("[{0}] Connected",
                            this.GetType().Name);
#endif
        }

        public void OnDisconnected()
        {
            IsConnected = false;
            Disconnected?.Invoke();

#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.LogFormat("[{0}] Disconencted",
                            this.GetType().Name);
#endif
        }

        public void OnGetMessages(string channelName, string[] senders, object[] messages)
        {
            if (Channel == channelName)
            {
                for (int i = 0; i < senders.Length; i++)
                {
                    string receivedMessageString = messages[i].ToString();
                    ChatMessage receivedMessage = new ChatMessage(senders[i], receivedMessageString);
                    this.Messages.Add(receivedMessage);
                    MessageReceived?.Invoke(receivedMessage);
                }
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            else
            {
                Debug.LogWarningFormat("[{0}] Received message from channel \"{1}\" but expected messages only from " +
                                       "channel \"{2}\" Client is subscribed to {3} channels",
                                        this.GetType().Name,
                                        channelName,
                                        Channel,
                                        Client.PublicChannels.Count);
            }
#endif
        }

        public void OnPrivateMessage(string sender, object message, string channelName)
        {
            ChatMessage receivedMessage = new ChatMessage(sender, message.ToString());
            this.Messages.Add(receivedMessage);
            PrivateMessageReceived?.Invoke(receivedMessage);
        }

        public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }

        public void OnSubscribed(string[] channels, bool[] results)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            //User should be subcribed to only one channel at once
            for (int i = 0; i < channels.Length; i++)
            {
                Debug.LogFormat("[{0}] Subscribed to channel: {1} ({2})",
                    this.GetType().Name,
                    channels[i],
                    results[i]);
            }
#endif
            Channel = (true == results[0]) ? channels[0] : null;

            if (null != Channel)
            {
                Messages.Clear();
            }
        }

        public void OnUnsubscribed(string[] channels)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (channels.Length > 1)
            {
                Debug.LogWarningFormat("[{0}] User is currently subscribed to {1} channels." +
                                       "This is a bug, user should be subcribed to one channel only",
                                       this.GetType().Name,
                                       channels.Length);
            }

            foreach (string channel in channels)
            {
                Debug.LogFormat("[{0}] Unsubscribed from channel: {1}",
                                this.GetType().Name,
                                channel);
            }
#endif
        }

        public void OnUserSubscribed(string channel, string user) { }

        public void OnUserUnsubscribed(string channel, string user) { }

        //autoJoinLobby is set to true so this will be called when connection
        //to master server is established
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            Init();
        }

        public override void OnJoinedRoom()
        {
            if (null != Client)
            {
                base.OnJoinedRoom();
                //User should be subscribed only to one channel.
                //There should be only one chat per photon room.
                string[] channels = { PhotonNetwork.room.Name };
                Client.Subscribe(channels);
            }
        }

        public override void OnLeftRoom()
        {
            if (null != Client)
            {
                base.OnLeftRoom();
                var subscribedChannels = Client.PublicChannels;
                string[] channels = subscribedChannels.Keys.ToArray();
                Client.Unsubscribe(channels);
            }
        }
    }
}