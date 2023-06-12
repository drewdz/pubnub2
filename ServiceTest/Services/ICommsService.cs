namespace ServiceTest.Services
{
    public interface ICommsService
    {
        void Start(string clientId, string[] channels);

        void Stop();
    }
}
