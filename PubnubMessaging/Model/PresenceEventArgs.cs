using System;
using System.Collections.Generic;

namespace PubnubMessaging.Model
{
    public enum PresenceAction
    {
        Join,
        Leave,
        Timeout
    }

    public class PresenceEventArgs : EventArgs
    {
        public Dictionary<string, PresenceAction> Members { get; set; } = new Dictionary<string, PresenceAction> ();

        public int Count { get; set; } = 0;

        public long TimeToken { get; set; } = 0;

        public string Channel { get; set; } = string.Empty;
    }
}
