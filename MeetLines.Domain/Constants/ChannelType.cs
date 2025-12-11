namespace MeetLines.Domain.Constants
{
    public static class ChannelType
    {
        // Conversational (High Priority)
        public const string WhatsApp = "WhatsApp";
        public const string Instagram = "Instagram";
        public const string Messenger = "Messenger";
        public const string WebChat = "WebChat";

        // Notification (Medium Priority)
        public const string Sms = "SMS";
        public const string Email = "Email";

        // Social / Link (Low Priority)
        public const string TikTok = "TikTok";
        public const string LinkedIn = "LinkedIn";
        public const string YouTube = "YouTube";
        public const string ContactLink = "ContactLink";
    }
}
