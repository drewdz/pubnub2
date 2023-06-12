using PubnubMessaging.Model;
using Serilog;
using System;

namespace PubnubMessaging
{
    public sealed class MessageFactory : IMessageFactory
    {
        #region Fields

        private readonly ILogger _Logger;
        private readonly ISerializer _Serializer;

        #endregion Fields

        #region Constructors

        public MessageFactory(ILogger logger, ISerializer serializer)
            => (_Logger, _Serializer) = (logger, serializer);

        #endregion Constructors

        #region Operations

        public MessageBase GetMessage<TMessage>(string message)
            where TMessage : MessageBase
        {
            return _Serializer.Deserialize<TMessage>(message);
        }

        public MessageBase GetMessage(string message)
        {
            //  first deserialize to a base message to get the type
            var baseMessage = _Serializer.Deserialize<MessageBase>(message);
            var type = GetType().Assembly.GetType(baseMessage.MessageType);
            if (type == null) throw new ArgumentException($"Unable to resolve message from MessageType - {baseMessage.MessageType}, {message}");
            var msg = _Serializer.Deserialize(type, message) as MessageBase;
            return msg;
        }

        #endregion Operations
    }
}
