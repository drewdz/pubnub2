using PubnubMessaging.Model;

namespace InstaClient.Model
{
    public class User : IPresenceState
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }
    }
}
