using PubnubMessaging.Model;

namespace InstaClient.Services
{
    public interface ICommsService
    {
        void Start(string clientId);

        void Stop();
    }
}