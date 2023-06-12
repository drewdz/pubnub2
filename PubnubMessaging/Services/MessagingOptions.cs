namespace PubnubMessaging.Services
{
    public class MessagingOptions : IMessagingOptions
    {
        public string ClientId { get; set; } = string.Empty;

        public string PublishKey { get; set; } = string.Empty;

        public string SubscribeKey { get; set; } = string.Empty;

        public bool SubscribeOnPresence { get; set; } = false;

        public bool DebugLogs { get; set; } = false;
    }
}
