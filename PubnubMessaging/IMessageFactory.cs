using PubnubMessaging.Model;

namespace PubnubMessaging
{
    public interface IMessageFactory
    {
        MessageBase GetMessage<TMessage>(string message) where TMessage : MessageBase;

        MessageBase GetMessage(string message);
    }
}