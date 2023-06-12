namespace PubnubMessaging.Model
{
    public interface IPresenceState
    {
        string FirstName { get; set; }

        string LastName { get; set; }

        string Role { get; set; }
    }
}
