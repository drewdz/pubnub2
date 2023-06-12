namespace PubnubMessaging.Model
{
    public class ConnectMessage : MessageBase
    {
        #region Constructors

        public ConnectMessage()
        {
            MessageType = typeof(ConnectMessage).FullName;
        }

        #endregion Constructors
    }
}
