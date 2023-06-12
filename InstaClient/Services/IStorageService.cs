namespace InstaClient.Services
{
    public interface IStorageService
    {
        TItem Get<TItem>(string key);

        void Put<TItem>(string key, TItem item);
    }
}