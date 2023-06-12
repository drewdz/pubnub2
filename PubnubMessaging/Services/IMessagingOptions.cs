namespace PubnubMessaging.Services
{
    public interface IMessagingOptions
    {
        string PublishKey { get; set; }

        string SubscribeKey { get; set; }

        bool SubscribeOnPresence { get; set; }
    }
}