using System;

namespace PubnubMessaging
{
    public interface ISerializer
    {
        string Serialize<TItem>(TItem item);

        TItem Deserialize<TItem>(string json);

        object Deserialize(Type type, string json);
    }
}