using System;

namespace PubnubMessaging.Model
{
    public class MessageBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string From { get; set; } = string.Empty;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;

        public DateTimeOffset Received { get; set; } = DateTimeOffset.Now;

        public string MessageType { get; set; } = string.Empty;
    }
}
