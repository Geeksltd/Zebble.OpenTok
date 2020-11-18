namespace Zebble.Plugin
{
    public class OpenTokRole
    {
        public const string PUBLISHER = "Publisher";
        public const string SUBSCRIBER = "Subscriber";
        public const string MODERATOR = "Moderator";

        public static bool Validate(string role)
        {
            switch (role)
            {
                case PUBLISHER:
                case SUBSCRIBER:
                case MODERATOR:
                    return true;
                default:
                    return false;
            }
        }
    }
}