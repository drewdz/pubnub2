using PubnubMessaging.Model;
using PubnubMessaging.Services;
using Serilog;
using System;
using System.Collections.Generic;

namespace ServiceTest.Services
{
    public class CommsService : ICommsService
    {
        #region Fields

        private readonly IMessagingService _MessagingService;
        private readonly ILogger _Logger;

        #endregion Fields

        #region Constructors

        public CommsService(ILogger logger, IMessagingService messagingService)
            => (_Logger, _MessagingService) = (logger, messagingService);

        #endregion Constructors

        #region Operations

        public void Start(string clientId, string[] channels)
        {
            _MessagingService.MessageReceived += MessageReceived;
            _MessagingService.PresenceReceived += PresenceReceived;
            _MessagingService.StatusReceived += StatusReceived;
            _MessagingService.PresenceStateReceived += PresenceStateReceived; ;
            _MessagingService.Connect(clientId, channels);
        }

        public void Stop()
        {
            _MessagingService.Disconnect();
            _MessagingService.MessageReceived -= MessageReceived;
            _MessagingService.PresenceReceived -= PresenceReceived;
            _MessagingService.PresenceStateReceived -= PresenceStateReceived; ;
            _MessagingService.StatusReceived -= StatusReceived;
        }

        private void ConnectNewUser(ConnectMessage message)
        {
            _Logger.Information($"{nameof(MessagingService)}.{nameof(ConnectNewUser)} - New presence. {message.From}");
            //  get presence state
            _MessagingService.GetPresenceState(message.From);
            //  send welcome
            var ack = new GeneralMessage { Text = $"Welcome to the channel \"{message.From}\"" };
            _MessagingService.SendMessage(message.From, ack);
        }

        private void Acknowledge(GeneralMessage message)
        {
            _Logger.Debug($"{nameof(MessagingService)}.{nameof(Acknowledge)} - Sending response for {message.Id}");
            var ack = new GeneralMessage { Text = $"Your message \"{message.Id}\", sent at \"{message.Timestamp}\" was received" };
            _MessagingService.SendMessage(message.From, ack);
        }

        #endregion Operations

        #region Event Handlers

        private void MessageReceived(object o, MessageBase message)
        {
            Console.WriteLine("New message received. Processing...");
            if (message is ConnectMessage)
            {
                Console.WriteLine($"Welcome new user - {message.From}");
                ConnectNewUser((ConnectMessage)message);
            }
            else if (!(message is GeneralMessage))
            {
                //  this should never really happen
                _Logger.Debug($"{nameof(MessagingService)}.{nameof(MessageReceived)} - Somehow we received a message we didn't recognize!\r\n{message}");
                Console.WriteLine($"Message type unknown - {message.GetType()}");
            }
            else
            {
                Acknowledge((GeneralMessage)message);
            }
        }

        private void PresenceReceived(object o, PresenceEventArgs e)
        {
            Console.WriteLine("Presence felt");
            foreach (var key in e.Members.Keys)
            {
                Console.WriteLine($"{key} - {e.Members[key]}");
            }
        }

        private void PresenceStateReceived(object sender, IPresenceState e)
        {
            if (e == null)
            {
                Console.WriteLine("Empty presence state received");
            }
            else
            {
                Console.WriteLine($"Presence state received:\r\n\tFirst Name: {e.FirstName}\r\n\tLastname: {e.LastName}\r\n\tRole: {e.Role}");
            }
        }

        private void StatusReceived(object o, EventArgs e)
        {
            Console.WriteLine("Status received. What happened...?");
        }

        #endregion Event Handlers
    }
}
