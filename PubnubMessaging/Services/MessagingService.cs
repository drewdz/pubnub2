using PubnubMessaging.Model;
using PubnubApi;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PubnubMessaging.Services
{
    public class MessagingService : IMessagingService
    {
        #region Events

        public event EventHandler<MessageBase> MessageReceived;
        public event EventHandler<PresenceEventArgs> PresenceReceived;
        public event EventHandler StatusReceived;
        public event EventHandler<IPresenceState> PresenceStateReceived;

        #endregion Events

        #region Fields

        private readonly ILogger _Logger;
        private readonly IMessageFactory _MessageFactory;
        private readonly ISerializer _Serializer;
        private readonly IMessagingOptions _Options;

        private Pubnub _Pubnub;
        private SubscribeCallbackExt _SubscribeCallack;
        private SubscribeCallbackExt _SimpleCallbacks;
        private PNPublishResultExt _PublishResult;

        private string[] _Channels;

        #endregion Fields

        #region Constructors

        public MessagingService(ILogger logger, IMessagingOptions options, IMessageFactory messageFactory, ISerializer serializer)
        {
            _Logger = logger;
            _MessageFactory = messageFactory;
            _Serializer = serializer;
            _Options = options;
        }

        #endregion Constructors

        #region Initialization

        public void Connect(string clientId, string[] channels)
        {
            _Channels = channels;
            //  initialize pubnub
            PNConfiguration config = new PNConfiguration(new UserId(clientId));
            config.SubscribeKey = _Options.SubscribeKey;
            config.PublishKey = _Options.PublishKey;
            //  Uncomment to enable secure publishing. This may require other options to be configured on Pubnub.com
            //config.Secure = true;
            _Pubnub = new Pubnub(config);
            _SubscribeCallack = new SubscribeCallbackExt(OnMessageReceived, OnPresenceReceived, OnStatusReceived);
            _Pubnub.AddListener(_SubscribeCallack);
            _PublishResult = new PNPublishResultExt(OnPublished);

            //  subscribe to our "lobby" channel
            _Pubnub
                .Subscribe<string>()
                .Channels(_Channels)
                .WithPresence()
                .Execute();
        }

        public void Disconnect()
        {
            try
            {
                //  need to unsubscribe from channels
                _Pubnub.UnsubscribeAll<string>();
                _SubscribeCallack = null;
                _PublishResult = null;
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(Disconnect)} - Error disconnecting.\r\n{ex.Message}");
            }
        }

        #endregion Initialization

        #region Callbacks

        private void OnMessageReceived(Pubnub pubnub, PNMessageResult<object> result)
        {
            try
            {
                _Logger.Debug($"{nameof(MessagingService)}.{nameof(OnMessageReceived)} - Message received.");
                //  deserialize the message
                var message = _MessageFactory.GetMessage(result.Message.ToString());
                if (message == null)
                {
                    _Logger.Debug($"{nameof(MessagingService)}.{nameof(OnMessageReceived)} - Unable to deserialize message.\r\n{result.Message}");
                    return;
                }
                //  log some info
                _Logger.Information($"{nameof(MessagingService)}.{nameof(OnMessageReceived)} - Message received\r\nMessage ID: {message.Id}, Timestamp: {message.Timestamp}");
                MessageReceived?.Invoke(this, message);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(OnMessageReceived)} - Error receiving message.\r\n{ex.Message}");
            }
        }

        private void OnPresenceReceived(Pubnub pubnub, PNPresenceEventResult result)
        {
            _Logger.Debug($"{nameof(MessagingService)}.{nameof(OnPresenceReceived)} - Presence received.");
            if (PresenceReceived == null) return;

            var eventArgs = new PresenceEventArgs();
            //  find any new members
            if (result.Join?.Length > 0)
            {
                result.Join.ToList().ForEach(s => eventArgs.Members.Add(s, PresenceAction.Join));
            }
            if (result.Leave?.Length > 0)
            {
                result.Leave.ToList().ForEach(s => eventArgs.Members.Add(s, PresenceAction.Leave));
            }
            if (result.Timeout?.Length > 0)
            {
                result.Timeout.ToList().ForEach(s => eventArgs.Members.Add(s, PresenceAction.Join));
            }
            if (eventArgs.Members.Count > 0)
            {
                eventArgs.Count = result.Occupancy;
                eventArgs.TimeToken = result.Timetoken;
                eventArgs.Channel = result.Channel;
                //  send the presence info downstream
                PresenceReceived?.Invoke(this, eventArgs);
            }
            //  process presence state
            //  uncommenting this causes double state change events
            //if (result.Event.Equals("state-change"))
            //{
            //    ProcessPresenceState(result.Uuid, result.State);
            //}
        }

        private void OnStatusReceived(Pubnub pubnub, PNStatus status)
        {
            _Logger.Debug($"{nameof(MessagingService)}.{nameof(OnStatusReceived)} - Status received.");
            StatusReceived?.Invoke(this, new EventArgs());
        }

        private void OnPublished(PNPublishResult result, PNStatus status)
        {
            try
            {
                if (status.Error)
                {
                    _Logger.Information($"{nameof(MessagingService)}.{nameof(OnPublished)} - Error publishing message. {result}");
                }
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(OnPublished)} - Publish error.\r\n{ex.Message}");
            }
        }

        #endregion Callbacks

        #region Operations

        public void SendMessage(string to, MessageBase message)
        {
            _Logger.Debug($"{nameof(MessagingService)}.{nameof(SendMessage)} - Publishing message {message.Id}");
            try
            {
                _Pubnub.Publish()
                    .Channel(to)
                    .Message(_Serializer.Serialize(message))
                    .Execute(_PublishResult);
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(SendMessage)} - Error publishing message.\r\n{ex.Message}");
            }
        }

        public void SetPresenceState(IPresenceState state)
        {
            //  this may be a way to propogate additional meta data without having to generate a whole new message structure and flow
            //  this doesn't work in the current example since the user does not subscribe the the server's channel
            if ((_Channels == null) || (_Channels.Length == 0)) return;
            try
            {
                var dict = new Dictionary<string, object>();
                dict.Add("state", _Serializer.Serialize(state));

                _Logger.Information($"{nameof(MessagingService)}.{nameof(SetPresenceState)} - Preparing to set status");
                _Pubnub.SetPresenceState()
                    .Channels(_Channels)
                    .State(dict)
                    .Execute(new PNSetStateResultExt((r, s) =>
                    {
                        if (s.Error)
                        {
                            _Logger.Information($"{nameof(MessagingService)}.{nameof(SetPresenceState)} - Error setting state. {s.ErrorData.Information}");
                        }
                        else
                        {
                            _Logger.Information($"{nameof(MessagingService)}.{nameof(SetPresenceState)} - State set successfully.");
                        }
                    }));
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(SetPresenceState)} - Error setting state.\r\n{ex.Message}");
            }
        }

        public void GetPresenceState(string clientId)
        {
            _Logger.Debug($"{nameof(MessagingService)}.{nameof(GetPresenceState)} - Getting presence state for {clientId}.");
            try
            {
                _Pubnub.GetPresenceState()
                    .Channels(_Channels)
                    .Uuid(clientId)
                    .Execute(new PNGetStateResultExt((r, s) =>
                    {
                        if (s.Error)
                        {
                            _Logger.Debug($"{nameof(MessagingService)}.{nameof(GetPresenceState)} - Error getting presence state for {clientId}.");
                        }
                        else
                        {
                            ProcessPresenceState(clientId, r.StateByUUID);
                        }
                    }));
            }
            catch (Exception ex)
            {
                _Logger.Error(ex, $"{nameof(MessagingService)}.{nameof(GetPresenceState)} - Error getting presence state.\r\n{ex.Message}");
            }
        }

        private void ProcessPresenceState(string id, Dictionary<string, object> dict)
        {
            if ((dict == null) || !dict.ContainsKey("state"))
            {
                _Logger.Debug($"{nameof(MessagingService)}.{nameof(ProcessPresenceState)} - Empty presence state received for {id}.");
                return;
            }

            _Logger.Debug($"{nameof(MessagingService)}.{nameof(GetPresenceState)} - Presence state received for {id}.");
            var json = dict["state"] as string;
            if (string.IsNullOrEmpty(json)) return;
            PresenceStateReceived?.Invoke(this, _Serializer.Deserialize<PresenceState>(json));
        }

        #endregion Operations
    }
}
