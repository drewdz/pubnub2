using PubnubMessaging.Model;
using System;

namespace PubnubMessaging.Services
{
    public interface IMessagingService
    {
        #region Events

        event EventHandler<MessageBase> MessageReceived;
        event EventHandler<PresenceEventArgs> PresenceReceived;
        event EventHandler<IPresenceState> PresenceStateReceived;
        event EventHandler StatusReceived;

        #endregion Events

        #region Operations

        void SendMessage(string to, MessageBase message);

        void Connect(string clientId, string[] channels);

        void SetPresenceState(IPresenceState state);

        void GetPresenceState(string clientId);

        void Disconnect();

        #endregion Operations
    }
}