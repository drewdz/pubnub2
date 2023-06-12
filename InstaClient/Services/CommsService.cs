using PubnubMessaging.Model;
using System.Collections.Generic;
using System;
using PubnubMessaging.Services;
using Serilog;
using InstaClient.Model;

namespace InstaClient.Services
{
    public class CommsService : ICommsService
    {
        #region Fields

        private readonly IMessagingService _MessagingService;
        private readonly IStorageService _StorageService;

        private List<Guid> _Debounce = new List<Guid>();

        #endregion Fields

        #region Constructors

        public CommsService(IMessagingService messagingService, IStorageService storageService)
            => (_MessagingService, _StorageService) = (messagingService, storageService);

        #endregion Constructors

        #region Operations

        public void Start(string clientId)
        {
            /* Get the whole processing going, AKA "Do awesome stuff here"
             * For now this is just a console app showing a basic flow
             */

            var id = _StorageService.Get<string>(Constants.CLIENT_ID);
            if (string.IsNullOrEmpty(id)) id = Guid.NewGuid().ToString();

            Console.WriteLine($"Enter your client Id [{id}]");
            var newId = Console.ReadLine();
            if (string.IsNullOrEmpty(newId)) newId = id;
            _StorageService.Put(Constants.CLIENT_ID, newId);

            //  state
            Console.WriteLine("Enter your first name");
            var firstName = Console.ReadLine();
            Console.WriteLine("Enter your last name");
            var lastName = Console.ReadLine();
            Console.WriteLine("Enter your role");
            var role = Console.ReadLine();
            var user = new User { FirstName = firstName, LastName = lastName, Role = role };

            //  event handlers
            _MessagingService.MessageReceived += MessageReceived;
            _MessagingService.PresenceReceived += PresenceReceived;
            _MessagingService.StatusReceived += StatusReceived;
            _MessagingService.Connect(newId, new string[] { clientId, newId });

            //  set our presence state
            _MessagingService.SetPresenceState(user);

            Console.Clear();

            //  tell the server we're here
            Console.WriteLine("Announcing ourselves...");
            var msg = new ConnectMessage() { From = newId };
            _Debounce.Add(msg.Id);
            _MessagingService.SendMessage(clientId, msg);

            //  send message
            while (true)
            {
                Console.WriteLine("Enter message text to send (\"STOP\" to exit)");
                var text = Console.ReadLine();
                if (text.Equals("STOP")) break;
                var gmsg = new GeneralMessage() { From = newId, Text = text };
                _Debounce.Add(gmsg.Id);
                _MessagingService.SendMessage(clientId, gmsg);
            }
        }

        public void Stop()
        {
            _MessagingService.Disconnect();
            _MessagingService.MessageReceived -= MessageReceived;
            _MessagingService.PresenceReceived -= PresenceReceived;
            _MessagingService.StatusReceived -= StatusReceived;
        }

        #endregion Operations

        #region Event Handlers

        private void MessageReceived(object o, MessageBase message)
        {
            //  ignore message we send, only respond to messages from the server
            if (_Debounce.Contains(message.Id))
            {
                _Debounce.Remove(message.Id);
                return;
            }

            Console.WriteLine("New message received. Processing...");
            if (message is GeneralMessage)
            {
                var gmsg = (GeneralMessage)message;
                Console.WriteLine($"Message reads, \"{gmsg.Text}\"");
            }
            else if (message is ConnectMessage)
            {
                Console.WriteLine("Its a HelloMessage. Not sure why we're getting that - must be an echo in here.");
            }
            else
            {
                Console.WriteLine("Unrecognized message received.");
            }
        }

        private void PresenceReceived(object o, PresenceEventArgs e)
        {
            Console.WriteLine($"Presence felt: ");
        }

        private void StatusReceived(object o, EventArgs e)
        {
            Console.WriteLine("Status received. What happened...?");
        }

        #endregion Event Handlers
    }
}
