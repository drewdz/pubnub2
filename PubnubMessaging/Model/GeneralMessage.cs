namespace PubnubMessaging.Model
{
    public class GeneralMessage : MessageBase
    {
        #region Constructors

        public GeneralMessage()
        {
            MessageType = typeof(GeneralMessage).FullName;
        }

        #endregion Constructors

        #region Properties

        public string Text { get; set; } = string.Empty;

        #endregion Properties
    }
}
